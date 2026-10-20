# Tile Convention

Shared spec for the modular tile system described in [requirements.md §3.3](requirements.md). Every tile piece must follow this so pieces snap together regardless of who built them or whether they are CSG greybox or final Blender meshes.

Each value is marked **Locked** (build against it) or **Tentative** (expected to change after marble/physics tuning). Change a value here first, then in the tiles.

## 1. Grid and dimensions

| Item | Value | Status | Rationale |
|---|---|---|---|
| Tile footprint | 2 m × 2 m (X × Z) | Locked | The marble is 0.5 m across (radius 0.25 m), so a tile is 4 marble-widths: fine-grained enough to build curves, forks and narrow paths from many small pieces. Wider features are made by placing tiles side by side. |
| Tile origin | Centre of the footprint, at the tile's base height | Locked | Centre origins make 90° rotation in place safe: a rotated tile still occupies the same cell. |
| Flat tile surface | y = 0 (top of the walking surface) | Locked | Levels stack tiles by elevation offset, so a consistent reference height is needed. |
| Flat tile thickness | 0.5 m (surface at y = 0, underside at y = -0.5) | Locked | Thick enough that fast-moving marbles do not tunnel through. |
| Elevation step | 1 m | Locked | A ramp spanning one tile (2 m run) rises 1 m, a 2:1 slope of about 27°. The marble's 12 N move force climbs it from a standing start. Steeper ramps are a separate tile variant (`Steep`), not ad-hoc rotations. `Half` is a half-width (1 × 2 m) flat slab for narrowing paths. |
| Grid unit | GridMap cell 1 × 0.5 × 1 m | Locked | Levels are painted on a GridMap (§3.1). A tile covers 2 × 2 cells and an elevation step is two cells. |

## 2. Connection points

- Tiles connect along the **edges** of the footprint, at X = ±1 m or Z = ±1 m from the origin.
- At an open edge, the walking surface must meet the edge flush, with full-width track and no lip. The neighbouring tile's surface height at the shared edge must match, or differ by exactly one elevation step (a ramp joins levels).
- Tiles may only be rotated in **90° steps around Y**. Never scale a tile in the level scene; make a new tile variant instead.
- Tiles should not overlap the neighbouring cell. Decorative overhang is fine only if it has no collision.

## 3. Scene structure and naming

- One tile = one `.tscn`, root node named after the file.
- File names are `<Theme>_<Type>.tscn`, with an underscore. The default theme uses the `Tile` prefix: `Tile_Flat.tscn`, `Tile_Ramp.tscn`. Themed sets use their theme name and sit in a subfolder: `Marble/Marble_Flat.tscn`, `Sand/Sand_Flat.tscn`.
- The ten tile types are Flat, Half, Ramp, Steep, RampCorner, Curve, Hole, Wall, Arch and Goal. Every theme provides all ten.
- Level scenes go in `scenes/levels/level_XX/`. They're painted onto a GridMap (§3.1) rather than built from hand-placed tile instances. No level-specific geometry is built directly in a level scene, except temporary experiments.

### 3.1 GridMap and mesh libraries

All levels are painted onto a Godot `GridMap`, one per level, using a theme's `MeshLibrary`:

| Theme | Source scene | Mesh library |
|---|---|---|
| Default (colour-coded) | `scenes/tiles/TileMap.tscn` | `assets/libraries/tile_library.tres` |
| Marble | `scenes/tiles/Marble/MarbleTileMap.tscn` | `assets/libraries/marble_library.tres` |
| Sand | `scenes/tiles/Sand/SandTileMap.tscn` | `assets/libraries/sand_library.tres` |

All three galleries use the default library. The Marble and Sand libraries are kept but unused (see the design document §5.3). The libraries list their ten tiles in different orders, so switching a level to another library means remapping its tile IDs, not just swapping the library.

- Each source scene holds one `MeshInstance3D` per tile type, each with a `StaticBody3D` and collision shape as a child, so the library carries collision.
- To update a library, edit the source scene, then use **Scene → Export As → MeshLibrary** and overwrite the `.tres`. Every level using it picks up the change.
- GridMap settings: **cell size 1 × 0.5 × 1 m** with cell centring off on all axes. A 2 m tile covers 2 × 2 cells, and a 1 m elevation step is two cells.
- GridMap batches tiles into octants, which keeps draw calls low on large levels.
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
- Keep tile types distinguishable (a tint or accent per type). In the shipped colour-coded set, each type has its own colour, which is what makes slopes, holes and rails readable from the camera.
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

The layers are named in Project Settings → Layer Names → 3D Physics (`project.godot`). Tiles, hazards and triggers must use them as below.

| Layer | Name | Used by |
|---|---|---|
| 1 | World | Tiles and level geometry |
| 2 | Marble | The marble |
| 3 | Hazards | Moving and environmental hazards |
| 4 | Triggers | Goal, checkpoints, kill zones |

**Layer and mask per object** (layer = what it is, mask = what it detects or collides with):

| Object | Layer | Mask | Notes |
|---|---|---|---|
| Marble | 2 | 1 + 3 | Collides with the world and hazards. Already set on `Marble.tscn` (layer 2, mask 5). |
| Tiles | 1 | none | Static geometry. CSG `Use Collision` defaults to layer 1, so leave it. |
| Hazards | 3 | 2 | Reacts to the marble. |
| Triggers | 4 | 2 | `Area3D` nodes that only detect the marble. |
| Knock-over props | 1 | 1 + 2 | Physics props such as vases (`scenes/props/`). They rest on tiles, get hit by the marble, and can knock each other over. |

All of the above are applied in the current scenes.

## 7. Marble parameters tiles depend on

Final values ([Marble.tscn](../scenes/marble/Marble.tscn), [MarbleController.cs](../scripts/Marble/MarbleController.cs)). Tile dimensions such as gap width, ramp steepness and wall height are designed around these; revisit them if any of these change.

| Parameter | Value |
|---|---|
| Radius | 0.25 m (default sphere shape, scaled 0.5) |
| Mass | 1 kg (default) |
| Bounce | 0.5 |
| Move force | 12 |
| Max speed | 8 m/s |
| Continuous CD | On |

## 8. Resolved questions

- **Track width:** both are used. Most of the course is two or more tiles wide. One-tile paths appear deliberately as difficulty rises: with rails in Gallery 02, and without rails in Gallery 03 (design document §6).
- **Walls and rails:** a separate `Wall` tile, placed next to track tiles where a rail is wanted. It's also used to build mazes.
