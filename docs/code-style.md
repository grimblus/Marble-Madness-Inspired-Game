# C# Code Style

The shared conventions for gameplay code, as asked for in [requirements.md §4](requirements.md). They're taken from what most of the codebase already does, so new code should match the scripts around it.

## Formatting

- Tabs for indentation (Godot's default).
- Braces on their own line (Allman style), including for single-line `if` bodies.
- One class per file, named after the file: `CameraRig.cs` holds `public partial class CameraRig`.
- Scripts live in `scripts/<System>/`, matching the system folders in the README.

## Naming

| Kind | Style | Example |
|---|---|---|
| Classes, methods, properties, exported members | PascalCase | `MarbleController`, `GetHitPhys()`, `FollowSpeed` |
| Private fields | `_camelCase` | `_target`, `_inSpinout` |
| Constants | PascalCase | `FadeSeconds`, `HitAreaScale` |
| Signals | PascalCase delegate ending in `EventHandler` | `DiedEventHandler`, raised with `EmitSignal(SignalName.Died)` |
| Input actions | `snake_case` | `move_forward`, `zoom_in` |

## Comments

- **Header comment on every script:** what the node is for, which scene it belongs to, and how it connects to other systems. It goes above the class.
- **A comment on every exported value** saying what it does and its unit (`// Seconds for one full sway.`), so it reads well in the Inspector.
- **Explain *why*, not *what*.** Comment the non-obvious reason, such as a Godot quirk or a bug a line prevents. Don't restate the code.

## Inspector values

- Expose tuning values with `[Export]` so they can be adjusted per scene without code changes. Give each one a sensible default.
- Either export style is fine (`[Export] public float Speed = 8.0f;` or `[Export] public float Speed { get; set; } = 8.0f;`). Match the file you're in.

## How systems talk to each other

- **Signals over direct references.** A system announces what happened (`TimeChanged`, `LevelWon`, `Died`) and others listen. For example, the timer doesn't know the HUD exists.
- **Autoloads only for game-wide state:** `GameStateManager` (win/lose), `CheckpointManager` (respawn point) and `MusicPlayer`. Don't add one for something that belongs to a single scene.
- **Unsubscribe in `_ExitTree`** from any autoload signal you subscribed to, so a level reload (Retry) doesn't leave handlers pointing at freed nodes.
- **Check shared state directly** rather than caching it from signals. For example, the marble reads `GameStateManager.IsEnded` itself instead of keeping its own copy, so it can never fall out of step.
- **Avoid hard-coded scene paths** such as `/root/Main/Marble`. Prefer an exported `NodePath`, or the `marble` group (`IsInGroup("marble")`), so scenes can be rearranged without breaking scripts.
- **UI nodes:** look them up by unique name (`GetNode<Button>("%StartButton")`) so layout changes don't break the script.

## Physics

- Move physics bodies with forces and impulses, never by setting position or velocity each frame. Teleports such as respawning are the exception.
- Use the named collision layers (World, Marble, Hazards, Triggers) from [tile-convention.md §6](tile-convention.md).
- Use frame-rate-independent smoothing (`1 - Mathf.Exp(-speed * delta)`) rather than a fixed-fraction lerp.
