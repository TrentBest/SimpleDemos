using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using TheSingularityWorkshop.Physics.Hydrodynamics;
using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

const string physicsRevision = "b6e54def8d070832f8b88202e07487b4a55daa90";
const string rendererRevision = "03d5b77e6af71a4d21d24debf5a8f39012b080ae";
const string visualizeRevision = "ee7a0fc41eb0a3a78042b79fab72aed79f6e5d04";
const string guiRevision = "0ad4bde2bd7eab8ab5ba7264811f3c8c26d07e6";
var simpleDemosRevision = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local-working-tree";

const int width = 16;
const int height = 12;
const double cellWidth = 1.5d;
const double cellHeight = 1.5d;
const double timeStep = 0.01d;
const int steps = 300;
const double verticalExaggeration = 2500d;
const double stillWaterSurface = 0.14d;
const double baseRainRate = 0.000005d;
const double peakRainRate = 0.00002d;

var output = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("artifacts/water-surface.svg");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
var first = RunScenario();
var replay = RunScenario();
Require(first.Depth.SequenceEqual(replay.Depth), "Deterministic replay produced different final depth values.");
Require(first.Bed.SequenceEqual(replay.Bed), "Deterministic replay produced different bed values.");
Require(first.Volume.Equals(replay.Volume), "Deterministic replay produced a different water volume.");
Require(first.StepCount == steps && replay.StepCount == steps, "Unexpected committed Physics step count.");
Require(first.SimulationTime.Equals(steps * timeStep), "Simulation time does not match committed steps.");
Require(first.ExpectedRainVolumeDelta >= 0d, "Expected rainfall volume must be non-negative.");
Require(Math.Abs((first.Volume - first.InitialVolume) - first.ExpectedRainVolumeDelta) <= 1e-9,
    "Water-volume accounting does not match applied rainfall within 1e-9 m³.");

var vertices = BuildSurfaceVertices(first);
var indices = BuildGridIndices(width, height);
var geometry = new VisualizerGeometry(1, vertices, indices);
var material = new VisualizerMaterial(1, new Vector4(0.18f, 0.62f, 0.82f, 1f));
const ulong geometryId = 4101;
const ulong materialId = 4102;
const ulong entityId = 4103;
var catalog = new VisualizerResourceCatalog();
catalog.SetGeometry(new VisualizerResourceId(geometryId), geometry);
catalog.SetMaterial(new VisualizerResourceId(materialId), material);

var command = new RenderDrawCommand(
    new RenderResourceId(geometryId),
    new RenderResourceId(materialId),
    Matrix4x4.Identity,
    indices.Length,
    0,
    0,
    RendererRepresentation.Near,
    entity: new RenderEntityId(entityId));
var frame = new RenderFrame(
    new RenderView(Vector3.Zero, Matrix4x4.Identity, Matrix4x4.Identity),
    [command]);
var visualizerFrame = new RendererFrameAdapter().Adapt(frame);

await using var visualizer = new Visualizer(
    new GuiSvgVisualizerBackend(
        output,
        projectWithCamera: false,
        sceneLabel: "Height-field visualization of a depth-averaged 2D shallow-water model",
        drawGrid: false),
    catalog);
var renderResult = await visualizer.RenderAsync(visualizerFrame);
Require(renderResult.SubmittedDrawCount == 1, "Expected exactly one submitted surface draw.");
Require(File.Exists(output) && new FileInfo(output).Length > 0, "GUI.SVG did not produce a non-empty artifact.");
var svg = await File.ReadAllTextAsync(output);
Require(svg.Contains("<svg", StringComparison.Ordinal), "Output is not an SVG document.");
Require(svg.Contains("draw-", StringComparison.Ordinal), "SVG contains no indexed surface triangles.");

var manifestPath = Path.ChangeExtension(output, ".manifest.txt");
var manifest = new SortedDictionary<string, string>(StringComparer.Ordinal)
{
    ["backend"] = "CPU/SVG",
    ["cellHeightMetres"] = F(cellHeight),
    ["cellWidthMetres"] = F(cellWidth),
    ["depthArraySha256"] = HashDoubles(first.Depth),
    ["drawCommands"] = frame.Commands.Count.ToString(CultureInfo.InvariantCulture),
    ["entityId"] = entityId.ToString(CultureInfo.InvariantCulture),
    ["expectedRainVolumeDeltaCubicMetres"] = F(first.ExpectedRainVolumeDelta),
    ["finalWaterVolumeCubicMetres"] = F(first.Volume),
    ["geometryId"] = geometryId.ToString(CultureInfo.InvariantCulture),
    ["geometryIndices"] = indices.Length.ToString(CultureInfo.InvariantCulture),
    ["geometryVertices"] = vertices.Length.ToString(CultureInfo.InvariantCulture),
    ["guiRevision"] = guiRevision,
    ["heightCells"] = height.ToString(CultureInfo.InvariantCulture),
    ["initialWaterVolumeCubicMetres"] = F(first.InitialVolume),
    ["maxDepthMetres"] = F(first.Depth.Max()),
    ["maxFreeSurfaceMetres"] = F(first.FreeSurface.Max()),
    ["maxSpeedMetresPerSecond"] = F(first.Speed.Max()),
    ["model"] = "BathymetricShallowWaterBasin2D",
    ["modelDescription"] = "Depth-averaged 2D shallow-water model over fixed bathymetry; not a full 3D fluid solver",
    ["physicsRevision"] = physicsRevision,
    ["rainfallPeakRateMetresWaterEquivalentPerSecond"] = F(peakRainRate),
    ["rainfallBaseRateMetresWaterEquivalentPerSecond"] = F(baseRainRate),
    ["rainfallApplication"] = "ApplyRainfall(rateField, timeStep) once per simulation step, followed by Advance()",
    ["rendererRevision"] = rendererRevision,
    ["renderedSvg"] = Path.GetFileName(output),
    ["simpleDemosRevision"] = simpleDemosRevision,
    ["simulationSteps"] = steps.ToString(CultureInfo.InvariantCulture),
    ["simulationTimeSeconds"] = F(first.SimulationTime),
    ["timeStepSeconds"] = F(timeStep),
    ["verticalExaggeration"] = F(verticalExaggeration),
    ["visualizeRevision"] = visualizeRevision,
    ["waterVolumeDeltaCubicMetres"] = F(first.Volume - first.InitialVolume),
    ["widthCells"] = width.ToString(CultureInfo.InvariantCulture)
};
await File.WriteAllLinesAsync(manifestPath, manifest.Select(pair => $"{pair.Key}={pair.Value}"), new UTF8Encoding(false));

Console.WriteLine("Deterministic Physics rainfall -> bathymetric basin -> Renderer -> Visualize -> GUI.SVG");
Console.WriteLine($"Steps: {first.StepCount}; simulation time: {F(first.SimulationTime)} s");
Console.WriteLine($"Water volume: {F(first.InitialVolume)} -> {F(first.Volume)} m³");
Console.WriteLine($"Rainfall accounting delta: {F(first.ExpectedRainVolumeDelta)} m³; observed delta: {F(first.Volume - first.InitialVolume)} m³");
Console.WriteLine($"Surface: {width} × {height} cells; {vertices.Length} vertices; {indices.Length} indices");
Console.WriteLine($"SVG: {output}");
Console.WriteLine($"Manifest: {manifestPath}");
Console.WriteLine("Backend: CPU/SVG; height-field visualization, not GPU rendering or a full 3D fluid simulation.");

(double[] Bed, double[] Depth, double[] FreeSurface, double[] Speed, double InitialVolume,
    double Volume, double ExpectedRainVolumeDelta, long StepCount, double SimulationTime) RunScenario()
{
    var count = checked(width * height);
    var bed = new double[count];
    var initialDepth = new double[count];
    var rainRates = new double[count];
    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
    {
        var i = y * width + x;
        bed[i] = -0.025d
            + 0.012d * Math.Sin(x * 0.43d)
            + 0.009d * Math.Cos(y * 0.37d)
            + 0.004d * Math.Sin((x + y) * 0.29d);
        initialDepth[i] = stillWaterSurface - bed[i];
        var dx = (x - (width - 1) / 2d) / (width / 2d);
        var dy = (y - (height - 1) / 2d) / (height / 2d);
        var centralWeight = Math.Max(0d, 1d - Math.Sqrt(dx * dx + dy * dy));
        rainRates[i] = baseRainRate + peakRainRate * centralWeight;
    }

    var scenario = new BasinScenario(width, height, cellWidth, cellHeight, timeStep, 0d, 0d,
        physicsRevision);
    var basin = new BathymetricShallowWaterBasin2D(
        scenario.Width, scenario.Height, scenario.CellWidth, scenario.CellHeight, scenario.TimeStep,
        initialDepth, bed);
    var rainfall = new RainfallRateField2D(width, height, rainRates);
    var initialVolume = basin.WaterVolume;
    var expectedRainVolumeDelta = rainRates.Sum() * timeStep * width * height * cellWidth * cellHeight * steps;

    for (var step = 0; step < steps; step++)
    {
        basin.ApplyRainfall(rainfall, timeStep);
        basin.Advance();
    }

    var finalBed = basin.SnapshotBedElevation();
    var finalDepth = basin.SnapshotDepth();
    var freeSurface = new double[count];
    var speed = new double[count];
    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
    {
        var i = y * width + x;
        freeSurface[i] = basin.SampleFreeSurface(x, y);
        var vx = basin.SampleVelocityX(x, y);
        var vy = basin.SampleVelocityY(x, y);
        speed[i] = Math.Sqrt(vx * vx + vy * vy);
    }

    ValidateSnapshot(scenario, basin.Width, basin.Height, finalBed, finalDepth, freeSurface, speed);
    return (finalBed, finalDepth, freeSurface, speed, initialVolume, basin.WaterVolume,
        expectedRainVolumeDelta, basin.StepCount, basin.SimulationTime);
}

void ValidateSnapshot(BasinScenario scenario, int basinWidth, int basinHeight,
    double[] bed, double[] depth, double[] freeSurface, double[] speed)
{
    Require(scenario.Width == basinWidth && scenario.Height == basinHeight,
        "Scenario dimensions do not match the authoritative basin.");
    Require(double.IsFinite(scenario.CellWidth) && scenario.CellWidth > 0d &&
        double.IsFinite(scenario.CellHeight) && scenario.CellHeight > 0d &&
        double.IsFinite(scenario.TimeStep) && scenario.TimeStep > 0d &&
        double.IsFinite(scenario.OriginX) && double.IsFinite(scenario.OriginY),
        "Scenario cell spacing must be finite and positive.");
    var expectedCount = checked(scenario.Width * scenario.Height);
    Require(bed.Length == expectedCount && depth.Length == expectedCount &&
        freeSurface.Length == expectedCount && speed.Length == expectedCount,
        "Physics snapshot arrays do not match the scenario dimensions.");
    Require(bed.All(double.IsFinite) && depth.All(value => double.IsFinite(value) && value >= 0d) &&
        freeSurface.All(double.IsFinite) && speed.All(value => double.IsFinite(value) && value >= 0d),
        "Physics snapshot contains invalid numeric values.");
    Require(!string.IsNullOrWhiteSpace(scenario.PhysicsRevision), "Scenario provenance is missing the Physics revision.");
}

VisualizerVertex[] BuildSurfaceVertices(
    (double[] Bed, double[] Depth, double[] FreeSurface, double[] Speed, double InitialVolume,
        double Volume, double ExpectedRainVolumeDelta, long StepCount, double SimulationTime) state)
{
    var vertices = new VisualizerVertex[width * height];
    var baseline = state.FreeSurface.Min();
    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
    {
        var i = y * width + x;
        var u = x / (float)(width - 1);
        var v = y / (float)(height - 1);
        var relief = (float)((state.FreeSurface[i] - baseline) * verticalExaggeration);
        var screenX = -0.82f + u * 1.48f + v * 0.22f;
        var screenY = -0.48f + v * 0.86f + u * 0.12f + relief;
        vertices[i] = new VisualizerVertex(
            new Vector3(screenX, screenY, 0f),
            Vector3.UnitZ,
            new Vector2(u, v),
            Vector4.One);
    }
    return vertices;
}

uint[] BuildGridIndices(int gridWidth, int gridHeight)
{
    var indices = new List<uint>((gridWidth - 1) * (gridHeight - 1) * 6);
    for (var y = 0; y < gridHeight - 1; y++)
    for (var x = 0; x < gridWidth - 1; x++)
    {
        var topLeft = (uint)(y * gridWidth + x);
        var topRight = topLeft + 1;
        var bottomLeft = topLeft + (uint)gridWidth;
        var bottomRight = bottomLeft + 1;
        indices.AddRange([topLeft, bottomLeft, topRight, topRight, bottomLeft, bottomRight]);
    }
    return indices.ToArray();
}

string HashDoubles(IEnumerable<double> values)
{
    var bytes = values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
    return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

string F(double value) => value.ToString("R", CultureInfo.InvariantCulture);
void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed record BasinScenario(int Width, int Height, double CellWidth, double CellHeight,
    double TimeStep, double OriginX, double OriginY, string PhysicsRevision);
