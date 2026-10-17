# Third-Party Asset Credits

Third-party art assets used in the project, their source, author and licence. Required whenever a CC-licensed or other third-party asset is used — see [requirements.md](requirements.md) and [project-plan-responsibilities.md §4](project-plan-responsibilities.md) on individual/overall contribution and documentation standards.

## 3D Models

| Local file | Source | Title | Author | Licence |
|---|---|---|---|---|
| `assets/models/props/Aurelius2.glb` | [Sketchfab](https://sketchfab.com/3d-models/marcus-aurelius-36fd581851294110a7eda08ec2a20748) | Marcus Aurelius | The British Museum (photogrammetry by Daniel Pett) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/props/Horsey.glb` | [Sketchfab](https://sketchfab.com/3d-models/horse-head-british-museum-c004350560b74b2c8dca5d7b1d2228d4) | Horse Head, British Museum | The British Museum (photogrammetry by Daniel Pett) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/props/decimated_amenemhat.glb` | [Sketchfab](https://sketchfab.com/3d-models/granite-head-of-amenemhat-iii-64d0b7662b59417986e9d693624de97a) | Granite head of Amenemhat III | The British Museum (scan by Thomas Flynn) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/props/decimated_egyptian_head.glb` | [Sketchfab](https://sketchfab.com/3d-models/egyptian-head-3fb673e1a8314a4bab529f61726e53ce) | Egyptian Head | IU Indianapolis University Library | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) |
| `assets/models/hazards/decimated_well.glb` | [Sketchfab](https://sketchfab.com/3d-models/roman-well-head-d7bce96df89c4ba58c18116aba7cd2f4) | Roman Well-head | The British Museum (scan by Daniel Pett) | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| `assets/models/hazards/photorealistic_statue_of_kariatida.glb` | [Sketchfab](https://sketchfab.com/3d-models/photorealistic-statue-of-kariatida-4e5ac10fb8c347e0a6d7e2610a28cd39) | Photorealistic Statue of Kariatida | Soul_Warriors | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) |
| `assets/models/hazards/wet_floor_sign.glb` | [Sketchfab](https://sketchfab.com/3d-models/wet-floor-sign-d340f904c4684645a4e5282b5fbe963b) | Wet Floor Sign | louis-muir | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) |

Marcus Aurelius, the Horse Head, Amenemhat III and the Roman Well-head are 3D scans of objects in the British Museum's collection, published via the museum's Sketchfab account. The Egyptian Head is a scan of an object from the Indianapolis Public Library collection, published by IU Indianapolis University Library.

| `assets/models/objects/Vase-*.glb` (7 files) | [itch.io](https://pyromaster3000.itch.io/vase-asset-in-blend-file) | Vase-Asset in .blend file | pyromaster3000 | Name-your-own-price; free to use in games. **Uploading and redistribution of the asset are prohibited** (see below) |

The `objects/` vases are used by the knock-over vase props (`scenes/props/vases/`). The author's terms allow using the vases in a game but prohibit uploading or redistributing the asset itself. They're stored in this private team repository only to build the game, so don't make the repository public or share the raw model files elsewhere.

The `hazards/` models are used by the hazards: the Roman Well-head by the vacuum hazard (`vacuum_hazard.tscn`), the Kariatida statue by the moving hazard (`moving_hazard.tscn`), and the Wet Floor Sign by the stationary hazard (`stationary_hazard.tscn`).

**CC BY-NC-SA 4.0 terms to keep in mind** (Marcus Aurelius, Horse Head, Amenemhat III, Roman Well-head):
- **Attribution** — credit given above; keep this file if any of these models ships in the final build.
- **NonCommercial** — fine for this assignment (an academic, non-commercial submission).
- **ShareAlike** — any adaptation of these specific model files (re-texturing, remeshing, etc.) should carry the same licence if shared or redistributed on its own.

**CC BY 4.0** (Egyptian Head, Kariatida statue, Wet Floor Sign) only requires attribution, given above.

**Modifications:** Marcus Aurelius and the Horse Head are unmodified in geometry; only material/texture overrides (see [tile-convention.md §4](tile-convention.md), CSG/material conventions apply to tiles, not these props) have been applied where used in-scene. The three `decimated_` models (Amenemhat III, Egyptian Head, Roman Well-head) have been reduced in polygon count in Blender for performance. Those are adaptations, so the Amenemhat III and Roman Well-head files stay under CC BY-NC-SA 4.0, and all three credits should note that the model was modified.

## Music

All recordings sourced from [Musopen](https://musopen.org). The compositions themselves are public domain; each recording's licence is set per recording on its Musopen page and is recorded below. Performers are as credited on each recording's Musopen page or in the file's embedded tags.

| Local file | Piece | Composer | Performer | Source | Licence | Used in |
|---|---|---|---|---|---|---|
| `assets/audio/music/349_gymnopedie-no-1.mp3` | Gymnopédie No. 1 | Erik Satie | Anonymous | [Musopen](https://musopen.org/music/8010-3-gymnopedies/) | [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) | Start menu |
| `assets/audio/music/2411_arabesque-no-1mp3.mp3` | Arabesque No. 1 | Claude Debussy | Gerlüz (2010, Chicken House Records) | [Musopen](https://musopen.org/music/2491-two-arabesques-deux-arabesques-l-66/) | [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) | Level 1 |
| `assets/audio/music/2491_gnossienne.mp3` | Gnossienne No. 1 | Erik Satie | Edward Rosser | [Musopen](https://musopen.org/music/44070-gnossiennes-no-1/) | [Public Domain Mark 1.0](https://creativecommons.org/publicdomain/mark/1.0/) | Level 2 |
| `assets/audio/music/13_pavane.mp3` | Pavane pour une infante défunte | Maurice Ravel | Felipe Sarro (2009) | [Musopen](https://musopen.org/music/4709-pavane-for-a-dead-princess/) | [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) | Level 3 |

**CC BY-NC-SA 3.0 terms** (Gymnopédie No. 1, Arabesque No. 1, Pavane): credit the recording as above, non-commercial use only (fine for this assignment), and any edited version of a recording (trimmed, looped, remixed) must be shared under the same licence.

**Public Domain Mark 1.0** (Gnossienne No. 1): no restrictions and no attribution required. It is credited above anyway.

## Sound Effects

| Local file | Description | Source | Licence | Used in |
|---|---|---|---|---|
| `assets/audio/sfx/661650__solar01__glass-marbles-rolling-on-wood.wav` | Glass Marbles Rolling on Wood | [Freesound #661650](https://freesound.org/people/solar01/sounds/661650/) by Solar01 | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) (no attribution required) | Marble rolling loop (`Marble.tscn` → `RollSound`) |
| `assets/audio/sfx/240313__dsg__marble-drop.wav` | marble drop | [Freesound #240313](https://freesound.org/s/240313/) by D.S.G. | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) (no attribution required) | Marble landing clunk (`Marble.tscn` → `ImpactSound`) |

## Textures
| Local file | Source | Title | Author | Licence |
|---|---|---|---|---|
| `assets/textures/marble_107-1K/` | [ShareTextures](https://www.sharetextures.com/textures/floor/marble-107) | Marble 107 | M.Tolga Arslan | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) |
| `assets/textures/ground_4-1K/` | [ShareTextures](https://www.sharetextures.com/textures/ground/ground-4) | Ground 4 | M.Tolga Arslan | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) |
| `assets/textures/stone_wall_texture_2-1K/` | [ShareTextures](https://www.sharetextures.com/textures/wall/stone-wall-2) | Stone Wall Texture 2 | M.Tolga Arslan | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) |
| `assets/textures/pexels-this-and-no-internet-25-288559-35336001` | [Pexels](https://www.pexels.com/photo/modern-art-gallery-with-abstract-blue-paintings-35336001/) | This And No Internet 25 | photoholic | [Pexels License](https://www.pexels.com/license/) (free to use, no attribution required) |

**Not yet used:** the Pexels photo isn't referenced by any scene. Remove it, or note here where it's used once it is.

### VFX textures and meshes (sources needed)

These were added for the hazard visual effects. Their sources and licences aren't recorded yet. **Andrew, please fill these in**, or mark them as made by the team if you created them.

| Local file | Used by | Source | Licence |
|---|---|---|---|
| `assets/textures/T_VFX_WindNoise1.png` | Vacuum hazard wind effect (`scenes/hazards/vfx_vacuum_wind.tscn`) | *to confirm* | *to confirm* |
| `assets/models/hazards/vacuum_wind.obj` | Vacuum hazard wind effect | *to confirm* | *to confirm* |
| `assets/textures/T_CloudNoise_Tiled.png`, `T_Leaf_001.png`, `T_Leaf_001gray.png`, `T_NHU85.png`, `T_VFX_Noise_531.png`, `T_VFX_SparklesF21.jpg`, `T_arrows21.png` | Not used on `main` (possibly for the blower hazard on `feature/hazards`) | *to confirm* | *to confirm* |

## Fonts

| Local file | Source | Title | Author | Licence | Used in |
|---|---|---|---|---|---|
| `assets/fonts/diffusion-regular.ttf`, `assets/fonts/diffusion-bold.ttf` | [itch.io](https://amorphous.itch.io/diffusion) | Diffusion | Amorphous (a custom build of [Iosevka](https://github.com/be5invis/Iosevka)) | [SIL Open Font License 1.1](https://openfontlicense.org/) | Start menu, result screen |

**SIL OFL 1.1:** free to use and bundle in a game, including commercially. The font files can't be sold on their own, and any modified version of the font must be renamed.
