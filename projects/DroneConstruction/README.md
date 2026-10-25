# Drone Construction — a living proof

**Watch the real simulation, not just a concept picture.** This project runs the deterministic Drone Construction simulation, captures committed snapshots, and packages those snapshots as an animated, self-contained HTML artifact.

## Try it now

1. Open the latest [Drone Construction showcase workflow runs](https://github.com/TrentBest/SimpleDemos/actions/workflows/drone-construction-showcase.yml).
2. Choose the latest successful run.
3. Under **Artifacts**, download `drone-construction-browser-showcase`.
4. Extract the HTML file and open it in a modern browser.

The workflow checks out the current `development` source of [Experiences](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/tree/development/experiences/DroneConstruction), runs its real `ConstructionSimulation`, checks that the generated HTML is non-empty and contains the run summary, and uploads the artifact. The page is generated from captured simulation snapshots; it is not a prerecorded storyboard.

## What this small run demonstrates

- Three construction agents share a finite block supply.
- The simulation advances in discrete ticks and places three required blocks.
- Captured snapshots are projected into an isometric SVG view and replayed in the browser.
- The run is bounded and deterministic, making it suitable as an inspectable first proof.

## Reproduce locally

Requires the .NET 8 SDK. Clone [TheSingularityWorkshop.Experiences](https://github.com/TrentBest/TheSingularityWorkshop.Experiences) and run from its repository root:

```sh
dotnet run --project samples/DroneConstructionShowcase/DroneConstructionShowcase.csproj --configuration Release -- ./artifacts/drone-construction-showcase.html
```

Open `artifacts/drone-construction-showcase.html` in your browser.

## Architecture and limits

- **Experiences** owns authoritative simulation state and immutable snapshots.
- **Renderer** owns observer-relative representation policy.
- **Visualize / GUI.SVG** provide the downstream frame and presentation path.

The browser artifact currently proves the small simulation-to-SVG replay path. The separate end-to-end Renderer → Visualize → GUI.SVG integration proof is available in [Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/tree/development/integrations/RendererAdapter/DroneConstructionProof), but it is not yet the same as a hosted, continuously updating observer-relative application.

This is **not** evidence of 100,000 construction agents, starvation-free routing, a production GPU path, or a deployed live web application. The current scoped readiness estimate for the broader 100,000-agent showcase is **60%**. We will raise that estimate only as those milestones are implemented and verified.

## Evidence

- [Experiences implementation and gallery](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/tree/development/experiences/DroneConstruction)
- [Drone Construction integration proof](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/tree/development/integrations/RendererAdapter/DroneConstructionProof)
- [100k scale validation protocol](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/blob/development/experiences/DroneConstruction/design/100K_SCALE_VALIDATION.md)
- [Latest generated artifact workflow](https://github.com/TrentBest/SimpleDemos/actions/workflows/drone-construction-showcase.yml)
