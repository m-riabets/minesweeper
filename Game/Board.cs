namespace Minesweeper.Game;

public class Board
{
    public int Rows { get; }
    public int Columns { get; }
    public int Mines { get; }
    public Cell[,] Cells { get; }

    public Board(int rows, int columns, int mines)
    {
        Rows = rows;
        Columns = columns;
        Mines = mines;
        Cells = new Cell[rows, columns];

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                Cells[row, column] = new Cell();
            }
        }
    }

    public bool InBounds(int row, int column) =>
        row >= 0 && row < Rows && column >= 0 && column < Columns;

    public IEnumerable<(int Row, int Column)> Neighbors(int row, int column)
    {
        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (rowOffset == 0 && columnOffset == 0) continue;

                var neighborRow = row + rowOffset;
                var neighborColumn = column + columnOffset;

                if (InBounds(neighborRow, neighborColumn))
                {
                    yield return (neighborRow, neighborColumn);
                }
            }
        }
    }

    public void Reveal(int row, int column)
    {
        if (!InBounds(row, column)) return;

        var cell = Cells[row, column];
        if (cell.IsRevealed || cell.Mark == CellMark.Flagged) return;

        cell.IsRevealed = true;

        if (!cell.IsMine && cell.AdjacentMines == 0)
        {
            foreach (var (neighborRow, neighborColumn) in Neighbors(row, column))
            {
                Reveal(neighborRow, neighborColumn);
            }
        }
    }

    public void CycleMark(int row, int column)
    {
        if (!InBounds(row, column)) return;

        var cell = Cells[row, column];
        if (cell.IsRevealed) return;

        cell.Mark = cell.Mark switch
        {
            CellMark.None => CellMark.Flagged,
            CellMark.Flagged => CellMark.Questioned,
            _ => CellMark.None
        };
    }

    public void ChordReveal(int row, int column)
    {
        if (!InBounds(row, column)) return;

        var cell = Cells[row, column];
        // AdjacentMines is only meaningful for revealed, non-mine cells (see RecalculateAdjacency).
        if (!cell.IsRevealed || cell.IsMine) return;

        var neighbors = Neighbors(row, column).ToList();
        var flaggedNeighborCount = neighbors.Count(n => Cells[n.Row, n.Column].Mark == CellMark.Flagged);

        if (flaggedNeighborCount != cell.AdjacentMines) return;

        foreach (var (neighborRow, neighborColumn) in neighbors)
        {
            Reveal(neighborRow, neighborColumn);
        }
    }

    public int FlaggedCellCount()
    {
        var count = 0;

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (Cells[row, column].Mark == CellMark.Flagged) count++;
            }
        }

        return count;
    }

    public bool HasRevealedMine()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (Cells[row, column].IsMine && Cells[row, column].IsRevealed) return true;
            }
        }

        return false;
    }

    public bool IsWon()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (!Cells[row, column].IsMine && !Cells[row, column].IsRevealed) return false;
            }
        }

        return true;
    }

    public void RevealAllMines()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (Cells[row, column].IsMine) Cells[row, column].IsRevealed = true;
            }
        }
    }

    public void RecalculateAdjacency()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (Cells[row, column].IsMine) continue;

                Cells[row, column].AdjacentMines =
                    Neighbors(row, column).Count(n => Cells[n.Row, n.Column].IsMine);
            }
        }
    }
}
