# Assignment 3 — Project Plan & Team Responsibilities

**Team:** Donovan Ong, Aaron Tran, Andrew Loi, Jayden Bach
**Timeline:** 17 September – 25 October 2026 (~5.5 weeks)
**Reference:** see `requirements.md` for full technical scope

## 1. Role Breakdown

### Donovan Ong — Head of Core Gameplay Mechanics
- Owns the marble controller: physics tuning, input handling, movement feel
- Owns the camera system
- Sets and enforces the "core game feel" bar — reviews any PR that touches marble physics or camera
- Performance profiling of the core gameplay loop (marble + camera + physics tick)
- Coordinates milestone presentation content and final documentation compilation
- Also built the knock-over props, the see-through marble and backdrop fading, the game-event sounds, the Gallery 03 layout, and the later development of all three galleries (vases and hazard placement), and ran the pre-submission code audit

### Aaron Tran — Technical Lead / Level Systems
- Owns the tile/level system: the modular tile kit and the GridMap mesh libraries that levels are built from
- Extended the starting greybox tiles with curve, half-width, ramp-corner, steep-ramp, wall and goal pieces, in three themed sets (default, Marble, Sand), each with its own mesh library. The team later chose the default colour-coded set for every gallery, and the Marble and Sand sets were removed before submission
- Owns level layout and integration: rebuilt Level 1 on a GridMap, built Level 2, and set up Level 3's GridMap and backdrop (its layout was then built by Donovan)
- Keeps levels to the shared grid and connection rules in [tile-convention.md](tile-convention.md)

### Andrew Loi — Gameplay Systems (Hazards, Timer, Checkpoints)
- Owns hazards and obstacles. Built so far:
  - the caryatid **moving hazard**, which follows a configurable Path3D patrol route
  - the **vacuum hazard** (well), which pulls the marble in with an area force and has wind VFX
  - the **stationary hazard** (wet floor), which spins the marble out
  - the **blower**, which pushes the marble away with the vacuum's wind reversed
  - the **trampoline**, which launches the marble upwards
- Owns how the marble reacts to hazards: on contact, the marble briefly loses control and gets knocked away
- Sourced the hazard models (Roman well-head, Kariatida statue, wet floor sign)
- Owns the timer, checkpoint and win/lose rules. The base versions were built early by Donovan to unblock testing

### Jayden Bach — Polish, UI & Support
- Owns all UI and menus:
  - the start menu (museum-themed redesign and fonts)
  - the pause menu (Esc; resume, restart and main menu; disabled once the level ends)
  - the in-level HUD
  - the results screen: 1–3 stars by completion time, with each level's best time and stars saved
  - the level select, with level previews and unlocking in order
  - the 3D gallery backdrop behind the start menu
- Changed the level timer from a countdown to a stopwatch, with an optional time limit
- Built level loading: `GameSession` remembers the chosen level and `LevelLoader` loads it into the shared main scene
- Owns audio. Donovan's Week 4 music player and marble sounds are the starting point for the audio polish pass

**Note:** every system above has a documented owner but nothing is a single point of failure — see §3.

## 2. Milestones

| Week | Dates | Milestone | Status (20 Oct) |
|---|---|---|---|
| 1 | 17–23 Sep | Prototyping: basic marble controller (Donovan), tile system + repo/project structure set up (Aaron), basic hazard prototype (Andrew), UI wireframes + asset gathering (Jayden). Control scheme locked in by end of week. | Done. Direct-force control locked in. |
| 2 | 24–30 Sep | Core systems integrated into one test level. Camera finalized. | Done. |
| 3 | 1–7 Oct | Level 1 complete and fully playable (physics, hazards, checkpoints, timer, win/lose all working end-to-end). | Done. Level 1 is playable end-to-end with its hazards placed. |
| 4 | 8–14 Oct | Level 2 built; Level 3 blocked out. Audio and UI polish pass begins. Design document drafting starts. | Done. |
| 5 | 15–21 Oct | Level 3 complete. Full polish pass (audio, visuals, juice). Playtesting across all levels. README and design doc finished. Milestone presentation prepared. | Done. Level 3 is built, all sound effects are in, the README and design document are complete, and a code and documentation audit was run against the requirements. The milestone presentation was given in person in class. |
| 6 | 22–25 Oct | Buffer for bug fixes only — no new features. Video demonstration recorded and edited. Fresh clone tested end-to-end to confirm it runs cleanly for the marker. Submission. | In progress. Video demonstration recorded (`VIDEO_DEMO.mp4` in the repository root). Next: the fresh-clone test with [qa-checklist.md](qa-checklist.md), and submission. |

Week 6 is deliberately kept as pure buffer — no feature work should be scheduled into it.

## 3. Risk & Dependency Management

To keep the critical path resilient to any one person's availability slipping:

- **No task is a strict single point of failure.** Aaron and Donovan both have working knowledge of the physics/level integration point since they meet there every week; Andrew's hazard system is designed to be swappable/stubbable so a level can ship with placeholder hazards if needed and be upgraded later.
- **Polish and UI/audio work (Jayden's scope) is intentionally scheduled to run in parallel, not gate the critical path.** Levels are playable end-to-end with placeholder UI/audio from Week 3 onward, so a slip in polish work doesn't block anyone else.
- **Weekly check-in** (suggest a fixed time each week) to catch slippage early — if a milestone item is at risk, reassign or descope rather than discovering it in Week 5.
- **Week 6 buffer** absorbs any late slippage without threatening the deadline.
- Aim to get each level to a "rough but complete" state before polishing any single level to perfection — protects against running out of time with zero finished levels.

## 4. Individual Contribution Tracking

Since this is graded with individual responsibility, keep a lightweight running log (commit history + a shared doc noting who did what each week) from Week 1 — much easier than reconstructing it in Week 6 for the documentation deliverable.
