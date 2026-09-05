# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Windows desktop Minesweeper built with WPF on .NET 10. `App.xaml` starts `MainWindow.xaml` (difficulty-select screen), which opens `GameWindow.xaml` (the board) via `MainWindow.StartGame`. The project is early-stage: the board grid renders but click handling (`GameWindow.CellClicked`) and the custom-settings dialog (`MainWindow.ShowCustomSettingsDialog`) are still stubs with no logic.

There is no test project or lint configuration checked in.

## Build and run

```bash
dotnet restore
dotnet build Minesweeper.sln -nologo
dotnet run --project Minesweeper.csproj
```

No `dotnet test` target exists yet. If a test project is added, keep it separate from the WPF app project (`Minesweeper.csproj`) rather than mixing test code into it.

## Architecture

- `App.xaml` / `App.xaml.cs`: WPF application bootstrap and startup URI.
- `MainWindow.xaml` / `MainWindow.xaml.cs`: difficulty-select screen. Builds difficulty buttons (Beginner 9x9/10 mines, Intermediate 16x16/40, Advanced 16x30/99) plus a Custom button, each constructing a `GameSettings` and calling `StartGame`, which opens a new `GameWindow` and hides `MainWindow`.
- `GameWindow.xaml` / `GameWindow.xaml.cs`: the game board window. `BuildBoard` procedurally fills a `Grid` (`BoardGrid`) with one `Button` per cell sized by `tileSize` (16px); window size is derived from rows/columns and capped at 900px.
- `Game/GameSettings.cs`: `namespace Minesweeper.Game` — currently just the `GameSettings(int Rows, int Columns, int Mines)` record. Put board/gameplay logic here as it's built out, not in the window code-behind.
- `Assets/`: tile/number/face PNGs (Minesweeper-classic sprite set) under `Faces/`, `Mines/`, `Numbers/`, `Tiles/`. Not yet wired into rendering — `GameWindow` currently uses plain `Button`s with no sprites.
- `Minesweeper.csproj`: `net10.0-windows`, `UseWPF=true`, `Nullable=enable`, `ImplicitUsings=enable`.

## Conventions

- Preserve the `Minesweeper` namespace and matching `x:Class` names in XAML/code-behind pairs; keep each XAML file paired with its code-behind.
- Windows-only WPF app — avoid cross-platform abstractions or new framework dependencies not already in use.
- Keep gameplay logic as simple .NET classes (e.g. under `Game/`) rather than introducing a heavier app architecture (MVVM frameworks, DI containers, etc.) prematurely.
