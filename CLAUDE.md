# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Windows desktop Minesweeper built with WPF on .NET 10. `App.xaml` starts `MainWindow.xaml`
(difficulty-select screen), which opens `GameWindow.xaml` (the board) via
`MainWindow.StartGame`. The game is fully implemented: first-click-safe, no-guess board
generation, flagging/chording, a timer, win/lose detection, a custom-settings dialog, a
game-over dialog, and classic sprite-based visuals (see `Assets/`).

There is no test project or lint configuration checked in.

## Build and run

```bash
dotnet restore
dotnet build Minesweeper.sln -nologo
dotnet run --project Minesweeper.csproj
```

No `dotnet test` target exists yet. If a test project is added, keep it separate from the WPF app project (`Minesweeper.csproj`) rather than mixing test code into it.

`.github/workflows/build.yml` builds a self-contained, single-file `win-x64`
executable on push/PR to `main`, and publishes it to a GitHub Release on tags matching `v*`.

## Architecture

### Windows (root namespace, XAML + code-behind pairs)

- `App.xaml` / `App.xaml.cs`: WPF application bootstrap and startup URI. `App.xaml.Resources`
  holds app-wide implicit `Style`s (classic gray palette `#C0C0C0`, raised/sunken 3D-bevel
  `Window`/`Button` styling) that apply everywhere unless a window/control opts out
  (e.g. `CellButton` and `GameWindow`'s face button set their own `Template` locally).
- `MainWindow.xaml` / `MainWindow.xaml.cs`: difficulty-select screen. Builds difficulty
  buttons (Beginner 9x9/10 mines, Intermediate 16x16/40, Advanced 16x30/99) plus a Custom
  button that opens `CustomSettingsWindow`; each constructs a `GameSettings` and calls
  `StartGame`, which opens a `GameWindow` (`Owner = this`) and hides `MainWindow`.
- `CustomSettingsWindow.xaml` / `.xaml.cs`: dialog for a custom board (rows/columns/mines),
  validated via `GameSettings.TryCreate`. Returns a `GameSettings?` via `DialogResult`.
- `GameWindow.xaml` / `.xaml.cs`: the game board window. Content is wrapped in a `Viewbox`
  (uniform scaling, nearest-neighbor filtering) so the window is freely resizable despite
  the fixed-size pixel sprites. `BuildBoard` fills `BoardGrid` with one `CellButton` per
  cell (sized by `CellButton.TileSize`, 16px). The board isn't generated until the first
  click (`CellClicked` calls `BoardGenerator.Generate` lazily, then starts the timer).
  Tracks `gameOver`, the mine that ended the game (`triggerMine`, for the red "exploded"
  sprite), and drives the face (`SetFace`/`ShowAmazedFaceBriefly`) and both `DigitCounter`s
  (mines-left, elapsed-seconds timer). Ends the game via `EndGame`, which shows
  `GameOverWindow`.
- `GameOverWindow.xaml` / `.xaml.cs`: win/lose dialog showing elapsed time, with "New Game"
  (returns to `MainWindow`) and "Show Board" (just closes the dialog; the board stays
  rendered and inert since `gameOver` blocks further input) — `GameWindow` also has a
  persistent "New Game" face-button that does the same thing without needing the dialog.
- `CellButton.cs`: a chrome-free `Button` (custom `ControlTemplate`, no default WPF button
  styling) that renders one `Image` per `Cell`, mapping `Cell`/`CellMark` state to a sprite
  from `Assets/Tiles` or `Assets/Mines` (see `Assets.cs`).
- `DigitCounter.cs`: a `StackPanel` of three `Assets/Numbers` digit sprites, used for both
  the mines-left and timer counters (`Display(int)`, clamped to [-99, 999]).
- `Assets.cs`: pack-URI helpers (`Assets.Tile/Number/Mine/Face`) for `Assets/`.

### Gameplay logic (`Game/` namespace — pure .NET, no WPF types)

- `GameSettings.cs`: `record GameSettings(int Rows, int Columns, int Mines)` plus
  `MinRows`/`MaxRows`/`MinColumns`/`MaxColumns`/`MaxCells`/`MinMines` constants and
  `TryCreate(rows, columns, mines, out settings, out error)` validation (mines must leave
  at least one safe cell; board must not exceed `MaxCells`).
- `Cell.cs`: mutable cell state — `IsMine`, `IsRevealed`, `Mark` (`CellMark.None`/`Flagged`/
  `Questioned`), `AdjacentMines`.
- `Board.cs`: the grid (`Cell[,]`) plus gameplay operations — `Reveal` (flood-fills zero-
  adjacency cells), `ChordReveal`, `CycleMark`, `RecalculateAdjacency`, `HasRevealedMine`,
  `IsWon`, `RevealAllMines`, `FlaggedCellCount`.
- `BoardGenerator.cs`: builds a first-click-safe `Board` and retries mine placement
  (Fisher-Yates sample of eligible cells) up to a fixed attempt budget until
  `BoardSolver` confirms the layout is fully solvable by logic alone; falls back to the
  first candidate if no attempt qualifies within the budget. Shrinks the excluded safe
  zone from the full 3x3 neighborhood down to just the clicked cell if mine density is
  too high to otherwise fit (custom games can request very dense boards).
- `BoardSolver.cs`: simulates a logical player from the first click — single-point
  deduction, subset deduction, and (as a last resort, bitmask-optimized) frontier
  brute-force enumeration over the exposed unrevealed frontier, capped at
  `MaxFrontierSize` cells since it's exponential in frontier size.

### Assets

- `Assets/Tiles`, `Assets/Numbers`, `Assets/Mines`, `Assets/Faces`: fixed-size sprite sets
  (16x16 tiles/adjacency tiles, 13x23 digits, 24x24 faces) matching the classic Windows
  Minesweeper look. Loaded via pack URIs (`Assets.cs`), never resized on disk — resizing
  the window scales the rendered `Viewbox`, not the source images.

## Conventions

- Preserve the `Minesweeper` namespace and matching `x:Class` names in XAML/code-behind pairs; keep each XAML file paired with its code-behind.
- Windows-only WPF app — avoid cross-platform abstractions or new framework dependencies not already in use.
- Keep gameplay logic as simple .NET classes under `Game/` (no WPF types there) rather than introducing a heavier app architecture (MVVM frameworks, DI containers, etc.) prematurely.
