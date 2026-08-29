using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Minesweeper.Game;

namespace Minesweeper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Name = "Minesweeper";
        Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Tiles/Mine.png", UriKind.Absolute));
        CreateDifficultyButtons();
    }

    private void CreateDifficultyButtons()
    {
        AddDifficultyButton("Beginner", new GameSettings(9, 9, 10));
        AddDifficultyButton("Intermediate", new GameSettings(16, 16, 40));
        AddDifficultyButton("Advanced", new GameSettings(16, 30, 99));
        
        AddCustomButton();
    }

    private void AddDifficultyButton(string name, GameSettings settings)
    {
        var button = new Button
        {
            Content = name,
            Width = 100,
            Height = 50,
            Margin =  new Thickness(5),
        };
        
        button.Click += (_, _) => StartGame(settings);
        DifficultyPanel.Children.Add(button);
    }

    private void AddCustomButton()
    {
        var button = new Button
        {
            Content = "Custom",
            Width = 100,
            Height = 50,
            Margin = new Thickness(5),
        };

        button.Click += (_, _) =>
        {
            var settings = ShowCustomSettingsDialog();

            if (settings is not null)
            {
                StartGame(settings);
            }
        };
        
        DifficultyPanel.Children.Add(button);
    }

    private void StartGame(GameSettings settings)
    {
        var gameWindow = new GameWindow(settings);
        gameWindow.Show();
        this.Hide();
    }

    private GameSettings? ShowCustomSettingsDialog()
    {
        return null;
    }
}