using System.Windows;

namespace TheSingularityWorkshop.SimpleDemos.Minesweeper.Wpf;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var application = new Application();
        application.Run(new MainWindow());
    }
}
