using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TheSingularityWorkshop.SimpleDemos.Minesweeper;

namespace TheSingularityWorkshop.SimpleDemos.Minesweeper.Wpf;

public partial class MainWindow : Window
{
    private const int BoardWidth = 9;
    private const int BoardHeight = 9;
    private const int MineTotal = 10;

    private MinesweeperGame _game;
    private int _seed = 20261122;
    private readonly Button[,] _cells = new Button[BoardHeight, BoardWidth];

    public MainWindow()
    {
        InitializeComponent();
        _game = CreateGame();
        BuildBoard();
        RefreshBoard();
    }

    private MinesweeperGame CreateGame() => new(BoardWidth, BoardHeight, MineTotal, _seed);

    private void BuildBoard()
    {
        for (var row = 0; row < BoardHeight; row++)
        for (var column = 0; column < BoardWidth; column++)
        {
            var cell = new CellId(row, column);
            var button = new Button
            {
                Tag = cell,
                Margin = new Thickness(2),
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                Focusable = true,
                ToolTip = $"Cell {row + 1}, {column + 1}"
            };
            button.Click += Cell_Click;
            button.PreviewMouseRightButtonUp += Cell_RightClick;
            _cells[row, column] = button;
            Board.Children.Add(button);
        }
    }

    private void Cell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: CellId cell }) return;
        _game.Reveal(cell);
        RefreshBoard();
    }

    private void Cell_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { Tag: CellId cell }) return;
        _game.ToggleFlag(cell);
        RefreshBoard();
        e.Handled = true;
    }

    private void NewBoard_Click(object sender, RoutedEventArgs e)
    {
        _seed++;
        _game = CreateGame();
        RefreshBoard();
    }

    private void RefreshBoard()
    {
        foreach (var snapshot in _game.Snapshot())
        {
            var button = _cells[snapshot.Id.Row, snapshot.Id.Column];
            button.IsEnabled = _game.Status is not (MinesweeperStatus.Won or MinesweeperStatus.Lost)
                               && !snapshot.IsRevealed;
            button.Content = snapshot.IsFlagged ? "⚑"
                : snapshot.IsMine ? "✹"
                : snapshot.IsRevealed && snapshot.AdjacentMines > 0 ? snapshot.AdjacentMines.ToString()
                : string.Empty;

            button.Background = snapshot.IsRevealed
                ? new SolidColorBrush(Color.FromRgb(203, 213, 225))
                : new SolidColorBrush(Color.FromRgb(51, 65, 85));
            button.Foreground = snapshot.IsMine ? Brushes.DarkRed
                : NumberBrush(snapshot.AdjacentMines);
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105));
            button.ToolTip = snapshot.IsRevealed
                ? $"Cell {snapshot.Id.Row + 1}, {snapshot.Id.Column + 1}: {snapshot.AdjacentMines} adjacent mines"
                : $"Cell {snapshot.Id.Row + 1}, {snapshot.Id.Column + 1}: {(snapshot.IsFlagged ? "flagged" : "hidden")}";
        }

        MineCountText.Text = MineTotal.ToString("00");
        FlagCountText.Text = _game.FlagCount.ToString("00");
        StatusText.Text = _game.Status switch
        {
            MinesweeperStatus.Ready => "Left-click to reveal · Right-click to flag",
            MinesweeperStatus.Playing => $"Safe cells remaining: {_game.RemainingSafeCount} · Left-click to reveal · Right-click to flag",
            MinesweeperStatus.Won => "Board cleared! You found every safe cell. Start a new board to play again.",
            MinesweeperStatus.Lost => "Boom. You hit a mine. Start a new board and try another deterministic layout.",
            _ => string.Empty
        };
        StatusText.Foreground = _game.Status switch
        {
            MinesweeperStatus.Won => Brushes.LightGreen,
            MinesweeperStatus.Lost => Brushes.Salmon,
            _ => new SolidColorBrush(Color.FromRgb(203, 213, 225))
        };
    }

    private static Brush NumberBrush(int number) => number switch
    {
        1 => Brushes.DodgerBlue,
        2 => Brushes.ForestGreen,
        3 => Brushes.IndianRed,
        4 => Brushes.MidnightBlue,
        5 => Brushes.Maroon,
        6 => Brushes.Teal,
        7 => Brushes.Black,
        8 => Brushes.DimGray,
        _ => Brushes.White
    };
}
