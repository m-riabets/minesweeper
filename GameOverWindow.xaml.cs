using System.Windows;
using System.Windows.Media.Imaging;

namespace Minesweeper;

public partial class GameOverWindow : Window
{
    public GameOverWindow(bool won, TimeSpan elapsed)
    {
        InitializeComponent();

        Title = "Minesweeper";
        Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Tiles/Mine.png", UriKind.Absolute));

        MessageText.Text = won ? "You Win!" : "You Lose!";
        TimeText.Text = $"Time: {elapsed:hh\\:mm\\:ss}";
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void ShowBoardButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
