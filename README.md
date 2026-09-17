# Marble Madness 3D Clone

A 3D clone of *Marble Madness* built in Godot 4.7.2 (.NET / C#) for COSC2349 Games Studio 2, Assignment 3.

See [docs/requirements.md](docs/requirements.md) for the full requirements spec and [docs/project-plan-responsibilities.md](docs/project-plan-responsibilities.md) for team roles and the milestone schedule.

## Requirements

- [Godot 4.7.2 .NET/Mono build](https://godotengine.org/download) (not the standard build — C# support requires the .NET version)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Setup

1. Clone the repository.
2. Open Godot 4.7.2 (.NET), choose **Import**, and select this folder's `project.godot`.
3. On first open, Godot will prompt to build the C# solution (`MarbleMadness3D.sln`) — accept this, or run `dotnet build` from the repo root.
4. Press **Run Project** (F5) to launch from the `Main` scene.

## Controls

_To be finalized during Week 1 prototyping — see [docs/requirements.md §7](docs/requirements.md#7-open-items-to-confirm)._

## Project Structure

```
assets/      Art, models, textures, materials, audio
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

_None yet — project scaffolding only, no gameplay implemented._
