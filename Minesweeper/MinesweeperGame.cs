namespace TheSingularityWorkshop.SimpleDemos.Minesweeper;

/// <summary>Stable identity of a cell in a Minesweeper board.</summary>
public readonly record struct CellId(int Row, int Column)
{
    public override string ToString() => $"{Row}:{Column}";
}

/// <summary>Public state of one cell; mine placement is intentionally hidden until revealed.</summary>
public readonly record struct CellSnapshot(CellId Id, bool IsRevealed, bool IsFlagged, int AdjacentMines, bool IsMine);

public enum MinesweeperStatus { Ready, Playing, Won, Lost }

public enum MinesweeperActionResult
{
    Accepted,
    AlreadyRevealed,
    AlreadyFlagged,
    RejectedOutOfBounds,
    RejectedTerminal
}

/// <summary>
/// Authoritative, renderer-independent Minesweeper rules. A seed or fixed mine layout
/// makes board generation reproducible; the first accepted reveal is always safe.
/// </summary>
public sealed class MinesweeperGame
{
    private readonly bool[,] _mines;
    private readonly bool[,] _revealed;
    private readonly bool[,] _flagged;
    private readonly int[,] _adjacent;
    private readonly bool[,] _initialMines;
    private readonly int _mineCount;
    private bool _firstReveal = true;

    public int Width { get; }
    public int Height { get; }
    public int MineCount => _mineCount;
    public int FlagCount { get; private set; }
    public int RevealedSafeCount { get; private set; }
    public int RemainingSafeCount => Width * Height - _mineCount - RevealedSafeCount;
    public MinesweeperStatus Status { get; private set; } = MinesweeperStatus.Ready;

    public MinesweeperGame(int width, int height, int mineCount, int seed)
        : this(width, height, CreateSeededLayout(width, height, mineCount, seed), seed) { }

    public MinesweeperGame(int width, int height, IEnumerable<CellId> mineLocations, int seed = 0)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        ArgumentNullException.ThrowIfNull(mineLocations);

        Width = width;
        Height = height;
        _mines = new bool[height, width];
        _revealed = new bool[height, width];
        _flagged = new bool[height, width];
        _adjacent = new int[height, width];

        foreach (var cell in mineLocations)
        {
            ValidateCell(cell);
            if (_mines[cell.Row, cell.Column])
                throw new ArgumentException($"Duplicate mine location: {cell}.", nameof(mineLocations));
            _mines[cell.Row, cell.Column] = true;
        }

        _initialMines = (bool[,])_mines.Clone();
        _mineCount = CountMines();
        if (_mineCount >= width * height)
            throw new ArgumentException("At least one safe cell is required.", nameof(mineLocations));
        RecalculateNeighbors();
    }

    public MinesweeperActionResult Reveal(CellId cell)
    {
        if (!IsInside(cell)) return MinesweeperActionResult.RejectedOutOfBounds;
        if (IsTerminal()) return MinesweeperActionResult.RejectedTerminal;
        if (_flagged[cell.Row, cell.Column]) return MinesweeperActionResult.AlreadyFlagged;
        if (_revealed[cell.Row, cell.Column]) return MinesweeperActionResult.AlreadyRevealed;

        if (_firstReveal)
        {
            EnsureFirstRevealSafe(cell);
            _firstReveal = false;
        }

        Status = MinesweeperStatus.Playing;
        if (_mines[cell.Row, cell.Column])
        {
            _revealed[cell.Row, cell.Column] = true;
            Status = MinesweeperStatus.Lost;
            return MinesweeperActionResult.Accepted;
        }

        FloodReveal(cell);
        if (RemainingSafeCount == 0) Status = MinesweeperStatus.Won;
        return MinesweeperActionResult.Accepted;
    }

    public MinesweeperActionResult ToggleFlag(CellId cell)
    {
        if (!IsInside(cell)) return MinesweeperActionResult.RejectedOutOfBounds;
        if (IsTerminal()) return MinesweeperActionResult.RejectedTerminal;
        if (_revealed[cell.Row, cell.Column]) return MinesweeperActionResult.AlreadyRevealed;
        _flagged[cell.Row, cell.Column] = !_flagged[cell.Row, cell.Column];
        FlagCount += _flagged[cell.Row, cell.Column] ? 1 : -1;
        return MinesweeperActionResult.Accepted;
    }

    public CellSnapshot GetCell(CellId cell)
    {
        ValidateCell(cell);
        var isRevealed = _revealed[cell.Row, cell.Column];
        return new CellSnapshot(cell, isRevealed, _flagged[cell.Row, cell.Column],
            isRevealed ? _adjacent[cell.Row, cell.Column] : 0, isRevealed && _mines[cell.Row, cell.Column]);
    }

    public IReadOnlyList<CellSnapshot> Snapshot()
    {
        var cells = new CellSnapshot[Width * Height];
        var index = 0;
        for (var row = 0; row < Height; row++)
        for (var column = 0; column < Width; column++)
        {
            var id = new CellId(row, column);
            cells[index++] = GetCell(id);
        }
        return cells;
    }

    public void Reset()
    {
        Array.Clear(_revealed);
        Array.Clear(_flagged);
        Array.Copy(_initialMines, _mines, _mines.Length);
        FlagCount = 0;
        RevealedSafeCount = 0;
        Status = MinesweeperStatus.Ready;
        _firstReveal = true;
        RecalculateNeighbors();
    }

    private void FloodReveal(CellId start)
    {
        var pending = new Queue<CellId>();
        pending.Enqueue(start);
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            if (!IsInside(current) || _revealed[current.Row, current.Column] ||
                _flagged[current.Row, current.Column] || _mines[current.Row, current.Column]) continue;

            _revealed[current.Row, current.Column] = true;
            RevealedSafeCount++;
            if (_adjacent[current.Row, current.Column] != 0) continue;
            foreach (var neighbor in Neighbors(current))
                if (!_revealed[neighbor.Row, neighbor.Column] && !_flagged[neighbor.Row, neighbor.Column])
                    pending.Enqueue(neighbor);
        }
    }

    private void EnsureFirstRevealSafe(CellId first)
    {
        if (!_mines[first.Row, first.Column]) return;
        _mines[first.Row, first.Column] = false;
        var replacement = Enumerable.Range(0, Height)
            .SelectMany(row => Enumerable.Range(0, Width).Select(column => new CellId(row, column)))
            .FirstOrDefault(candidate => candidate != first && !_mines[candidate.Row, candidate.Column]);
        _mines[replacement.Row, replacement.Column] = true;
        RecalculateNeighbors();
    }

    private void RecalculateNeighbors()
    {
        Array.Clear(_adjacent);
        for (var row = 0; row < Height; row++)
        for (var column = 0; column < Width; column++)
            _adjacent[row, column] = Neighbors(new CellId(row, column)).Count(n => _mines[n.Row, n.Column]);
    }

    private IEnumerable<CellId> Neighbors(CellId cell)
    {
        for (var row = Math.Max(0, cell.Row - 1); row <= Math.Min(Height - 1, cell.Row + 1); row++)
        for (var column = Math.Max(0, cell.Column - 1); column <= Math.Min(Width - 1, cell.Column + 1); column++)
            if (row != cell.Row || column != cell.Column) yield return new CellId(row, column);
    }

    private static IEnumerable<CellId> CreateSeededLayout(int width, int height, int mineCount, int seed)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (mineCount < 1 || mineCount >= width * height) throw new ArgumentOutOfRangeException(nameof(mineCount));
        return Enumerable.Range(0, width * height)
            .ToArray()
            .Shuffle(new Random(seed), mineCount, width);
    }

    private int CountMines()
    {
        var count = 0;
        foreach (var mine in _mines) if (mine) count++;
        return count;
    }

    private bool IsTerminal() => Status is MinesweeperStatus.Won or MinesweeperStatus.Lost;
    private bool IsInside(CellId cell) => cell.Row >= 0 && cell.Row < Height && cell.Column >= 0 && cell.Column < Width;
    private void ValidateCell(CellId cell)
    {
        if (!IsInside(cell)) throw new ArgumentOutOfRangeException(nameof(cell), cell, "Cell is outside the board.");
    }
}

internal static class MinesweeperEnumerableExtensions
{
    public static IEnumerable<CellId> Shuffle(this int[] indexes, Random random, int mineCount, int width)
    {
        for (var i = indexes.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
        }
        return indexes.Take(mineCount).Select(index => new CellId(index / width, index % width));
    }
}
