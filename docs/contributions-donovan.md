# Individual Contribution Log — Donovan Ong

Running log of individual work, kept from Week 1 per [project-plan-responsibilities.md §4](project-plan-responsibilities.md). Covers work outside what's visible from commit history alone (design decisions, rationale, in-progress learning), plus a pointer to the relevant commits.

**Role:** Head of Core Gameplay Mechanics — marble controller, camera system, performance profiling, milestone/documentation coordination.

## Week 1 (17–23 Sep 2026)

- **Requirements spec authorship and extension** — drafted/extended [requirements.md](requirements.md), including adding two stretch goals: phone-gyroscope tilt control as an alternate input scheme (§3.1), and photogrammetry-sourced models/textures for the marble and level tiles (§3.3), each scoped with explicit caveats (latency/calibration risk for gyro input; poly-budget/FPS risk for photogrammetry) so they're clearly additive rather than core commitments.
- **Repository and Godot project scaffold** — set up the initial Godot 4.7.x (.NET/C#) project from scratch: `project.godot`, `MarbleMadness3D.csproj`/`.sln`, `.gitignore`, and a per-system folder structure (`assets/`, `resources/`, `scenes/`, `scripts/`) mapped directly to the ownership split in [project-plan-responsibilities.md §1](project-plan-responsibilities.md). Verified the SDK/target-framework pairing (`Godot.NET.Sdk` → `GodotSharp` → `net8.0`) actually resolves and builds via `dotnet build` before committing it, rather than assuming version numbers. Later reconciled the SDK version (4.7.2 → 4.7.1) and cleaned up the editor-generated `.csproj.old` backup once the project was opened in the team's actual installed Godot editor.
- **README** — wrote initial setup/run instructions, folder-to-system-to-owner mapping table, and controls/known-issues placeholders, satisfying the baseline for the README documentation deliverable (§5.2 of requirements.md) early rather than leaving it to Week 5.
- **Git workflow** — established the working pattern for this repo: commit the first/foundational changes directly to `main`, but branch (`chore/project-scaffold`) for the first substantive chunk of scaffolding work and merge it back via a reviewed step, in line with the branch/PR rule in requirements.md §4.
- **Marble controller — design/prototyping start** — worked through the physics-body setup for the marble controller (RigidBody3D root, separate CollisionShape3D/SphereShape3D from MeshInstance3D/SphereMesh so the placeholder sphere can be swapped for a modeled/photogrammetry asset later without touching physics or code), Continuous CD for tunnelling prevention, Input Map wiring, and a first pass at a direct-force `MarbleController.cs` (`ApplyCentralForce` + speed cap), per the direct-force control scheme requirements.md recommends as the primary target. This satisfies the Week 1 milestone item "basic marble controller" and feeds the "control scheme locked in by end of week" target in [project-plan-responsibilities.md §2](project-plan-responsibilities.md).

## Notes

- This log is a supplement to `git log`, not a replacement — see commit history for exact diffs and timestamps.
- Update this file weekly (or as work happens) rather than reconstructing it later.
