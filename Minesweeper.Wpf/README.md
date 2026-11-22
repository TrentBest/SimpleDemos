# Minesweeper — WPF playable demo

This is the first interactive manifestation of the deterministic Minesweeper domain.

## Run

On Windows with the .NET 10 SDK:

```powershell
dotnet run --project Minesweeper.Wpf/Minesweeper.Wpf.csproj
```

## Controls

- Left-click a covered cell to reveal it.
- Right-click a covered cell to toggle a flag.
- Select **New board** to start the next deterministic seeded layout.

The WPF project references `Minesweeper/Minesweeper.Domain.csproj`. It contains presentation and input wiring only: mine placement, first-reveal safety, flood reveal, flags, win/loss state, and counters remain owned by `MinesweeperGame`.

The board uses a fixed initial seed and increments that seed for each new board, making each scenario reproducible. This host intentionally uses ordinary WPF controls as a small first playable proof; it does not introduce a NuGet dependency or duplicate Renderer/Visualization game rules.
