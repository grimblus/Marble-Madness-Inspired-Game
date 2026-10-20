# Marble Madness 3D Clone — Requirements Specification

## 1. Project Context

- **Course:** COSC2349 — Games Studio
- **Assessment:** Assignment 3 — 3D clone of *Marble Madness*
- **Weighting:** 40% of final mark
- **Type:** Group project, with individually assessed contribution
- **Due:** Monday, Week 14 — **25 October, 11:59pm AEDT**
- **Submission format:** A Classroom50 invite link to the repository — the game runs locally from the project/repo, no web export or hosting required
- **Engine:** Godot 4.7.1 (.NET build), with gameplay code written in C#
- **Scope target:** 2–3 fully polished levels, prioritizing depth and quality over quantity

## 2. Rubric-to-Feature Mapping

| Rubric Criterion | Points | Top-band requirement |
|---|---|---|
| Functionality (gameplay, physics, tileable world) | 20 | All core features implemented and working; fully playable |
| Technical Implementation (code quality, structure, optimization) | 15 | Clean, well-structured code; stable 60 FPS |
| Creativity and Polish (visual/audio, unique features) | 5 | High polish; visually/audibly appealing; creative elements |
| Documentation (design doc, milestone presentation, README, video) | 10 | Complete, clear, professional across all four artifacts |

Every requirement below is written to satisfy the top rubric band. Anything marked **[Stretch]** is beyond-HD scope — implement only once baseline is solid and stable.

## 3. Core Systems — Functional Requirements

### 3.1 Marble Controller

- Marble must be a true physics body (`RigidBody3D`), not a kinematic body faked to look physical.
- Movement is driven by applying torque/force to the marble in response to input, not by directly setting velocity or position.
- Required input scheme: tilt-the-world OR direct-marble-force control (pick one, document the choice). Direct force control is simpler to tune in Godot and recommended as the primary target.
- Physics tuning targets:
  - No tunnelling through geometry at max speed — use continuous collision detection or a fixed, sufficiently small physics tick (`physics_ticks_per_second`) to prevent this.
  - Friction/rolling resistance tuned so the marble feels weighty but responsive — avoid ice-rink sliding or instant-stop feel.
  - Max speed cap to keep the marble controllable and prevent camera/physics breakage.
- Marble must support: falling off the level (triggers death/respawn), colliding with hazards (triggers death or knockback), colliding with collectibles/goal triggers (triggers game state change).
- **[Stretch]** Multiple selectable marble skins/materials with no gameplay difference, or with minor stat variation (e.g. weight vs. control trade-off).
- **[Stretch]** Phone-gyroscope tilt control as an alternate input scheme — a companion mobile web page (or small app) streams device orientation/accelerometer data to the game over a local connection (e.g. WebSocket) to steer the marble, mimicking the arcade cabinet's tilt controller. Treat as an additive input mode alongside keyboard/gamepad, not a replacement, and scope it only after the primary control scheme (§3.1) is solid — network latency and calibration (dead zone, sensitivity, re-centering) will need dedicated tuning and testing across devices.

### 3.2 Camera System

- Isometric or fixed-angle 3rd-person camera, matching the original Marble Madness perspective, OR a smooth-follow 3rd-person camera — pick one and keep it consistent across all levels.
- Camera must never lose sight of the marble or clip through level geometry.
- Camera movement should be smoothed (lerp/spring-arm) — no jitter or hard snapping.
- **[Stretch]** Dynamic camera behaviour: slight zoom-out at high speed, camera shake on impact.

### 3.3 Level / Tile System

- Levels must be built from a **modular, tileable set of pieces** — ramps, straight track segments, curves, gaps, bridges, rotating platforms, moving platforms — reused and recombined across levels, not one-off bespoke geometry per level.
- Recommend implementing tile pieces as individual `.tscn` scenes with standardized connection points (consistent tile size/anchor convention) so levels assemble like a kit.
- Each level must have: a clear start point, a clear goal/end trigger, at least one fork or elevation change, and escalating difficulty relative to the previous level.
- **Target: 2–3 levels**, each with a distinct visual theme (e.g. stone/ancient, ice, industrial) to satisfy both the "tileable world" and "creativity" rubric lines.
- **[Stretch]** Levels defined via external data (resource files/JSON) describing tile placement, so level layout is data-driven rather than hardcoded in scene trees.
- **[Stretch]** In-editor or in-game level assembly tool.
- **[Stretch]** Photogrammetry-sourced models/textures for the marble and/or level tile pieces (e.g. scanned rock, stone, or ground surfaces reconstructed via photogrammetry and retopologized/baked for real-time use) to boost visual fidelity beyond hand-modelled assets. Scope carefully: requires capture or sourcing, cleanup (retopology, UV, texture baking), and a poly-count/texel-density budget that still hits the 60 FPS target — treat as a polish pass on top of a baseline of simpler placeholder/hand-modelled assets, not a dependency for level completion.

### 3.4 Hazards & Obstacles

- At minimum, per level: one moving hazard (e.g. patrolling enemy, swinging obstacle, rolling boulder) and one environmental hazard (pit, acid/lava, vacuum/suction zone as in the original).
- Hazard collision must trigger a clear, consistent response: instant death/respawn, or knockback + damage state if a health/lives system is implemented.
- **[Stretch]** Physics-interactive hazards (see-saws, pinball bumpers, pushable blocks) rather than purely static/scripted hazards.

### 3.5 Timer, Checkpoints, Win/Lose

- Countdown timer per level, visible in the HUD at all times — matches the original's time-pressure design.
- Checkpoint system: falling or dying respawns the marble at the last checkpoint, not the start of the level, and does not require a full level reload.
- Win condition: reaching the level's goal trigger before time runs out → victory screen → transition to next level (or final victory screen on the last level).
- Lose condition: timer reaches zero → game-over screen with retry option.
- **[Stretch]** Best-time tracking per level (local storage), shown on the victory screen.

### 3.6 Submission & Repository

- Submission is a **Classroom50 invite link** to the team repository — the game is assessed by running the Godot project locally, not via a hosted web build. No web/HTML5 export is required.
- Repository must be self-contained and runnable by a marker who clones it fresh: correct `.gitignore` (exclude `.godot/`, build artifacts), a pinned/documented Godot version (4.7.1 .NET) and .NET SDK version, and a clear entry-point scene set as the project's main scene.
- Keep the repo clean before submission — no dead branches left as the default, no broken commits on main, no missing assets (check nothing needed is gitignored by accident).

### 3.7 UI / HUD

- In-level HUD: timer, current level indicator, lives/attempts remaining (if applicable).
- Menus: main menu (start/level select if applicable), pause menu, victory screen, game-over screen — all must be navigable via mouse and not dead-end the player.
- UI must scale reasonably across common desktop resolutions, since it will be run locally by the marker rather than in a fixed browser canvas.

### 3.8 Audio

- Sound effects required for: rolling/movement, collision/impact, hazard death, checkpoint, victory, UI interaction.
- Background music, at minimum one track, ideally distinct per level theme or per game state (menu vs. in-level vs. low-time-remaining).
- **[Stretch]** Reactive/adaptive audio (e.g. music intensity increases as timer runs low).

## 4. Non-Functional / Technical Requirements

- **Performance:** stable 60 FPS running locally from the editor/exported desktop build on a standard laptop — this is explicitly graded, so profile with Godot's built-in profiler regularly, not only at the end.
- **Code structure:** clear separation between player controller, level/tile system, camera, hazards, UI, and audio as independent, decoupled scripts/nodes. Avoid god-objects (e.g. one script controlling marble, camera, and UI all together).
- **Version control:** feature branches per system, pull requests before merging to main, no direct commits to main in the final 48 hours before submission.
- **Code style:** agree on a shared C# style convention (naming, typing, signal/event usage) before heavy implementation begins, since four people will be touching the codebase.
- **[Stretch]** Object pooling for frequently spawned objects (particles, hazard instances) if profiling shows GC/instantiation cost.
- **[Stretch]** Automated or documented QA checklist run before each milestone submission.

## 5. Documentation Deliverables

All four are individually graded rubric items — none can be skipped or left thin:

1. **Design document** — concept, level design rationale, physics/control design decisions, team role breakdown, playtesting notes and what changed as a result.
2. **README** — setup/run instructions (Godot version, how to open and run the project), controls, known issues.
3. **Milestone presentation** — details to be released by the course coordinator at a later stage; prepare a short deck covering progress, design decisions, and demo footage.
4. **Video demonstration** — clean, edited gameplay footage covering all levels, narrated or captioned to explain features being shown (not a raw unedited screen recording).

## 6. Out of Scope (explicitly, to protect timeline)

- More than 3 levels
- Multiplayer/co-op
- Level editor exposed to end users (unless time allows as a stretch goal per §3.3)

## 7. Open Items to Confirm

- ~~Confirmed Classroom50 repo/invite-link setup process~~ **Resolved (20 Sep):** the Classroom50 repo was set up, and the project history was migrated into it with full commit history kept.
- ~~Final choice of control scheme (tilt-world vs. direct-force)~~ **Resolved (Week 1):** direct-force was chosen and implemented in the first marble controller prototype. In Week 4, movement was made relative to the camera (W rolls up the screen).

## 8. Status and deviations (20 Oct, final pass)

The spec above is kept as written. This section records where the game differs from it, and what's still outstanding.

| § | Requirement | Status |
|---|---|---|
| 3.5 | Countdown timer; losing when it reaches zero | **Changed by design.** A stopwatch with three-star grades and saved best times, plus a hard 5-minute limit that ends the run in a game over. Time pressure stays, but faster play is rewarded with a grade. See the design document §8 and §13. |
| 3.4 | At least one moving and one environmental hazard per level | **Met.** Every gallery has patrolling caryatids and wet floors; Galleries 02 and 03 add vacuum wells and trampolines, and Gallery 02 blowers. See the design document §6. |
| 3.3 | 2–3 levels, each with a distinct theme | **Met, with a change.** Three galleries are playable. They share the colour-coded tile set, so tile types read the same everywhere, and each gallery's theme comes from its own exhibit backdrop and music instead of tile materials. See the design document §5.3. |
| 3.8 | Sound effects for hazard death, checkpoint, victory and UI | **Met.** Rolling, landing, prop impacts, UI clicks, trampoline bounces, checkpoints, victory, game over, hazard hits, wet-floor slips and falls off the level all have sounds, plus per-scene music. |
| 4 | Agree a shared C# style | Written up in [code-style.md](code-style.md). |
| 4 | Documented QA checklist before submission (stretch) | [qa-checklist.md](qa-checklist.md). |
| 5 | Milestone presentation and video demonstration | **Done.** The video is `VIDEO_DEMO.mp4` in the repository root, linked from the README. The milestone presentation was given in person in class. |
