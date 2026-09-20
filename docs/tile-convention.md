# Tile Convention

Shared spec for the modular tile system described in [requirements.md §3.3](requirements.md). Every tile piece must follow this so pieces snap together regardless of who built them or whether they are CSG greybox or final Blender meshes.

Each value is marked **Locked** (build against it) or **Tentative** (expected to change after marble/physics tuning). Change a value here first, then in the tiles.

## 1. Grid and dimensions

| Item | Value | Status | Rationale |
|---|---|---|---|
| Tile footprint | 2 m × 2 m (X × Z) | Locked | The marble is 0.5 m across (radius 0.25 m), so a tile is 4 marble-widths: fine-grained enough to build curves, forks and narrow paths from many small pieces. Wider features are made by placing tiles side by side. |
| Tile origin | Centre of the footprint, at the tile's base height | Locked | Centre origins make 90° rotation in place safe: a rotated tile still occupies the same cell. |
| Flat tile surface | y = 0 (top of the walking surface) | Locked | Levels stack tiles by elevation offset, so a consistent reference height is needed. |
| Flat tile thickness | 0.5 m (surface at y = 0, underside at y = -0.5) | Tentative | Thick enough that fast-moving marbles do not tunnel through. |
| Elevation step | 1 m | Tentative | A ramp spanning one tile (2 m run) rises 1 m, a 2:1 slope of about 27°. This is steep, so check it against the current move force during tuning. Steeper or shallower ramps should be new tile variants, not ad-hoc rotations. |
| Grid unit | 2 m for X/Z, 1 m for Y | Tentative | Set the editor's snap step to these values when placing tiles. |

## 2. Connection points

- Tiles connect along the **edges** of the footprint, at X = ±1 m or Z = ±1 m from the origin.
- At an open edge, the walking surface must meet the edge flush, with full-width track and no lip. The neighbouring tile's surface height at the shared edge must match, or differ by exactly one elevation step (a ramp joins levels).
- Tiles may only be rotated in **90° steps around Y**. Never scale a tile in the level scene; make a new tile variant instead.
- Tiles should not overlap the neighbouring cell. Decorative overhang is fine only if it has no collision.

## 3. Scene structure and naming

- One tile = one `.tscn` in `scenes/tiles/`, root node is a `Node3D` named after the file.
- File names use PascalCase, `Tile` prefix, type then variant: `TileFlat.tscn`, `TileRamp.tscn`, `TileHole.tscn`, `TileArch.tscn`, `TileCurve.tscn`, `TileBridge.tscn`.
- Themed variants add a suffix: `TileFlat_Stone.tscn`, `TileFlat_Ice.tscn`.
- Level scenes go in `scenes/levels/level_XX/` and only *instance* tile scenes. No level-specific geometry is built directly in a level scene, except temporary experiments.
- Tile visuals and physics live inside the tile scene, so changing how a tile is built (CSG, baked mesh or Blender mesh) only changes the internals of the tile and nothing that instances it.

## 4. CSG tiles and materials

Tiles are built from CSG nodes. There are two ways to ship them, and a level can mix both since levels only instance tile scenes:

- **Final CSG + material pass:** keep the CSG geometry and make it look finished with PBR materials and scene lighting (§4.2).
- **Blender replacement:** swap the tile's internals for a modelled mesh (§5) where CSG geometry is not good enough.

### 4.1 CSG construction rules

- Use `CSGBox3D`/`CSGPolygon3D` inside the tile scene, with **Use Collision** on.
- Only combine CSG shapes within the same tile scene. Never combine across tiles.
- Cutting shapes (subtraction) should be taller or larger than the shape they cut, so their faces do not coincide with the surface they cut through.
- Moving or rotating tiles need an `AnimatableBody3D` parent with Use Collision off on the CSG child and a separate collision shape.

### 4.2 Material pass

- Every tile references a shared material resource in `assets/materials/`, never a per-tile material. Themed variants swap the material, not the geometry.
- For a textured look, use a `StandardMaterial3D` with albedo, roughness and normal maps from a CC0 PBR set (for example ambientCG or Poly Haven), stored in `assets/textures/`.
- Turn on **UV1 → Triplanar** so the texture maps consistently on any CSG shape without UV setup. Leave **World Triplanar** off so the texture stays fixed to the tile when it is moved or rotated.
- Set **UV1 Scale** for a consistent texel density across tile types, roughly one repeat per 1-2 m.
- Keep tile types distinguishable (a tint or accent per type), since the placeholder colours currently do that job.
- Record the source and licence of every texture set used (a note in the docs is enough).
- Scene lighting (`WorldEnvironment` with SSAO, glow and sky, plus shadows on the directional light) is part of the look and is set in the level or main scene, not per tile.

### 4.3 Limits of CSG, and when to move to Blender

CSG gives hard 90° edges with no bevels, faceted curves, and stretched texture on cut surfaces. Use a Blender model for a tile when the material pass cannot fix that (curves, bridges, bevelled edges, organic pieces).

### 4.4 Shipping CSG in the final build

CSG tiles may stay in the final build if the game holds the 60 FPS target (see requirements §3) and scene load time stays acceptable on a full level. Check both with the largest level.

If either target is missed, bake the affected tiles to static meshes with proper collision (CSG menu → **Bake Static Mesh**), or replace them with Blender models. Because the tile scene is the boundary, this does not change the levels.

## 5. Blender / final asset export

Applies to tiles that are replaced by a modelled mesh (see §4.3), once the dimensions above are settled.

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

- Track width: a single 2 m tile is only 4 marble-widths across. Are lanes one tile wide, or do levels use two-tile-wide tracks? Do tiles have a narrower path with edges to fall off? This affects the fall/respawn design.
- Wall or rail pieces: separate tiles, or part of each tile?
