# Pre-Submission QA Checklist

Run this before the final submission (Week 6, 22–25 Oct), and again after any late fix. It covers what a marker will do first: clone the repo fresh and play it. Tick each item; if one fails, fix it or add it to the README's Known Issues.

## 1. Fresh clone

- [x] Clone the repository into a new folder (not your working copy).
- [x] Open it in **Godot 4.7.1 .NET** with **.NET SDK 8** installed. The C# solution builds with no errors (`dotnet build`).
- [x] No missing-resource or broken-dependency warnings on import.
- [x] **Run Project** (F5) opens the start menu.
- [x] No asset the game needs is excluded by `.gitignore`.

## 2. Every level, start to finish

For each playable gallery:

- [x] The marble starts at the level's start marker, and the camera follows it.
- [x] The goal can be reached, and the win screen shows the time and stars.
- [x] Falling off returns the marble to the last checkpoint passed, or to the start.
- [x] Each hazard reacts as described in the design document §7.
- [x] Knock-over props topple and settle.
- [x] The level's music plays; rolling and landing sounds work; checkpoints, hazard hits, slips, trampoline bounces and falls play their sounds, and the goal and game over play their jingles.

## 3. Win, lose and retry

- [x] **Win:** stars match the gallery's star times in `LevelCatalog` (3 and 2 stars under 30 / 60 s, 50 / 80 s and 1:15 / 2:00 for Galleries 01–03; 1 star under 5:00), and the best time is saved.
- [x] **Lose:** at 5:00 the run ends with "Timer expires".
- [x] **Retry** restarts the level cleanly: timer at zero, marble at the start, nothing frozen.
- [x] **Next level** and **Level Select** work from the win screen, and the next gallery unlocks after a 1-star clear.

## 4. Menus never dead-end

- [x] Start menu: every button works, and **Back** returns from How to Play and Settings.
- [x] The Settings volume slider changes the master volume (music and sound effects).
- [x] Pause (Esc): Resume, Restart and Main Menu all work. Pause is unavailable once the level has ended.
- [x] Every screen works with both the mouse and the keyboard.
- [x] The UI looks right at 1280 × 720, 1920 × 1080 and a non-16:9 window size.

## 5. Performance

- [x] Each level holds 60 FPS or better in a normal (non-editor) run. Use the Godot profiler or the FPS monitor. Record the numbers in the design document §12.
- [x] No errors or warnings in the Output panel during a full playthrough.

## 6. Repository and documentation

- [x] `main` is the default branch. Everything that's meant to ship is merged, and nothing broken is on `main`.
- [x] The README's setup steps, controls and known issues match the game.
- [x] Every "In progress" note in the docs (`grep -n "In progress" docs/*.md`) has been either finished or moved to Known Issues.
- [x] Every third-party asset is in [credits.md](credits.md) with its source and licence. No "*to confirm*" entries remain.
- [x] The video demonstration is linked from the README.
- [ ] The **Tests** workflow is green for the final commit on GitHub (both the unit tests and the Godot smoke test). See [testing.md](testing.md).
