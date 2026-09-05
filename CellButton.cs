using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Minesweeper.Game;

namespace Minesweeper;

public class CellButton : Button
{
    public const int TileSize = 16;

    public int Row { get; }
    public int Column { get; }

    private readonly Image image;

    public CellButton(int row, int column)
    {
        Row = row;
        Column = column;

        Width = TileSize;
        Height = TileSize;
        Padding = new Thickness(0);
        BorderThickness = new Thickness(0);
        Background = Brushes.Transparent;
        Template = new ControlTemplate(typeof(Button))
        {
            VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
        };

        image = new Image { Width = TileSize, Height = TileSize, Stretch = Stretch.None };
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
        Content = image;
    }

    public void Render(Cell? cell, bool isTriggerMine)
    {
        if (cell is null || !cell.IsRevealed)
        {
            image.Source = new BitmapImage(Assets.Tile(cell?.Mark switch
            {
                CellMark.Flagged => "Flag",
                CellMark.Questioned => "QuestionMark",
                _ => "Tiles"
            }));
            return;
        }

        if (cell.IsMine)
        {
            var name = isTriggerMine ? "MinePressed"
                : cell.Mark == CellMark.Flagged ? "MineDisarmed"
                : "Mine";
            image.Source = new BitmapImage(Assets.Tile(name));
            return;
        }

        image.Source = cell.AdjacentMines == 0
            ? new BitmapImage(Assets.Tile("Clear"))
            : new BitmapImage(Assets.Mine(cell.AdjacentMines));
    }
}
