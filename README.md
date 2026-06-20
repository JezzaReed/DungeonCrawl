# DungeonCrawl

A 2D dungeon-crawl roguelike built in **Godot 4.7** with **C# / .NET 8**. Procedurally generated dungeons, turn-based combat, permadeath, and pixel-art SVG tiles.

## Download

**[⬇ Download the latest Windows build](https://github.com/JezzaReed/DungeonCrawl/releases/download/latest/DungeonCrawl-windows.zip)**

Extract the zip and run `DungeonCrawl.exe`. No installer required.

> The build is produced automatically from the latest push to `main`, so this link always serves the most recent version.
>
> This repository is currently **private**, so downloading requires being signed in to GitHub with access to the repo.

## Features

- Procedural BSP dungeon generation with connected rooms and corridors
- Turn-based movement and combat (hold a direction to move repeatedly)
- Permadeath — a run ends for good when you die
- Five enemy types (Goblin, Skeleton, Orc, Troll, Dragon), scaling by floor
- Items: health potions, swords, shields, and scrolls
- Player levelling with XP, plus a stats / message HUD
- Camera that scrolls with the player and fills the available window

## Controls

| Action | Keys |
|--------|------|
| Move (4-way) | Arrow keys / WASD / numpad |
| Move (diagonals) | Y U B N / numpad 7 9 1 3 |
| Wait a turn | `.` / numpad 5 |
| Return to menu | Esc |

Hold a movement key to keep moving.

## Building from source

Requires the Godot 4.7 **.NET (mono)** editor and the .NET 8 SDK.

```sh
dotnet build "New Game Project.sln"
```

Then open the project in Godot and press **F5**, or export via **Project → Export → Windows Desktop**.

## Deployment

Pushing to `main` triggers [`.github/workflows/build-windows.yml`](.github/workflows/build-windows.yml), which exports a Windows build, zips it, uploads it as a workflow artifact, and updates the rolling `latest` release linked above.
