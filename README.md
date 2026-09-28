# Marble Madness 3D Clone

A 3D clone of *Marble Madness* built in Godot 4.7.1 (.NET / C#) for COSC2349 Games Studio 2, Assignment 3.

See [docs/requirements.md](docs/requirements.md) for the full requirements spec, [docs/project-plan-responsibilities.md](docs/project-plan-responsibilities.md) for team roles and the milestone schedule, and [docs/tile-convention.md](docs/tile-convention.md) for the tile grid and level-building rules.

## Requirements

- [Godot 4.7.1 .NET/Mono build](https://godotengine.org/download) (not the standard build — C# support requires the .NET version)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Setup

1. Clone the repository.
2. Open Godot 4.7.1 (.NET), choose **Import**, and select this folder's `project.godot`.
3. On first open, Godot will prompt to build the C# solution (`MarbleMadness3D.sln`) — accept this, or run `dotnet build` from the repo root.
4. Press **Run Project** (F5) to launch from the `Main` scene.

## Controls

| Input | Action |
|---|---|
| WASD / arrow keys | Roll the marble |

Full control scheme still to be finalized — see [docs/requirements.md §7](docs/requirements.md#7-open-items-to-confirm).

## Project Structure

```
assets/      Art, models, textures, materials, audio, fonts
resources/   Data resources (level layouts, tile definitions)
scenes/      .tscn scene files, grouped by system
scripts/     C# scripts, grouped by system
docs/        Design/requirements/planning documentation
```

Folder-to-system mapping follows [docs/requirements.md §3](docs/requirements.md#3-core-systems--functional-requirements):

| Folder | System | Owner |
|---|---|---|
| `scenes/marble/`, `scripts/Marble/` | Marble controller | Donovan |
| `scenes/camera/`, `scripts/Camera/` | Camera system | Donovan |
| `scenes/tiles/`, `resources/tiles/`, `scripts/Level/` | Tile / level system | Aaron |
| `scenes/levels/` | Individual levels | Aaron (integration) |
| `scenes/hazards/`, `scripts/Hazards/` | Hazards & obstacles | Andrew |
| `scripts/Level/` (timer, checkpoints, win/lose) | Timer / checkpoint / win-lose | Andrew |
| `scenes/ui/`, `scripts/UI/` | UI / HUD / menus | Jayden |
| `assets/audio/`, `scripts/Audio/` | Audio | Jayden |
| `scripts/Core/` | Cross-cutting game state (autoloads) | Shared |

## Known Issues

- Only a greybox prototype exists: the marble rolls around a CSG-built test course (`scenes/levels/level_01/Level01.tscn`). No hazards, timer, checkpoints, win/lose states, UI or audio yet.
- The camera is a fixed angled `Camera3D` and does not follow the marble yet.
- Tiles are CSG placeholders and will be replaced by final modelled assets.

## Team

- Aaron Tran
- Andrew Loi
- Donovan Ong
- Jayden Bach
