namespace Minesweeper.Game;

public record GameSettings(int Rows, int Columns, int Mines)
{
    public const int MinRows = 5;
    public const int MaxRows = 30;
    public const int MinColumns = 5;
    public const int MaxColumns = 30;
    public const int MaxCells = 480; // same ceiling as the built-in Advanced difficulty (16x30)
    public const int MinMines = 1;

    public static bool TryCreate(int rows, int columns, int mines, out GameSettings? settings, out string? error)
    {
        settings = null;

        if (rows < MinRows || rows > MaxRows)
        {
            error = $"Rows must be between {MinRows} and {MaxRows}.";
            return false;
        }

        if (columns < MinColumns || columns > MaxColumns)
        {
            error = $"Columns must be between {MinColumns} and {MaxColumns}.";
            return false;
        }

        var totalCells = rows * columns;
        if (totalCells > MaxCells)
        {
            error = $"Board is too large: {totalCells} cells exceeds the maximum of {MaxCells}.";
            return false;
        }

        if (mines < MinMines || mines > totalCells - 1)
        {
            error = $"Mines must be between {MinMines} and {totalCells - 1}.";
            return false;
        }

        error = null;
        settings = new GameSettings(rows, columns, mines);
        return true;
    }
}