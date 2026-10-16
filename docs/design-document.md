# Marble Madness 3D: Design Document

**Course:** COSC2349 Games Studio 2, Assignment 3
**Team (Group 5):** Donovan Ong, Aaron Tran, Andrew Loi, Jayden Bach
**Engine:** Godot 4.7.1 (.NET), gameplay code in C#

## 1. Concept

Marble Madness 3D is a physics-driven clone of Atari's *Marble Madness* (1984). It is set inside a bright, high-key museum. The player rolls a glass marble through three gallery wings, each built from the same modular kit of tiles. The goal is to reach each wing's exit as quickly as possible while avoiding falls and hazards.

The original game's isometric view, time pressure and hazards that push the marble off course are kept. The setting is reworked: in place of an abstract arcade grid, each course is a museum exhibit. The hazards are museum objects: a caryatid statue that patrols its plinth, an antique well that pulls the marble in, and a freshly mopped floor behind a wet-floor sign.

### Design pillars

1. **Weight you can feel.** The marble is a real rigid body pushed by forces, never moved directly. Every slope, bounce and knockback comes from the physics engine, so skill means managing momentum.
2. **Readable from above.** A fixed isometric camera, clear tile shapes and clear hazard silhouettes let the player plan a line before committing to it.
3. **Short runs, worth replaying.** Each wing takes one to two minutes. A stopwatch, three-star grades and saved best times reward taking each course again more cleanly.
4. **One kit, many rooms.** Every course is assembled from the same set of tiles. A new wing comes from a new layout and a new material theme, not new geometry.

## 2. Core loop and controls

**Loop:** pick a gallery → roll to the exit, using checkpoints as safety nets → get a time and a 1–3 star grade → retry for a better grade, or unlock and play the next gallery.

| Input | Action |
|---|---|
| WASD / arrow keys | Roll the marble, relative to the camera (W rolls up the screen) |
| Mouse wheel / Q, E | Step through five camera zoom levels |
| Esc | Pause and resume; back out of menu sub-screens |
| Mouse / keyboard | All menus are navigable with either |

The original arcade machine used a trackball. The team considered two schemes: tilting the whole level, or pushing the marble directly. Direct force was chosen in Week 1. It is easier to tune and keeps the camera and level static, which matters for readability. It also maps cleanly to keyboard input.

## 3. Marble physics and control

The marble (`scenes/marble/Marble.tscn`, `scripts/Marble/MarbleController.cs`) is a `RigidBody3D`. Movement applies a central force every physics tick in the direction of the input. Velocity and position are never set directly.

| Parameter | Value | Why |
|---|---|---|
| Radius | 0.25 m | Four marble-widths per 2 m tile, so narrow paths are possible |
| Mass | 1 kg | Godot's default, the baseline for all force values |
| Move force | 12 N | Enough to climb a one-tile ramp from a standing start, but not to stop instantly on a downhill run |
| Max speed | 8 m/s | Force is only applied below this speed, so gravity can still take the marble faster downhill, but the player can't power past it on the flat |
| Bounce | 0.5 | Landings visibly bounce once or twice and then settle |
| Continuous collision detection | On | Stops the marble tunnelling through thin tiles at high speed |
| Physics tick rate | 120 Hz | Doubles the default, for smoother contact with tile seams and ramp edges |

**Camera-relative input.** The camera looks at the level from a 45° angle. When input was mapped to world axes, pressing W rolled the marble diagonally up-left on screen. The controller now takes the active camera's forward and right vectors and flattens them onto the ground plane, then builds the force from them. That way, W, A, S and D always mean up, left, down and right on screen. Flattening matters because otherwise the camera's downward pitch would push the marble into the floor. The controller reads the camera from the viewport rather than referencing the camera rig, so the two systems stay decoupled.

**Hazard reactions.** The marble exposes two reactions that hazards call:

- `GetHitPhys(position)` handles contact with a moving hazard. It applies a horizontal impulse directly away from the hazard and takes away control for 0.6 s.
- `GetHit()` handles a stationary hazard such as the wet floor. It takes away control for 1.0 s with no impulse, so the marble slides on with its own momentum.

During a spin-out, further hits are ignored, so a marble can't be stuck in a loop of repeated knockbacks.

**Death and respawn.** Falling into a kill zone calls `Die()`. This puts the marble back at the last checkpoint with zero velocity and fires a `Died` signal for audio and UI. The level does not reload. Once the level has ended, `Die()` and input are both ignored, so a fall at the final moment can't restart a finished run behind the results screen.

## 4. Camera

`CameraRig` (`scripts/Camera/CameraRig.cs`) follows the marble at a fixed isometric angle: a pitch of −30° and a yaw of 45°. The −30° pitch is the flatter, classic isometric-game look, which shows more of the course ahead than true isometric (−35.26°). It uses an orthographic `Camera3D`, which keeps distances readable and matches the original game's flat projection.

- **Smoothing:** the rig moves its position toward the marble each physics tick using exponential smoothing (`1 − e^(−k·Δt)`). Unlike a fixed-alpha lerp, this gives the same feel at any frame rate. The rig never rotates, so the viewing angle never changes.
- **Zoom:** five levels (orthographic sizes 5, 7, 10, 15 and 20, starting at 15), stepped with the mouse wheel or Q/E and eased with the same smoothing. Camera distance scales with zoom:
  - zoomed out, the near edge of the frame never clips behind the camera;
  - zoomed in, the camera sits closer, keeping directional shadows sharp.
- **Never losing the marble:** the camera always looks straight at the marble from above the level. It sits on the side facing the backdrop, so no level geometry is ever between it and the marble.

## 5. Tile system

### 5.1 Grid convention

Every tile follows `docs/tile-convention.md`:

| Rule | Value |
|---|---|
| Footprint | 2 m × 2 m, origin at the footprint centre |
| Walking surface | y = 0 at the tile's base height |
| Elevation step | 1 m, so a one-tile ramp is a 2:1 slope of about 27° |
| Rotation | 90° steps around Y only; tiles are never scaled in a level |
| Connections | Flush edges at ±1 m; neighbouring surfaces match or differ by exactly one step |

An early 4 m footprint was dropped for 2 m in Week 1. The finer grid lets curves, forks and one-tile bridges be assembled from small pieces instead of special-case geometry.

### 5.2 The kit

Ten tile types, each a scene in `scenes/tiles/`:

| Tile | Role |
|---|---|
| Flat | Standard track |
| Half | Half-height step, for small drops and ledges |
| Ramp | One-step slope between elevations |
| Steep | Two-step slope, for fast drops in later levels |
| Ramp Corner | Turns 90° while changing elevation |
| Curve | Rounded 90° turn on the flat |
| Hole | Slab with a cut-out; the ring around it can be rolled, the centre is a fall |
| Wall | Rail that keeps the marble on track, or shapes mazes |
| Arch | Gateway the marble rolls under; frames routes and reads as "museum" |
| Goal | Exit platform, marked with a gold flag |

### 5.3 Themes and GridMap

Each theme is a `MeshLibrary` (`assets/libraries/`) built from the same ten tiles with a different material set. Levels are painted onto a Godot `GridMap` with the theme's library (cell size 1 × 0.5 × 1 m, so the 2 m tiles snap cleanly).

| Library | Look | Used in |
|---|---|---|
| `tile_library` | Pale limestone gallery stone | Gallery 01 |
| `marble_library` | Polished white marble with dark marble accents and gold trim | Gallery 02 |
| `sand_library` | Sandstone floors, dark sand edges, sandstone walls, gold trim | Gallery 03 |

Materials are shared resources in `assets/materials/`. Each one is a `StandardMaterial3D` that uses triplanar mapping with CC0 PBR textures (see `docs/credits.md`), so any tile shape textures cleanly without UV work. Gold is used only for goal flags and accents, so the player's eye is always drawn to the exit.

Painting levels onto a GridMap, rather than placing tile scenes one by one, keeps level files small. GridMap also batches its meshes into octants, which cuts draw calls on the largest level (see §12).

## 6. Level design

All three levels share the same structure. Each has:

- a start marker (`PlayerStart`) on the highest tier;
- a descent across several elevations to a gold goal;
- checkpoints placed after each major hazard section;
- a kill zone below the level to catch falls.

Each level holds only its own tiles, triggers and hazards. The marble, camera, HUD and menus live in a shared shell (§11.2).

### Gallery 01: Stone Hall (introduction)

- **Theme:** limestone, with classical busts in the backdrop.
- **Layout:** a 7.5 m descent across six tiers, starting at the top-floor landing and winding down via ramps, curves and hole tiles.
- **Teaches:**
  1. Rolling and stopping on flat ground.
  2. Taking ramps under control.
  3. Avoiding the centre of hole tiles.
  4. Meeting one of each hazard type in isolation:
     - one patrolling caryatid on an open stretch, where the knockback is easy to see and survive;
     - one wet-floor puddle on a straight, so the loss of control is harmless;
     - one vacuum well beside a wide section, where its pull can be felt and resisted.
- **Checkpoint:** one, at the midpoint.
- **Difficulty:** wide paths, few drops without a rail, generous star times.

### Gallery 02: Marble Hall (combination)

- **Theme:** white marble with gold trim.
- **Layout:**
  1. It opens in a walled maze on the top floor. The player must find the single exit, an arch in one corner.
  2. It then drops 4 m over three tiers to a lower hall.
- **Escalation:**
  - Hazards are combined rather than isolated. Four caryatids patrol in two facing pairs across the lower hall, so the player has to time a gap between crossing paths.
  - A vacuum well sits beside the main route, so a knockback near it can drag the marble off the edge.
  - Paths narrow to one tile in places.
- **Checkpoints:** two, at the maze exit and before the caryatid hall.
- **Difficulty:** first real test of route-finding and timing; longer star times to fit the maze.

### Gallery 03: Egyptian Wing (mastery)

- **Theme:** sandstone, with the granite head of Amenemhat III and the Egyptian head scans displayed in the backdrop.
- **Layout:** the tallest and longest course. Steep tiles and ramp corners give fast, committed drops. One-tile bridges with no rails cross open space.
- **Fork:** midway the route splits around a central obelisk court.
  - The short route crosses a narrow bridge between two vacuum wells.
  - The long route is wider and safer, but passes a wet floor at the foot of a steep ramp, where losing control is most dangerous.
  - The two routes meet again before the final descent.
- **Escalation:** every hazard type appears together, at speed. Caryatids patrol the final approach to the goal, so the run ends with the hardest timing check.
- **Checkpoints:** three: after the first steep drop, at the fork, and after the routes rejoin.
- **Difficulty:** tightest margins in the game; three stars requires the short route.

### Difficulty progression

| | Gallery 01 | Gallery 02 | Gallery 03 |
|---|---|---|---|
| Hazards | 1 caryatid, 1 well, 1 puddle | 4 caryatids, 1 well | Caryatids, 2 wells, puddles |
| Hazards combined? | No, one at a time | Paired caryatids; knockback near a well | All types together |
| Narrowest path | 2 tiles | 1 tile with rails | 1 tile without rails |
| Signature feature | Hole tiles | Opening maze | Route fork, steep drops |
| Checkpoints | 1 | 2 | 3 |
| 3 / 2 / 1-star times | 30 / 60 / 90 s | 45 / 75 / 120 s | 60 / 100 / 150 s |

Levels unlock in order. A gallery opens once the previous one has been cleared with at least one star.

## 7. Hazards

All hazards sit on the Hazards physics layer and react only to the marble. Each one calls the marble's public reaction methods, so no hazard needs to know how the marble works internally.

| Hazard | Model | Behaviour | Response |
|---|---|---|---|
| **Caryatid** (moving) | Photogrammetry caryatid statue | Kinematic body following a `Path3D` patrol route at a set speed, looping. The route is captured in world space when the level loads, so a scaled-up statue follows a correctly scaled path. | Knockback away from the statue plus 0.6 s without control |
| **Well** (environmental) | Decimated well model plus animated wind VFX | Inside its area, pulls the marble toward the centre. The pull is strongest at the middle and falls off linearly to zero at a 5 m radius (20 N at full strength). Wind ribbons spiral inward to show the pull. | Continuous force; the player must steer out. Falling in is a fall like any other. |
| **Wet floor** (stationary) | Wet-floor sign beside a puddle decal | Trigger area on the floor | 1.0 s without control; the marble keeps its momentum |

The well's suction comes from the original game's vacuums. The wet floor turns the original's acid pools into a museum gag: it never kills the marble, but a spin-out at the wrong moment can carry it off an edge. Hazards are therefore dangerous because of where they're placed, which is why the level design puts them near drops in later galleries.

## 8. Timer, checkpoints, win and lose

- **Stopwatch:** `LevelTimer` counts up from zero and is visible on the HUD at all times. Each level's star times give it time pressure: finishing at or after the 1-star time counts as a game over ("timer expires"). This keeps the original game's pressure to beat the clock while rewarding faster play with a grade.
- **Checkpoints:** passing a checkpoint updates the respawn point in the `CheckpointManager` autoload. A fall respawns the marble there instantly; the level does not reload and the clock keeps running. Each new attempt clears the old checkpoint, so one never carries over into another run or level.
- **Win:** reaching the goal calls `GameStateManager.Win()`. The marble stops responding, the timer freezes, and the results screen shows the grade.
- **Lose:** finishing too slowly ends the run on the game-over panel, with Try Again, Level Select and Main Menu.
- **Ended state:** `GameStateManager` keeps a single ended flag that the marble, timer, pause menu and results screen all read directly. Each new run clears it when the marble loads, so retrying or moving to another level always starts live.
- **Progress:** best time and best stars per level are saved to `user://progress.cfg` by `ProgressStore`, and shown on the results screen and level select.

## 9. UI and menus

The UI has its own look: cream panels, a muted gold accent, and the Diffusion typeface for headings and body text.

| Screen | Content |
|---|---|
| **Start menu** | A live 3D gallery backdrop with a slowly drifting camera, behind a crossfading slideshow of in-game screenshots from all three galleries. Buttons: Start Game, Level Select, How to Play, Settings (master volume, fullscreen), Quit. |
| **Level select** | One card per gallery, switched with the arrows, the dots, or left/right keys. Each card shows an in-game screenshot, earned stars, best time, and Play / Play Again / Locked. Locked galleries show a padlock and the requirement to unlock them. A counter shows total stars out of 9. |
| **HUD** | Gallery name top-left, stopwatch top-right (mm:ss.cc), and a "Reach the exit" objective that fades after six seconds to keep the screen clear. |
| **Pause** | Esc dims the level and freezes the game, the timer included. Offers Resume, Restart and Main Menu. Disabled once the level has ended. |
| **Results** | Win panel: animated stars, the time, "NEW BEST" or the previous best, and the three star thresholds with the earned tier highlighted. Game-over panel: the reason and the time. Next, Retry, Level Select and Main Menu are offered as they apply, so no screen is a dead end. |

All screens use Godot containers and anchors, so they scale across common desktop resolutions and aspect ratios. The 3D backdrop scales to cover the whole window at any size.

## 10. Audio

| Sound | Implementation |
|---|---|
| **Rolling** | A looping glass-on-wood recording whose volume and pitch rise with rolling speed. Only speed along the ground surface counts, so a landing doesn't cause a burst of rolling sound. It is silent while airborne. The loop never stops, only its volume changes, which avoids clicks. |
| **Landing** | A clunk that plays when the marble goes from airborne to grounded. Its volume scales with impact speed and its pitch varies randomly, so repeated bounces don't sound identical. Respawns are filtered out. |
| **Hazard, checkpoint, victory, UI** | Short cues triggered by the existing signals: `MarbleController.Died`, `CheckpointManager.CheckpointReached` and `GameStateManager.LevelWon`, plus button presses. |
| **Music** | A `MusicPlayer` autoload crossfades between tracks as scenes change, and keeps playing through retries and pauses. Each scene names its track with a `MusicCue` node. |

Music is public-domain-era piano repertoire, chosen to suit a quiet gallery and to tell the wings apart:

| Scene | Track |
|---|---|
| Start menu and level select | Satie, *Gymnopédie No. 1* |
| Gallery 01 | Debussy, *Arabesque No. 1* |
| Gallery 02 | Satie, *Gnossienne No. 1* |
| Gallery 03 | Ravel, *Pavane pour une infante défunte* |

The marble's sounds are not positional: the camera always follows the marble, and a 3D sound would get quieter as the camera zooms out.

## 11. Technical architecture

### 11.1 Systems and ownership

Each system is a separate script, in a folder named after the system. Systems talk through signals and three autoloads, never through direct references to each other.

| System | Key scripts | Talks to others via |
|---|---|---|
| Marble | `MarbleController` | Reads the camera from the viewport; exposes `Die()`, `GetHit()`, `GetHitPhys()`; emits `Died` |
| Camera | `CameraRig` | Follows a target `NodePath` only |
| Level | `LevelLoader`, `Checkpoint`, `Goal`, `KillZone`, `LevelTimer` | Calls the autoloads; the timer emits `TimeChanged` |
| Hazards | `MovingHazard`, `VacuumHazard`, `StationaryHazard` | Call the marble's reaction methods |
| UI | `StartMenu`, `LevelSelect`, `GameHud`, `PauseMenu`, `ResultScreen` | Listen to `TimeChanged`, `LevelWon` and `LevelLost`; read `ProgressStore` |
| Audio | `MusicPlayer`, `MusicCue`, `MarbleRollSound`, `MarbleImpactSound` | Child nodes read the marble's physics state; music is cued by scenes |
| Core (autoloads) | `GameStateManager`, `CheckpointManager`, `MusicPlayer` | Signals; state shared by the whole level |

Two examples of the decoupling:

- `MarbleController` contains no audio code. The sound nodes are its children and only read its velocity.
- `LevelTimer` doesn't know the HUD exists. It only emits the time.

### 11.2 Shared shell and level loading

`scenes/main/Main.tscn` is a shell that every level is played inside. It holds:

- the marble and camera rig;
- lighting and the gallery environment;
- the backdrop;
- the HUD, pause menu, timer and results screen.

The level select records the player's choice in `GameSession`. `LevelLoader` then loads that level into the shell, places the marble at its `PlayerStart`, and tells the HUD and results screen which level is running.

Adding a gallery takes a new level scene and one line in the level select. The marble, camera and UI are never duplicated per level.

### 11.3 Physics layers

| Layer | Name | Contents | Detects |
|---|---|---|---|
| 1 | World | Tiles, level geometry | — |
| 2 | Marble | The marble | World, Hazards |
| 3 | Hazards | Caryatids, wells, wet floors | Marble |
| 4 | Triggers | Goal, checkpoints, kill zones | Marble |

## 12. Performance

The game holds a stable 60 FPS on a standard laptop on all three galleries, including the largest, Gallery 03. Profiling with Godot's built-in profiler was part of the workflow from Week 3, not left to the end.

- **Model budget:** an early measurement found the "decimated" hazard models still had over 1 million triangles each, about ten times the statue props. The well was re-decimated in Blender from 1.25 M to 37.6 k triangles (−97%, 41.8 MB → 1.9 MB) and replaced under the same import ID, so no scenes changed. All other hazard and backdrop models were brought within the same budget.
- **GridMap:** levels render tiles in batched octants instead of hundreds of separate nodes, which keeps draw calls low on large layouts.
- **Shadows:** one directional light. Shadow distance is tightened to the play area, and the orthographic camera sits 15 m from the marble, inside that range. This keeps shadow detail high without raising shadow-map cost.
- **Physics:** continuous collision detection is enabled only on the marble. Hazards are kinematic or static bodies. Triggers are simple `Area3D` shapes on their own layer, masked to the marble only.
- **No runtime allocation in hot paths:** respawning moves the existing marble instead of creating a new one, and nothing is created or destroyed during play, so object pooling is unnecessary.
- **Rendering settings:** 2× MSAA and SSAO give the bright gallery look at a fixed, predictable cost. Fog hides the far backdrop, so distant detail is never rendered sharply.

## 13. Playtesting and iteration

Changes made in response to testing, in roughly chronological order:

| Observation | Change |
|---|---|
| A 4 m tile grid made curves and forks blocky, with too few placement options | Grid reduced to 2 m (four marble-widths) with a 1 m elevation step |
| Shadows were missing or blocky in game but sharp in the editor | The camera sat beyond the shadow range; pulled it in to 15 m (no change in framing under orthographic projection) and tightened the shadow settings |
| Falling just as time ran out let the marble respawn and keep playing behind the game-over screen | The marble now checks the shared ended flag directly, for both input and respawn, instead of keeping its own copy |
| Under the 45° camera, W rolled the marble diagonally on screen | Input made relative to the camera |
| A 2 s loss of control from a moving hazard felt like a punishment | Reduced to 0.6 s, with a knockback impulse so the cause is visible; 1.0 s for the wet floor |
| A countdown timer made new players rush and fall, and gave experienced players no reason to replay | Changed to a stopwatch with three-star grades and saved best times; finishing past the 1-star time is still a loss |
| Rolling sound burst loudly on every landing | Only speed along the ground surface counts as rolling |
| Respawning played a hard landing clunk at the checkpoint | Landing sound ignores the step in which the marble respawns |
| The marble won before it visibly reached the goal | Goal trigger shrunk and marked with a gold flag |
| Hazards scaled up for Gallery 02 followed tiny, unscaled patrol paths | Patrol paths captured in world space, so they scale with the statue |
| Gallery 02's lower hall was too crowded at the original speed | Reduced from five caryatids to four, and patrol speed from 2.0 to 1.5 |
| After a win, Retry or the next level started with a frozen marble | The ended state is cleared whenever a new marble loads |
| Hazards placed in the shared shell appeared in every level | Hazards moved into the level that owns them |

## 14. Team roles

| Member | Role | Owned systems |
|---|---|---|
| **Donovan Ong** | Head of core gameplay mechanics | Marble controller, camera, physics layers, checkpoint, timer and win/lose base systems, lighting and backdrop, music system and marble sounds, performance, documentation |
| **Aaron Tran** | Technical lead, level systems | Tile kit and the three themed GridMap libraries, all three gallery layouts and their integration |
| **Andrew Loi** | Gameplay systems | Caryatid, well and wet-floor hazards and their VFX; how the marble reacts to hazards; hazard models |
| **Jayden Bach** | Polish, UI and support | Start menu, level select, HUD, pause menu, results screen and progress saving, level loading, menu backdrop, audio polish |

Work was done on feature branches per system and merged to `main` once reviewed. Individual contributions are logged in `docs/contributions-*.md` and the commit history.

## 15. Scope

**Delivered from the stretch goals in `docs/requirements.md`:**
- Best time per level, saved locally and shown on the results and level select screens (§3.5).
- Photogrammetry models: British Museum and IU Indianapolis scans used as backdrop props and hazards (§3.3).
- Level progression with locks and star grades.

**Deliberately left out**, to protect polish on the three galleries:
- Phone-gyroscope tilt control.
- Selectable marble skins.
- A level editor exposed to players.

## 16. Credits

All third-party models, textures, music and sound effects, with their sources and licences, are listed in `docs/credits.md`.
