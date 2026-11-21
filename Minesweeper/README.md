# Minesweeper

A deterministic, host-independent Minesweeper game domain for the SimpleDemos playground.

## Architecture

- `MinesweeperGame` owns all rules and mutable game state.
- `CellId` is the stable row/column identity shared by every manifestation.
- `CellSnapshot` is the read model consumed by renderers and hosts.
- Seeded and fixed layouts make behavior reproducible in tests and demonstrations.
- UI code must call domain actions; it must never implement a second copy of reveal, flood-fill, flag, or terminal-state rules.

The domain is a project inside this repository, not a NuGet package. A browser, WPF, SVG, or 3D host can reference it directly.

## Current capabilities

- Deterministic seeded board generation and injectable fixed layouts
- First-reveal safety
- Neighbor mine counts
- Flag toggling and reveal protection
- Zero-cell flood reveal
- Win/loss terminal states
- Reset to the original mine layout
- Immutable cell snapshots

## Run the tests

From the repository root:

```bash
dotnet test Minesweeper.Tests/Minesweeper.Tests.csproj
```

## Next manifestation

The next slice is a playable 2D board. It should consume `Snapshot()`, route every player action through `Reveal` or `ToggleFlag`, and expose status, remaining-safe count, and flag count without taking ownership of game rules.
