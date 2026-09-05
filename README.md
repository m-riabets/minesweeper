# Minesweeper

A classic Minesweeper built with WPF on .NET 10, with a no-guess board generator and
sprite-based visuals matching the original Windows game.

## Features

- **Difficulty selection**: Beginner (9x9/10 mines), Intermediate (16x16/40), Advanced
  (16x30/99), or a Custom board (validated rows/columns/mine count).
- **First-click-safe, no-guess generation**: mines are placed only after the first
  click, and the generator retries placements until the resulting board is fully
  solvable by logic alone (single-point deduction, subset deduction, and frontier
  brute-force elimination) — no guessing required to win.
- **Full input model**: left-click to reveal, left-click a revealed number to "chord"
  (auto-reveal its unflagged neighbors once enough are flagged), right-click to cycle a
  cell through flag → question mark → clear.
- **Timer and mine counter**, win/lose detection, and a game-over dialog showing time
  spent with options to start a new game or inspect the finished board.
- **Classic sprite visuals** (`Assets/`) for tiles, revealed-number tiles, the
  smiley/face button (reacts to reveals, wins, and losses), and digit-sprite counters,
  rendered inside a resizable, uniformly-scaled window.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/) with the WPF workload (Windows only —
  the app targets `net10.0-windows` and uses WPF, which does not run on macOS/Linux).

## Build and run

```bash
dotnet restore
dotnet build Minesweeper.sln -nologo
dotnet run --project Minesweeper.csproj
```

## Continuous integration

`.github/workflows/build.yml` builds a self-contained, single-file `win-x64`
executable on every push/PR to `main` (available as a workflow artifact), and
publishes it to a GitHub Release automatically when a tag matching `v*` is pushed.

## Project layout

See [CLAUDE.md](CLAUDE.md) for a detailed breakdown of the architecture and
conventions used in this codebase.
