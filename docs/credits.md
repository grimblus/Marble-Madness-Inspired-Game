# Third-Party Asset Credits

Third-party art assets used in the project, their source, author and licence. Required whenever a CC-licensed or other third-party asset is used — see [requirements.md](requirements.md) and [project-plan-responsibilities.md §4](project-plan-responsibilities.md) on individual/overall contribution and documentation standards.

## 3D Models

| Local file | Source | Title | Author | Licence |
|---|---|---|---|---|
| `assets/models/props/Aurelius2.glb` | [Sketchfab](https://sketchfab.com/3d-models/marcus-aurelius-36fd581851294110a7eda08ec2a20748) | Marcus Aurelius | The British Museum (photogrammetry by Daniel Pett) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/props/Horsey.glb` | [Sketchfab](https://sketchfab.com/3d-models/horse-head-british-museum-c004350560b74b2c8dca5d7b1d2228d4) | Horse Head, British Museum | The British Museum (photogrammetry by Daniel Pett) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/props/decimated_amenemhat.glb` | [Sketchfab](https://sketchfab.com/3d-models/granite-head-of-amenemhat-iii-64d0b7662b59417986e9d693624de97a) | Granite head of Amenemhat III | The British Museum (scan by Thomas Flynn) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/props/decimated_egyptian_head.glb` | [Sketchfab](https://sketchfab.com/3d-models/egyptian-head-3fb673e1a8314a4bab529f61726e53ce) | Egyptian Head | IU Indianapolis University Library | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) |

The first three are 3D scans of objects in the British Museum's collection, published via the museum's Sketchfab account. The Egyptian Head is a scan of an object from the Indianapolis Public Library collection, published by IU Indianapolis University Library.

**CC BY-NC-SA 4.0 terms to keep in mind** (Marcus Aurelius, Horse Head, Amenemhat III):
- **Attribution** — credit given above; keep this file if any of these models ships in the final build.
- **NonCommercial** — fine for this assignment (an academic, non-commercial submission).
- **ShareAlike** — any adaptation of these specific model files (re-texturing, remeshing, etc.) should carry the same licence if shared or redistributed on its own.

**CC BY 4.0** (Egyptian Head) only requires attribution, given above.

**Modifications:** Marcus Aurelius and the Horse Head are unmodified in geometry; only material/texture overrides (see [tile-convention.md §4](tile-convention.md), CSG/material conventions apply to tiles, not these props) have been applied where used in-scene. The two `decimated_` models have been reduced in polygon count in Blender for performance. Those are adaptations, so the Amenemhat III file stays under CC BY-NC-SA 4.0, and both credits should note that the model was modified.

## Music

All recordings sourced from [Musopen](https://musopen.org). The compositions themselves are public domain; each recording's licence is set per recording on its Musopen page and still needs to be recorded below. Performers are taken from each file's embedded tags, where present.

| Local file | Piece | Composer | Performer | Source | Licence | Used in |
|---|---|---|---|---|---|---|
| `assets/audio/music/349_gymnopedie-no-1.mp3` | Gymnopédie No. 1 | Erik Satie | *not in file tags, check source page* | [Musopen](https://musopen.org/music/8010-3-gymnopedies/) | *to confirm* | Start menu |
| `assets/audio/music/2411_arabesque-no-1mp3.mp3` | Arabesque No. 1 | Claude Debussy | Gerlüz (2010, Chicken House Records) | [Musopen](https://musopen.org/music/2491-two-arabesques-deux-arabesques-l-66/) | *to confirm* | Level 1 |
| `assets/audio/music/2491_gnossienne.mp3` | Gnossienne No. 1 | Erik Satie | *not in file tags, check source page* | [Musopen](https://musopen.org/music/44070-gnossiennes-no-1/) | *to confirm* | Not yet assigned |
| `assets/audio/music/13_pavane.mp3` | Pavane pour une infante défunte | Maurice Ravel | Felipe Sarro (2009) | [Musopen](https://musopen.org/music/4709-pavane-for-a-dead-princess/) | *to confirm* | Not yet assigned |

## Sound Effects

| Local file | Description | Source | Licence | Used in |
|---|---|---|---|---|
| `assets/audio/sfx/661650__solar01__glass-marbles-rolling-on-wood.wav` | Glass Marbles Rolling on Wood | [Freesound #661650](https://freesound.org/people/solar01/sounds/661650/) by Solar01 | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) (no attribution required) | Marble rolling loop (`Marble.tscn` → `RollSound`) |
| `assets/audio/sfx/240313__dsg__marble-drop.wav` | marble drop | [Freesound #240313](https://freesound.org/s/240313/) by D.S.G. | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) (no attribution required) | Marble landing clunk (`Marble.tscn` → `ImpactSound`) |

## Fonts

- `assets/fonts/diffusion-bold.ttf`, `assets/fonts/diffusion-regular.ttf` — source/licence not yet recorded. *(Whoever added these to `scenes/ui/menus/startMenu.tscn`, please fill in the source and licence here.)*


## Textures
| Local file | Source | Title | Author | Licence |
|---|---|---|---|---|
| `assets/textures/marble_107-1K/` | [ShareTextures](https://www.sharetextures.com/textures/floor/marble-107) | Marble 107 | M.Tolga Arslan | [CC0] |
| `assets/textures/ground_4-1K/` | [ShareTextures](https://www.sharetextures.com/textures/ground/ground-4) | Ground 4 | M.Tolga Arslan | [CC0] |
| `assets/textures/stone_wall_texture_2-1K/` | [ShareTextures](https://www.sharetextures.com/textures/wall/stone-wall-2) | Stone Wall Texture 2 | M.Tolga Arslan | [CC0] |
| `assets/textures/pexels-this-and-no-internet-25-288559-35336001` | [Pexels](https://www.pexels.com/photo/modern-art-gallery-with-abstract-blue-paintings-35336001/) | This And No Internet 25 | photoholic | [Pexels License] |
