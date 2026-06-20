# Dungeon Crawl — Texture & Tile Style Guide

## Overall style

Top-down 2D dungeon. **16×16 px SVG tiles**, pixel-art icon style.
Bold 1 px black outlines, 3–5 colours per tile, high contrast against a near-black background.
No gradients. `shape-rendering="crispEdges"` on every SVG root element.

## Prompt prefix

Two variants depending on tile type:

**Terrain tiles** (floor, wall, stairs) — solid background, always the bottom draw layer:
> 16×16 pixel art SVG icon, dark fantasy dungeon roguelike style, solid black background #0a0806, bold 1px black outline, limited palette (3–5 colours), no gradients, crispEdges rendering, high contrast. Subject: …

**Entity & item tiles** (player, enemies, items) — transparent background, drawn on top of terrain:
> 16×16 pixel art SVG icon, dark fantasy dungeon roguelike style, **transparent background** (no background rect), bold 1px black outline, limited palette (3–5 colours), no gradients, crispEdges rendering, high contrast. Subject: …

> ⚠️ Entity/item SVGs must NOT contain a full-tile background `<rect>`. Only paint the pixels that belong to the sprite — everything else must be transparent so the terrain tile shows through underneath.

---

## Terrain

| Tile | Subject |
|------|---------|
| Floor | Dark worn stone flagstone, subtle crack texture |
| Wall | Rough-cut stone brick block, slight top-face highlight |
| Stairs down | Stone steps descending into darkness, warm gold tones |

## Player

| Tile | Subject |
|------|---------|
| Hero | Front-facing armoured knight with a yellow helmet crest |

## Enemies

| Tile | Subject |
|------|---------|
| Goblin | Green classic fantasy goblin, small and scrappy |
| Skeleton | Undead skeleton warrior, bone white |
| Orc | Brown heavy-set orc with tusks, carrying a club |
| Troll | Huge dark green troll, hunched, knuckles dragging |
| Dragon | Red dragon seen from above, wings spread |

## Items

| Tile | Subject |
|------|---------|
| Health Potion | Red glass healing flask with cork stopper |
| Sword | Short sword laid diagonally, silver blade, brown hilt |
| Shield | Bronze heater shield, face-on, with central boss stud |
| Scroll of Lightning | Rolled parchment with a lightning bolt on the face |
