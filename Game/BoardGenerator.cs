namespace Minesweeper.Game;

/// <summary>
/// Builds a mined <see cref="Board"/> for the given settings, safe around the first
/// click, and retries mine placement until <see cref="BoardSolver"/> confirms the
/// board is fully solvable by logic alone (no guessing required).
/// </summary>
public class BoardGenerator
{
    private const int MaxAttempts = 200;

    private readonly Random random;

    public BoardGenerator(Random? random = null)
    {
        this.random = random ?? new Random();
    }

    public Board Generate(GameSettings settings, int firstClickRow, int firstClickColumn)
    {
        var safeCells = new HashSet<(int Row, int Column)>(
            new Board(settings.Rows, settings.Columns, settings.Mines).Neighbors(firstClickRow, firstClickColumn)
        )
        {
            (firstClickRow, firstClickColumn)
        };

        // High mine density can leave too few eligible cells outside the full 9-cell
        // neighborhood; shrink the safe zone to just the clicked cell rather than crash.
        var totalCells = settings.Rows * settings.Columns;
        if (totalCells - safeCells.Count < settings.Mines)
        {
            safeCells = new HashSet<(int Row, int Column)> { (firstClickRow, firstClickColumn) };
        }

        Board? firstCandidate = null;

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = PlaceMines(settings, safeCells);
            candidate.RecalculateAdjacency();
            firstCandidate ??= candidate;

            if (new BoardSolver(candidate).IsSolvableFrom(firstClickRow, firstClickColumn))
            {
                return candidate;
            }
        }

        // Could not find a no-guess layout within the attempt budget; fall back to the
        // first (still first-click-safe) candidate rather than failing to start the game.
        return firstCandidate!;
    }

    private Board PlaceMines(GameSettings settings, HashSet<(int Row, int Column)> safeCells)
    {
        var board = new Board(settings.Rows, settings.Columns, settings.Mines);

        var eligibleCells = new List<(int Row, int Column)>();
        for (var row = 0; row < settings.Rows; row++)
        {
            for (var column = 0; column < settings.Columns; column++)
            {
                if (!safeCells.Contains((row, column)))
                {
                    eligibleCells.Add((row, column));
                }
            }
        }

        // Fisher-Yates partial shuffle: a uniform sample of `Mines` cells without replacement.
        for (var i = 0; i < settings.Mines; i++)
        {
            var j = random.Next(i, eligibleCells.Count);
            (eligibleCells[i], eligibleCells[j]) = (eligibleCells[j], eligibleCells[i]);

            var (mineRow, mineColumn) = eligibleCells[i];
            board.Cells[mineRow, mineColumn].IsMine = true;
        }

        return board;
    }
}
