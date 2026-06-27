# Setup & build guide

## 1. Unity version & pipeline

- **Unity 2022 LTS** or **Unity 6 LTS**.
- **URP** is recommended (the design targets URP). The scripts also compile and run under
  the Built-in pipeline; generated materials fall back to the `Standard` shader when URP's
  `Universal Render Pipeline/Lit` shader is not found.

If you are starting from scratch:
1. Create a new project with the **3D (URP)** template.
2. Copy this repository's `Assets/` folder into the new project (merging with the existing
   `Assets/`), or open this folder directly as the Unity project root.
3. Let Unity import and compile.

## 2. Generate the game

In the Unity menu bar:

- **`SurvivalMoba > Build Everything`** — runs all three builders and registers the scenes.

Or run them individually:

1. `SurvivalMoba > 1. Build Content (Data + Prefabs)` → creates `Assets/Generated/`:
   - `Data/` — Hero, Abilities, Enemies, Waves, 10 Upgrades (ScriptableObjects).
   - `Prefabs/` — projectiles, frost zone, VFX, XP orb, damage number, enemy prefabs.
   - `Materials/` — flat colour materials for the placeholder primitives.
2. `SurvivalMoba > 2. Build Game Scene` → `Assets/Scenes/GameScene.unity`, fully wired:
   world, isometric camera, player, all managers, and the complete mobile HUD.
3. `SurvivalMoba > 3. Build Main Menu` → `Assets/Scenes/MainMenu.unity` + Build Settings.

> Re-running the builders is safe: data assets are updated in place, prefabs and scenes are
> regenerated.

## 3. Play in the editor

- Open `Assets/Scenes/MainMenu.unity` → press **Play** → **Play** button → match starts.
- Or open `GameScene.unity` directly.
- Editor testing: **WASD** moves the hero in addition to the on-screen joystick.

## 4. Android build settings (recommended)

`File > Build Settings > Android > Switch Platform`, then in `Player Settings`:

- **Minimum API level**: Android 7.0 (API 24) or higher.
- **Scripting backend**: IL2CPP; **Target architectures**: ARM64.
- **Graphics APIs**: OpenGLES3 (and/or Vulkan).
- **Orientation**: Landscape (the HUD is laid out for landscape).
- **Color space**: Linear (for URP).

Quality / performance (matches the design's Android plan):

- Disable real-time shadows on low-end, or use a single soft directional shadow on mid/high.
- Keep the per-device enemy cap via `EnemySpawner.hardMaxAlive` (default 120; use 60 for
  low-end, up to 200 for high-end).
- Object pooling is already used for all high-frequency objects.

## 5. Tuning knobs

| Where | What |
|-------|------|
| `Assets/Generated/Data/Waves_FiveMinute` | Wave timeline, spawn rates, caps, boss time, match length |
| `Assets/Generated/Data/Hero_ArcaneHunter` | Hero base stats and ability references |
| `Assets/Generated/Data/Ability_*` | Per-ability damage, cooldown, range, radius, slow, meteor count |
| `Assets/Generated/Data/Enemy_*` | Enemy health/damage/speed/XP |
| `Assets/Generated/Data/Upgrade_*` | Upgrade effect type, value, max stacks |
| `EnemySpawner` (in GameScene) | Spawn ring radius, global enemy cap |
| `XPSystem` (in GameScene) | XP curve base + growth |

## 6. Troubleshooting

- **"Run Build Content first" dialog** when building the scene → run step 1 before step 2.
- **Pink materials** → URP active but materials created under Built-in (or vice-versa);
  re-run `1. Build Content` so materials are recreated against the active pipeline's shader.
- **No enemies / nothing happens** → ensure the `Managers` object exists in the scene and
  `WaveManager.waveData` is assigned (the builder does this automatically).
- **Buttons don't respond** → confirm an `EventSystem` exists (the builder creates one).
