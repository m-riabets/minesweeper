using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minesweeper;

public static class Assets
{
    public static Uri Tile(string name) => Pack($"Tiles/{name}");
    public static Uri Number(string name) => Pack($"Numbers/{name}");
    public static Uri Mine(int adjacentMines) => Pack($"Mines/{adjacentMines}");
    public static Uri Face(string name) => Pack($"Faces/{name}");

    // BitmapImage only decodes the .ico's first (smallest) frame, which Windows then
    // has to blurrily upscale for the taskbar/Alt-Tab icon; picking the largest frame
    // ourselves gives Windows a high-res source to downscale from instead.
    public static readonly ImageSource AppIcon = LoadLargestIconFrame();

    private static ImageSource LoadLargestIconFrame()
    {
        var decoder = BitmapDecoder.Create(
            new Uri("pack://application:,,,/Assets/AppIcon.ico", UriKind.Absolute),
            BitmapCreateOptions.None,
            BitmapCacheOption.OnLoad
        );

        return decoder.Frames.OrderByDescending(frame => frame.PixelWidth).First();
    }

    private static Uri Pack(string relativePath) =>
        new($"pack://application:,,,/Assets/{relativePath}.png", UriKind.Absolute);
}
