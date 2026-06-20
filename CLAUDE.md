# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Dungeon Crawl** — a 2D turn-based roguelike in Godot 4.7 / C# (.NET 8).
Renderer: GL Compatibility. Physics: Jolt (unused in 2D gameplay).
Root namespace: `NewGameProject`.

## Build & Run

```powershell
# Build C# assemblies (validates compilation)
dotnet build "New Game Project.csproj"

# Run via Godot CLI (godot must be on PATH)
godot --path .
```

The `.godot/` directory is auto-generated — never edit manually.

## Architecture

### Scene flow
```
MainMenu.tscn → Game.tscn → GameOver.tscn
                    ↑ (Play Again)──────┘
```

All scene transitions use `GetTree().ChangeSceneToFile(...)`.

### Scene structure

**Game.tscn**
```
Node (root)
├── DungeonRenderer (Node2D)   ← draws dungeon + handles all input
└── CanvasLayer
    └── HUD (Control)          ← stats panel + message log overlay
```

**MainMenu.tscn / GameOver.tscn** — single Control node; UI built in code via `_Ready()`.

### Autoload singleton

`GameManager` (autoloaded as `GameManager`) holds all mutable game state and exposes it read-only. Scripts get the instance via `GameManager.Instance`. It emits `StateChanged` whenever turns resolve; `HUD` and `DungeonRenderer` subscribe to redraw.

### Data layer (pure C#, no Godot base classes)

| File | Purpose |
|------|---------|
| `Scripts/Core/DungeonData.cs` | `DungeonData` grid + `Room` |
| `Scripts/Core/DungeonGenerator.cs` | BSP room placement + L-corridor connection |
| `Scripts/Core/FovSystem.cs` | 360-ray shadowcast, radius 12 |
| `Scripts/Core/TurnManager.cs` | `TurnState` enum (PlayerTurn / EnemyTurn / GameOver) |
| `Scripts/Core/MessageLog.cs` | Capped list of coloured messages |
| `Scripts/Entities/EntityStats.cs` | HP, ATK, DEF, `TakeDamage`, `Heal` |
| `Scripts/Entities/PlayerData.cs` | XP, level-up, score tracking |
| `Scripts/Entities/EnemyData.cs` | Per-type stat tables + greedy one-step AI |
| `Scripts/Items/Item.cs` | Pickup effects (potions, weapons, scrolls) |

### Rendering

`DungeonRenderer._Draw()` iterates the 80×45 grid every redraw (called only on `StateChanged`). Tiles are 16×16 px; the full map = 1280×720 — exactly fills the viewport with no camera needed. FOV state drives three visual tiers: visible (lit), explored (dim), unseen (black). Glyphs are drawn with `DrawString(ThemeDB.FallbackFont, ...)`.

### Turn loop

1. Player presses a key → `DungeonRenderer._Input` → `GameManager.TryMovePlayer` / `TryWait`
2. GameManager resolves move/attack, then calls `ProcessEnemyTurns`
3. Enemies only act if inside `VisibleTiles` (aggro by line-of-sight)
4. `StateChanged` signal fires → `QueueRedraw` + HUD refresh

### Adding content

- **New enemy**: add to `EnemyType` enum; add entry in `EnemyData.MakeStats`, `DungeonRenderer.EnemyCol/EnemyGlyph`, and `DungeonGenerator.SpawnEntities`.
- **New item**: add to `ItemType`; handle in `Item.Apply`, `DungeonRenderer.ItemCol/ItemGlyph`.
- **New floor feature**: extend `TileType` + `DungeonRenderer._Draw` + generator.

## Controls

| Key | Action |
|-----|--------|
| Arrow / WASD / Numpad | Move / attack (bump) |
| Y U B N / Numpad diagonals | Diagonal move |
| `.` / Numpad 5 | Wait a turn |
| Esc | Return to main menu |
