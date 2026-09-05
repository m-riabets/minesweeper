namespace Minesweeper.Game;

public enum CellMark
{
    None,
    Flagged,
    Questioned
}

public class Cell
{
    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public CellMark Mark { get; set; } = CellMark.None;
    public int AdjacentMines { get; set; }
}
