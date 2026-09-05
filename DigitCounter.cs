using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minesweeper;

public class DigitCounter : StackPanel
{
    private readonly Image[] digitImages;

    public DigitCounter()
    {
        Orientation = Orientation.Horizontal;

        digitImages = new Image[3];
        for (var i = 0; i < digitImages.Length; i++)
        {
            var image = new Image { Width = 13, Height = 23, Stretch = Stretch.None };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            digitImages[i] = image;
            Children.Add(image);
        }

        Display(0);
    }

    public void Display(int value)
    {
        value = Math.Clamp(value, -99, 999);

        var glyphs = (value < 0
            ? new[] { "Minus" }.Concat(Digits(Math.Abs(value), 2))
            : Digits(value, 3)).ToArray();

        for (var i = 0; i < digitImages.Length; i++)
        {
            digitImages[i].Source = new BitmapImage(Assets.Number(glyphs[i]));
        }
    }

    private static IEnumerable<string> Digits(int value, int width) =>
        value.ToString().PadLeft(width, '0').Select(c => c.ToString());
}
