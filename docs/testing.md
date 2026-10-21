# Automated Tests

The project has two automated test suites. GitHub Actions runs both on every push and pull request ([`.github/workflows/tests.yml`](../.github/workflows/tests.yml)), and either can be run locally. They sit alongside the manual [QA checklist](qa-checklist.md): the tests catch broken wiring and missing files on every push, and the checklist covers what only a person can judge, such as how the marble feels and whether each route is fair.

| Suite | Location | Runs in | What it covers | Time |
|---|---|---|---|---|
| Unit tests | `tests/unit/` | .NET only (xUnit), no Godot needed | Level list and star times, time formatting, missing files | A few seconds |
| Smoke test | `tests/smoke/` | Godot 4.7.1 .NET, headless | Every level loaded into the real game shell: start, checkpoint, fall, goal, and both menus | About a minute in CI, mostly downloading Godot |

## Unit tests

An xUnit project (`tests/unit/MarbleMadness3D.Tests.csproj`) that references the game's C# project and tests its plain C# logic, plus the project's files on disk.

| Test file | Checks |
|---|---|
| `LevelCatalogTests.cs` | Level ids are unique; every level's scene exists; star times are positive, in order (three-star under two-star) and inside the 5-minute limit set in `Main.tscn`; star times never get shorter from one gallery to the next; Gallery 01 keeps the 30 / 60 s times shown in the video; `StarsFor` grades correctly at the boundaries (exactly 30 s is two stars, not three); next and previous level follow the catalog order. |
| `TimeFormatTests.cs` | The HUD stopwatch format (`01:47.10`), including that it never shows a negative time, and the short star-time format (`1:15`). |
| `ProjectFileTests.cs` | Every `res://` path in a scene, resource, script or `project.godot` points to a file that exists, so a moved or deleted asset fails the build before anyone opens the editor. The main scene and the four autoloads are registered. |

`RepoPaths.cs` is a helper that finds the repository root from the test runner's build folder.

The star-grading rule lives in `LevelCatalog.LevelEntry.StarsFor` rather than inside the result screen, so it can be tested without running Godot. The result screen calls it.

## Smoke test

`tests/smoke/SmokeTest.tscn` runs the actual game in headless Godot. For each level in `LevelCatalog`, it loads `Main.tscn` the same way the level select does, then checks that:

1. the level has a `PlayerStart` marker, a goal, a kill zone and at least one checkpoint;
2. the marble starts at `PlayerStart` and the level isn't already over;
3. passing the first checkpoint moves the respawn point there;
4. falling into the kill zone kills the marble and respawns it at that checkpoint, without ending the level;
5. reaching the goal ends the level and shows the result screen.

It then opens the start menu and level select. Any error or warning Godot logs during the test also counts as a failure. The leak report Godot prints as it shuts down is not counted. The test exits with code 0 if everything passed and 1 otherwise, and prints a list of failures.

The marble is moved by teleporting it into each trigger, not by rolling it. So the smoke test proves each level is wired up correctly, not that its route can be completed or that hazards are tuned well. Those stay on the manual checklist.

Winning a level saves a best time, so the smoke test backs up the player's `user://progress.cfg` before it starts and restores it afterwards. Running it locally doesn't change your saved stars.

## Running the tests locally

Unit tests (needs the .NET SDK, version 8 or later):

```bash
dotnet test tests/unit
```

Smoke test (needs Godot 4.7.1 .NET). Build the C# project first, so Godot runs the latest code:

```bash
dotnet build MarbleMadness3D.csproj
godot --headless --path . res://tests/smoke/SmokeTest.tscn
```

On macOS, `godot` is `/Applications/Godot_mono.app/Contents/MacOS/Godot`. On a fresh clone, run `godot --headless --path . --import` once first so the assets are imported.

## In GitHub Actions

The workflow has two jobs, shown as **Unit tests** and **Godot smoke test** on the repository's **Actions** tab and next to each commit:

- **Unit tests:** installs .NET 8 and runs `dotnet test tests/unit`.
- **Godot smoke test:** downloads Godot 4.7.1 .NET for Linux, builds the C# project, imports the assets and runs the smoke test. Godot and the imported assets are cached between runs, so later runs skip the download and only reimport assets that changed.

A failing job turns the commit's check red. Open the job's log to see which test failed and why.

## Adding tests

- **Plain C# logic** (no Godot nodes): add an xUnit test class in `tests/unit/`. Code that only uses Godot's math types (`Mathf`, `Vector3`) works there too. Code that creates or calls Godot nodes doesn't, because the engine isn't running.
- **Anything that needs the running game:** add a check to `SmokeTest.cs`, using `Check(condition, context, description)` so it's reported with the others.
- **A new level:** add it to `LevelCatalog` as usual. Both suites pick it up automatically.

`tests/unit/` is excluded from the game's own build (`MarbleMadness3D.csproj`) and hidden from the Godot editor by a `.gdignore` file. The smoke test is part of the game's build, because Godot can only run scripts it compiled.
