using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Minesweeper.Game;

namespace Minesweeper;

public partial class GameWindow : Window
{
    private readonly GameSettings settings;
    private readonly BoardGenerator generator = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DispatcherTimer faceRevertTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private CellButton[,] cellButtons = new CellButton[0, 0];
    private Board? board;
    private TimeSpan elapsed = TimeSpan.Zero;
    private bool gameOver;
    private (int Row, int Column)? triggerMine;
    private string faceState = "HappyFace";

    public GameWindow(GameSettings settings)
    {
        InitializeComponent();

        this.settings = settings;

        Title = "Minesweeper";
        Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Tiles/Mine.png", UriKind.Absolute));
        MinesLeftCounter.Display(this.settings.Mines);
        SetFace("HappyFace");

        MaxWidth = SystemParameters.WorkArea.Width;
        MaxHeight = SystemParameters.WorkArea.Height;

        timer.Tick += Timer_Tick;
        faceRevertTimer.Tick += (_, _) =>
        {
            faceRevertTimer.Stop();
            if (!gameOver) SetFace("HappyFace");
        };

        NewGameButton.PreviewMouseLeftButtonDown += (_, _) => FaceImage.Source = new BitmapImage(Assets.Face("HyppFacePressed"));
        NewGameButton.PreviewMouseLeftButtonUp += (_, _) => FaceImage.Source = new BitmapImage(Assets.Face(faceState));

        BuildBoard(settings.Rows, settings.Columns);

        // SizeToContent gives the correct natural size on first layout (Viewbox
        // reports the board's unscaled size when measured with infinite space), but
        // it would keep fighting manual resizes afterward, so switch to a fixed
        // starting size once that first layout has happened.
        Loaded += (_, _) =>
        {
            Width = ActualWidth / 2;
            Height = ActualHeight / 2;
            SizeToContent = SizeToContent.Manual;
        };
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        elapsed = elapsed.Add(TimeSpan.FromSeconds(1));
        TimerCounter.Display((int)elapsed.TotalSeconds);
    }

    private void BuildBoard(int rows, int columns)
    {
        BoardGrid.Children.Clear();
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();
        cellButtons = new CellButton[rows, columns];

        for (var row = 0; row < rows; row++)
        {
            BoardGrid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = new GridLength(CellButton.TileSize)
                }
            );
        }

        for (var column = 0; column < columns; column++)
        {
            BoardGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(CellButton.TileSize)
                }
            );
        }

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var cellRow = row;
                var cellColumn = column;
                var button = new CellButton(cellRow, cellColumn);
                button.Render(null, false);

                button.Click += (_, _) =>
                {
                    CellClicked(cellRow, cellColumn);
                };

                button.MouseRightButtonUp += (_, e) =>
                {
                    e.Handled = true;
                    CellRightClicked(cellRow, cellColumn);
                };

                Grid.SetRow(button, cellRow);
                Grid.SetColumn(button, cellColumn);
                BoardGrid.Children.Add(button);
                cellButtons[cellRow, cellColumn] = button;
            }
        }
    }

    private void CellClicked(int row, int column)
    {
        if (gameOver) return;

        if (board is null)
        {
            board = generator.Generate(settings, row, column);
            timer.Start();
        }

        if (board.Cells[row, column].IsRevealed)
        {
            board.ChordReveal(row, column);
        }
        else
        {
            board.Reveal(row, column);
        }

        RenderBoard();

        if (board.HasRevealedMine())
        {
            triggerMine = board.Cells[row, column].IsMine ? (row, column) : null;
            EndGame(won: false);
        }
        else if (board.IsWon())
        {
            EndGame(won: true);
        }
        else
        {
            ShowAmazedFaceBriefly();
        }
    }

    private void CellRightClicked(int row, int column)
    {
        if (gameOver || board is null) return;

        board.CycleMark(row, column);
        RenderBoard();
    }

    private void EndGame(bool won)
    {
        gameOver = true;
        timer.Stop();
        faceRevertTimer.Stop();

        if (!won) board!.RevealAllMines();
        RenderBoard();

        SetFace(won ? "WinFace" : "DeadFace");

        var dialog = new GameOverWindow(won, elapsed) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            StartNewGame();
        }
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e) => StartNewGame();

    private void StartNewGame()
    {
        Owner?.Show();
        Close();
    }

    private void SetFace(string name)
    {
        faceState = name;
        FaceImage.Source = new BitmapImage(Assets.Face(name));
    }

    private void ShowAmazedFaceBriefly()
    {
        SetFace("AmazedFace");
        faceRevertTimer.Stop();
        faceRevertTimer.Start();
    }

    private void RenderBoard()
    {
        if (board is null) return;

        for (var row = 0; row < board.Rows; row++)
        {
            for (var column = 0; column < board.Columns; column++)
            {
                cellButtons[row, column].Render(board.Cells[row, column], triggerMine == (row, column));
            }
        }

        MinesLeftCounter.Display(settings.Mines - board.FlaggedCellCount());
    }
}
