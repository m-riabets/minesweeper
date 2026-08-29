using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Minesweeper.Game;

namespace Minesweeper;

public partial class GameWindow : Window
{
    private readonly GameSettings settings;
    private readonly UInt16 tileSize = 16;

    public GameWindow(GameSettings settings)
    {
        InitializeComponent();

        this.settings = settings;

        Name = "Minesweeper";
        Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Tiles/Mine.png", UriKind.Absolute));
        MinesLeftText.Text = this.settings.Mines.ToString();

        MaxWidth = SystemParameters.WorkArea.Width;
        MaxHeight = SystemParameters.WorkArea.Height;

        BuildBoard(settings.Rows, settings.Columns);
    }

    private void BuildBoard(int rows, int columns)
    {
        BoardGrid.Children.Clear();
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();

        for (var row = 0; row < rows; row++)
        {
            BoardGrid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = new GridLength(tileSize)
                }
            );
        }

        for (var column = 0; column < columns; column++)
        {
            BoardGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(tileSize)
                }
            );
        }

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                Button button = new Button();

                button.Click += (_, _) =>
                {
                    CellClicked();
                };
                
                Grid.SetRow(button, row);
                Grid.SetColumn(button, column);
                BoardGrid.Children.Add(button);
            }
        }
    }

    private void CellClicked()
    {
        
    }
}