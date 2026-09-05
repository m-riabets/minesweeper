using System.Windows;
using Minesweeper.Game;

namespace Minesweeper;

public partial class CustomSettingsWindow : Window
{
    public GameSettings? Result { get; private set; }

    public CustomSettingsWindow()
    {
        InitializeComponent();

        Title = "Minesweeper";
        Icon = Assets.AppIcon;

        RowsTextBox.Text = "16";
        ColumnsTextBox.Text = "16";
        MinesTextBox.Text = "40";
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(RowsTextBox.Text, out var rows) ||
            !int.TryParse(ColumnsTextBox.Text, out var columns) ||
            !int.TryParse(MinesTextBox.Text, out var mines))
        {
            ErrorText.Text = "Rows, columns, and mines must be whole numbers.";
            return;
        }

        if (!GameSettings.TryCreate(rows, columns, mines, out var settings, out var error))
        {
            ErrorText.Text = error;
            return;
        }

        Result = settings;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
