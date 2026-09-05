namespace Minesweeper.Game;

/// <summary>
/// Simulates a logical player: reveals/flags cells using only deduction (no guessing)
/// starting from a given cell. Used by <see cref="BoardGenerator"/> to reject boards
/// that would require a guess.
/// </summary>
public class BoardSolver
{
    private const int MaxFrontierSize = 20;

    private readonly Board board;
    private readonly bool[,] revealed;
    private readonly bool[,] flagged;

    public BoardSolver(Board board)
    {
        this.board = board;
        revealed = new bool[board.Rows, board.Columns];
        flagged = new bool[board.Rows, board.Columns];
    }

    public bool IsSolvableFrom(int startRow, int startColumn)
    {
        RevealCascade(startRow, startColumn);

        bool progressed;
        do
        {
            progressed = ApplySinglePointDeduction()
                || ApplySubsetDeduction()
                || ApplyFrontierBruteForce();
        } while (progressed && !IsFullyRevealed());

        return IsFullyRevealed();
    }

    private bool IsFullyRevealed()
    {
        for (var row = 0; row < board.Rows; row++)
        {
            for (var column = 0; column < board.Columns; column++)
            {
                if (!board.Cells[row, column].IsMine && !revealed[row, column])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void RevealCascade(int row, int column)
    {
        if (!board.InBounds(row, column) || revealed[row, column]) return;

        revealed[row, column] = true;

        if (board.Cells[row, column].AdjacentMines == 0)
        {
            foreach (var (neighborRow, neighborColumn) in board.Neighbors(row, column))
            {
                RevealCascade(neighborRow, neighborColumn);
            }
        }
    }

    private bool ApplySinglePointDeduction()
    {
        var changed = false;

        for (var row = 0; row < board.Rows; row++)
        {
            for (var column = 0; column < board.Columns; column++)
            {
                if (!revealed[row, column]) continue;

                var hidden = board.Neighbors(row, column)
                    .Where(n => !revealed[n.Row, n.Column])
                    .ToList();
                if (hidden.Count == 0) continue;

                var flaggedCount = hidden.Count(n => flagged[n.Row, n.Column]);
                var number = board.Cells[row, column].AdjacentMines;

                if (number == hidden.Count)
                {
                    foreach (var (r, c) in hidden)
                    {
                        if (flagged[r, c]) continue;
                        flagged[r, c] = true;
                        changed = true;
                    }
                }
                else if (number == flaggedCount)
                {
                    foreach (var (r, c) in hidden)
                    {
                        if (flagged[r, c] || revealed[r, c]) continue;
                        RevealCascade(r, c);
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    private record Constraint(HashSet<(int Row, int Column)> Cells, int MineCount);

    private List<Constraint> BuildConstraints()
    {
        var constraints = new List<Constraint>();

        for (var row = 0; row < board.Rows; row++)
        {
            for (var column = 0; column < board.Columns; column++)
            {
                if (!revealed[row, column]) continue;

                var neighbors = board.Neighbors(row, column).ToList();
                var hidden = neighbors
                    .Where(n => !revealed[n.Row, n.Column] && !flagged[n.Row, n.Column])
                    .ToHashSet();
                if (hidden.Count == 0) continue;

                var remainingMines = board.Cells[row, column].AdjacentMines
                    - neighbors.Count(n => flagged[n.Row, n.Column]);

                constraints.Add(new Constraint(hidden, remainingMines));
            }
        }

        return constraints;
    }

    // Subset rule: if constraint A's cells are a strict subset of constraint B's cells,
    // the cells in (B - A) contain exactly (B.MineCount - A.MineCount) mines.
    private bool ApplySubsetDeduction()
    {
        var changed = false;
        var constraints = BuildConstraints();

        foreach (var a in constraints)
        {
            foreach (var b in constraints)
            {
                if (a == b || a.Cells.Count >= b.Cells.Count || !a.Cells.IsSubsetOf(b.Cells)) continue;

                var diffCells = b.Cells.Except(a.Cells).ToList();
                var diffMines = b.MineCount - a.MineCount;

                if (diffMines == 0)
                {
                    foreach (var (r, c) in diffCells)
                    {
                        if (revealed[r, c]) continue;
                        RevealCascade(r, c);
                        changed = true;
                    }
                }
                else if (diffMines == diffCells.Count)
                {
                    foreach (var (r, c) in diffCells)
                    {
                        if (flagged[r, c]) continue;
                        flagged[r, c] = true;
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    // Last resort: enumerate every mine/safe assignment consistent with the active
    // constraints and keep whichever cells come out the same way in every valid
    // assignment. Capped by MaxFrontierSize since this is exponential in frontier size;
    // a real implementation would split the frontier into connected components first.
    private bool ApplyFrontierBruteForce()
    {
        var constraints = BuildConstraints();
        if (constraints.Count == 0) return false;

        var frontier = constraints.SelectMany(c => c.Cells).Distinct().ToList();
        if (frontier.Count == 0 || frontier.Count > MaxFrontierSize) return false;

        // Precompute each cell's bit position and each constraint as a bitmask once,
        // so checking an assignment against a constraint is a single AND + popcount
        // instead of an IndexOf lookup per cell per assignment (this loop already runs
        // up to 2^MaxFrontierSize times, so avoiding per-iteration allocation/lookup
        // work here matters a lot).
        var cellIndex = new Dictionary<(int Row, int Column), int>(frontier.Count);
        for (var i = 0; i < frontier.Count; i++) cellIndex[frontier[i]] = i;

        var constraintMasks = new (int Mask, int MineCount)[constraints.Count];
        for (var i = 0; i < constraints.Count; i++)
        {
            var mask = 0;
            foreach (var cell in constraints[i].Cells) mask |= 1 << cellIndex[cell];
            constraintMasks[i] = (mask, constraints[i].MineCount);
        }

        var fullMask = frontier.Count == 32 ? -1 : (1 << frontier.Count) - 1;
        var neverMineMask = fullMask;
        var alwaysMineMask = fullMask;
        var foundValidAssignment = false;

        var assignmentCount = 1 << frontier.Count;
        for (var mask = 0; mask < assignmentCount; mask++)
        {
            var satisfiesAll = true;
            foreach (var (constraintMask, mineCount) in constraintMasks)
            {
                if (System.Numerics.BitOperations.PopCount((uint)(mask & constraintMask)) != mineCount)
                {
                    satisfiesAll = false;
                    break;
                }
            }
            if (!satisfiesAll) continue;

            foundValidAssignment = true;
            neverMineMask &= ~mask;
            alwaysMineMask &= mask;
        }

        if (!foundValidAssignment) return false;

        var changed = false;
        for (var i = 0; i < frontier.Count; i++)
        {
            var (r, c) = frontier[i];
            var bit = 1 << i;

            if ((neverMineMask & bit) != 0 && !revealed[r, c])
            {
                RevealCascade(r, c);
                changed = true;
            }
            else if ((alwaysMineMask & bit) != 0 && !flagged[r, c])
            {
                flagged[r, c] = true;
                changed = true;
            }
        }

        return changed;
    }
}
