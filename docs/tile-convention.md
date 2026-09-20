# Tile Convention

Shared spec for the modular tile system described in [requirements.md §3.3](requirements.md). Every tile piece must follow this so pieces snap together regardless of who built them or whether they are CSG greybox or final Blender meshes.

Each value is marked **Locked** (build against it) or **Tentative** (expected to change after marble/physics tuning). Change a value here first, then in the tiles.

## 1. Grid and dimensions

| Item | Value | Status | Rationale |
|---|---|---|---|
| Tile footprint | 4 m × 4 m (X × Z) | Tentative | The marble is 0.5 m across (radius 0.25 m), so a tile is 8 marble-widths: wide enough for forks and hazards, small enough to assemble quickly. |
| Tile origin | Centre of the footprint, at the tile's base height | Locked | Centre origins make 90° rotation in place safe: a rotated tile still occupies the same cell. |
| Flat tile surface | y = 0 (top of the walking surface) | Locked | Levels stack tiles by elevation offset, so a consistent reference height is needed. |
| Flat tile thickness | 0.5 m (surface at y = 0, underside at y = -0.5) | Tentative | Thick enough that fast-moving marbles do not tunnel through. |
| Elevation step | 1 m | Tentative | A ramp spanning one tile (4 m run) rises 1 m, about 14°. Steeper or shallower ramps should be new tile variants, not ad-hoc rotations. |
| Grid unit | 4 m for X/Z, 1 m for Y | Tentative | Set the editor's snap step to these values when placing tiles. |

## 2. Connection points

- Tiles connect along the **edges** of the footprint, at X = ±2 m or Z = ±2 m from the origin.
- At an open edge, the walking surface must meet the edge flush, with full-width track and no lip. The neighbouring tile's surface height at the shared edge must match, or differ by exactly one elevation step (a ramp joins levels).
- Tiles may only be rotated in **90° steps around Y**. Never scale a tile in the level scene; make a new tile variant instead.
- Tiles should not overlap the neighbouring cell. Decorative overhang is fine only if it has no collision.

## 3. Scene structure and naming

- One tile = one `.tscn` in `scenes/tiles/`, root node is a `Node3D` named after the file.
- File names use PascalCase, `Tile` prefix, type then variant: `TileFlat.tscn`, `TileRamp.tscn`, `TileCurve.tscn`, `TileGap.tscn`, `TileBridge.tscn`.
- Themed variants add a suffix: `TileFlat_Stone.tscn`, `TileFlat_Ice.tscn`.
- Level scenes go in `scenes/levels/level_XX/` and only *instance* tile scenes. No level-specific geometry is built directly in a level scene, except temporary experiments.
- Tile visuals and physics live inside the tile scene, so swapping CSG for a Blender mesh only changes the internals of the tile and nothing that instances it.

## 4. Greybox (CSG) rules

- Use `CSGBox3D`/`CSGPolygon3D` inside the tile scene, with **Use Collision** on.
- Only combine CSG shapes within the same tile scene. Never combine across tiles.
- Moving or rotating tiles need an `AnimatableBody3D` parent with Use Collision off on the CSG child and a separate collision shape.
- CSG is for prototyping only. Tiles are baked to static meshes with proper collision before the final build (performance target: 60 FPS, see requirements §3).

## 5. Blender / final asset export

Applies once the dimensions above are settled.

- Model at real-world scale: 1 Blender unit = 1 m, with the origin at the tile centre and the walking surface at y = 0.
- Apply scale and rotation before export, and export as `.glb`.
- Collision: either use Godot's import-time trimesh/convex generation, or include a simplified mesh with the `-col` suffix so physics geometry is cheaper than the visible mesh.
- Put source `.blend` files under `assets/models/` alongside the exported files.

## 6. Physics layers

Not yet applied in the project (everything is on the default layer 1). Proposed once hazards and triggers exist:

| Layer | Name |
|---|---|
| 1 | World (tiles) |
| 2 | Marble |
| 3 | Hazards |
| 4 | Triggers (goal, checkpoints, kill zones) |

**Tentative.** Agree this with Andrew (hazards, checkpoints) before applying it.

## 7. Marble parameters tiles depend on

Current prototype values ([Marble.tscn](../scenes/marble/Marble.tscn), [MarbleController.cs](../scripts/Marble/MarbleController.cs)). Tile dimensions such as gap width, ramp steepness and wall height should be designed around these, and revisited if they change.

| Parameter | Value |
|---|---|
| Radius | 0.25 m (default sphere shape, scaled 0.5) |
| Mass | 1 kg (default) |
| Bounce | 0.5 |
| Move force | 12 |
| Max speed | 8 m/s |
| Continuous CD | On |

Tentative until the control feel is tuned on ramps and gaps in the greybox.

## 8. Open questions

- Confirm the 4 m footprint with Aaron (tile system owner) before many tiles exist.
- Track width: full width of the tile, or a narrower path with edges for the marble to fall off? This affects the fall/respawn design.
- Wall or rail pieces: separate tiles, or part of each tile?
