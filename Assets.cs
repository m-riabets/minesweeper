namespace Minesweeper;

public static class Assets
{
    public static Uri Tile(string name) => Pack($"Tiles/{name}");
    public static Uri Number(string name) => Pack($"Numbers/{name}");
    public static Uri Mine(int adjacentMines) => Pack($"Mines/{adjacentMines}");
    public static Uri Face(string name) => Pack($"Faces/{name}");

    private static Uri Pack(string relativePath) =>
        new($"pack://application:,,,/Assets/{relativePath}.png", UriKind.Absolute);
}
