<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated workshop moniker" width="720" />
  </a>
</p>

# REST city-weather demonstration

This demonstration uses the free, no-key <a href="https://open-meteo.com/en/docs">Open-Meteo forecast API</a> for a fixed Tacoma, Washington coordinate. One HTTP response becomes one immutable weather snapshot. The sample separates transport, observer-aware representation, GUI semantics, and visual presentation.

## Three selectable paths

| Mode | What runs | What it proves |
| --- | --- | --- |
| visualize | Open-Meteo → snapshot → Visualize | Visualize creates a visual artifact directly from REST data; this path does not invoke Renderer. |
| renderer | Open-Meteo → snapshot → Renderer FSM → GUI.Core tree | Renderer selects a computational representation; GUI.Core expresses the textual observation and GUI.WPF manifests it as native controls. |
| combined | One snapshot → both paths | The same response feeds the textual GUI and Visualize artifact. |

The console host prints the GUI semantic tree as text. The WPF host uses the existing GUI.WPF adapter to display that tree as native controls.

## Run

The repository's sibling source checkouts are expected in _dependencies/Visualize, _dependencies/Renderer, and _dependencies/GUI. CI checks out those exact development branches before restoring and building.

From the repository root:

    dotnet run --project RestCityDemo/RestCityDemo.csproj -- --mode visualize
    dotnet run --project RestCityDemo/RestCityDemo.csproj -- --mode renderer
    dotnet run --project RestCityDemo/RestCityDemo.csproj -- --mode combined

On Windows, launch the native GUI host:

    dotnet run --project RestCityDemo.Wpf/RestCityDemo.Wpf.csproj -- --mode renderer
    dotnet run --project RestCityDemo.Wpf/RestCityDemo.Wpf.csproj -- --mode combined

The combined mode writes artifacts/tacoma-weather.svg by default. Use --output path/to/file.svg to choose another destination.

For an offline deterministic smoke run, add --fixture. The fixture is explicitly labelled as test data and is not represented as a live observation.

## Boundaries and evidence

- The REST client calls Open-Meteo's /v1/forecast endpoint with latitude/longitude, current weather fields, and the America/Los_Angeles time zone. No API key is required. The source URL, returned coordinates, observation time, and units are included in the data view.
- HTTP failure, timeout, cancellation, and malformed response remain failures; a successful HTTP status is not treated as proof that every expected field exists.
- Renderer currently provides the FSM-backed representation decision (Semantic through Interactive). It does not itself serialize a textual weather report. This sample maps that selected representation into a neutral GUI.Core tree.
- GUI.Core defines the semantic tree; GUI.WPF manifests it. The console text-tree writer is a host-side diagnostic view, not a new GUI adapter.
- Visualize's SvgVisualizerBackend creates a static SVG artifact on the CPU. It is not a production GPU renderer, a live window surface, or proof of GPU execution.
- The bar chart uses temperature, relative humidity, and wind speed from the same immutable snapshot. It is an explanatory visualization, not a meteorological forecast or a claim of calibrated chart axes.

## Architecture

    Open-Meteo REST
          |
          v
    WeatherSnapshot (one immutable response)
          |
          +------------------------------+
          |                              |
          v                              v
    RendererComputationMachine      VisualizerFrame
          |                              |
          v                              v
    RepresentationTarget            Visualizer
          |                              |
          v                              v
    GUI.Core GuiNode tree           SvgVisualizerBackend
          |                              |
          v                              v
    GUI.WPF native controls          Static SVG artifact

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
