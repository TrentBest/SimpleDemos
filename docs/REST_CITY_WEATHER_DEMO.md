<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated workshop moniker" width="720" />
  </a>
</p>

# REST city-weather demonstration

This demonstration uses the free, no-key <a href="https://open-meteo.com/en/docs">Open-Meteo forecast API</a> for a fixed Tacoma, Washington coordinate. One HTTP response becomes one immutable weather snapshot.

## Three independently buildable paths

| Project | Dependency boundary | What it demonstrates |
| --- | --- | --- |
| RestCityDemo.Visualize | Shared REST contract + Visualize | REST snapshot becomes a Visualize SVG. No Renderer or GUI reference. |
| RestCityDemo.Renderer | Shared REST contract + Renderer + GUI.Core | Renderer selects a representation and GUI.Core builds a textual semantic tree. No Visualize reference. |
| RestCityDemo | Shared REST contract + both presentation paths | One snapshot feeds both the Renderer/GUI and Visualize paths. |

`RestCityDemo.Shared` owns the single source definition of `WeatherSnapshot` and `OpenMeteoClient`. The independent hosts reference that project rather than duplicating transport or data contracts. The combined and WPF hosts are integration hosts and intentionally include both presentation dependency sets.

## Run

The repository's sibling source checkouts are expected in `_dependencies/Visualize`, `_dependencies/Renderer`, and `_dependencies/GUI`. CI checks out those development branches before restoring and building.

From the repository root:

    dotnet run --project RestCityDemo.Visualize/RestCityDemo.Visualize.csproj -- --output artifacts/tacoma-weather-visualize.svg
    dotnet run --project RestCityDemo.Renderer/RestCityDemo.Renderer.csproj
    dotnet run --project RestCityDemo/RestCityDemo.csproj -- --mode combined

On Windows, launch the native GUI host:

    dotnet run --project RestCityDemo.Wpf/RestCityDemo.Wpf.csproj -- --mode renderer
    dotnet run --project RestCityDemo.Wpf/RestCityDemo.Wpf.csproj -- --mode combined

The combined mode writes `artifacts/tacoma-weather.svg` by default. Use `--output path/to/file.svg` to choose another destination.

For an offline deterministic smoke run, add `--fixture`. The fixture is explicitly labelled as test data and is not represented as a live observation.

## Boundaries and evidence

- The REST client calls Open-Meteo's `/v1/forecast` endpoint with latitude/longitude, current weather fields, and the `America/Los_Angeles` time zone. No API key is required. The source URL, returned coordinates, observation time, and units are included in the data view.
- HTTP failure, timeout, cancellation, and malformed response remain failures; a successful HTTP status is not treated as proof that every expected field exists.
- Renderer currently provides the FSM-backed representation decision (Semantic through Interactive). It does not itself serialize a textual weather report. This sample maps that selected representation into a neutral GUI.Core tree.
- GUI.Core defines the semantic tree; GUI.WPF manifests it. The console text-tree writer is a host-side diagnostic view, not a new GUI adapter.
- Visualize's `SvgVisualizerBackend` creates a static SVG artifact on the CPU. It is not a production GPU renderer, a live window surface, or proof of GPU execution.
- CI uses a deterministic offline fixture. It proves project builds and path execution without depending on the live API. A live Open-Meteo call and interactive WPF launch remain manual environment checks.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
