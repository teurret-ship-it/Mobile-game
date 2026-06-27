# Arcane Survivors — 2.5D Isometric Android Survival MOBA (MVP)

An original Unity mobile game: a 2.5D isometric arena where one hero survives endless
enemy waves using MOBA-style controls — a virtual joystick, an auto-targeting basic
attack, three skillshot/area/dash abilities and an ultimate — gaining XP, levelling up,
choosing upgrades and finally facing a boss.

This repository is the implementation of the design in
[`docs/isometric_survival_moba_plan.md`](docs/isometric_survival_moba_plan.md). It contains
the complete MVP **codebase** plus **editor builders** that generate the data assets,
placeholder prefabs and a fully wired, playable scene — so you can open it in Unity and
press Play without any manual setup.

> All content here is original. No copyrighted characters, names, art, audio or assets
> from any existing game are included; the placeholder art is generated from Unity
> primitives.

---

## Quick start

1. **Create / open the Unity project**
   - Use **Unity 2022 LTS** or **Unity 6 LTS** with the **URP** template (recommended).
   - If you start a fresh project, copy this repo's `Assets/` folder into it (or open this
     folder directly as the project root). The scripts compile under URP or the Built-in
     pipeline — materials fall back to the `Standard` shader if URP is not present.

2. **Generate content and the playable scene**
   - In the Unity menu bar, choose **`SurvivalMoba > Build Everything`**.
   - This runs three steps:
     1. `1. Build Content` — creates ScriptableObjects + placeholder prefabs in `Assets/Generated/`.
     2. `2. Build Game Scene` — assembles `Assets/Scenes/GameScene.unity` (world, player, managers, full HUD).
     3. `3. Build Main Menu` — creates `Assets/Scenes/MainMenu.unity` and registers both scenes in Build Settings.

3. **Play**
   - Open `Assets/Scenes/MainMenu.unity` (or `GameScene.unity`) and press **Play**.
   - In the editor you can also drive the hero with **WASD** for quick testing.

4. **Build to Android**
   - `File > Build Settings > Android > Switch Platform`, then `Build` (see
     [`docs/SETUP.md`](docs/SETUP.md) for recommended player/quality settings).

---

## Controls

| Input | Action |
|-------|--------|
| Left virtual joystick | Move the hero on the isometric floor |
| Basic attack button (`A`) | Force a basic attack (the hero also auto-attacks the nearest enemy) |
| Ability buttons `1` / `2` / `3` | Cast Piercing Beam / Frost Zone / Dash Strike — **drag to aim**, release to cast; quick tap auto-aims |
| Ultimate button `R` | Meteor Storm |
| Pause button | Pause / resume |

---

## The MVP hero — *Arcane Hunter*

| Slot | Name | Type | Notes |
|------|------|------|-------|
| Basic | Magic Bolt | Auto-target projectile | 10 dmg, 0.8s cd, range 6 |
| Ability 1 | Piercing Beam | Line skillshot | 30 dmg, pierces all in a line |
| Ability 2 | Frost Zone | Area placement | 10 dmg/tick, slows 40% for 4s |
| Ability 3 | Dash Strike | Dash | Dashes 4 units, 20 dmg along the path |
| Ultimate | Meteor Storm | Area ultimate | 12 meteors over 5s around the hero |

Enemies: **Grunt**, **Runner**, **Brute**, and the **Corrupted Guardian** boss (spawns at 4:15).
Survive the 5-minute timeline or kill the boss to win.

---

## Architecture overview

Data-driven design using ScriptableObjects, with runtime systems wired through small
singletons and C# events. See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full map.

```
Assets/Scripts/
  Core/        GameEnums, IDamageable, GameManager
  Player/      PlayerCharacter, PlayerController, PlayerStats, CameraFollow
  Input/       MobileJoystick
  Combat/      Projectile, BasicAttack
  Abilities/   AbilitySystem, AbilityRuntime, AreaEffectZone, AimIndicator
  Enemies/     EnemyController, EnemyRegistry, EnemySpawner, WaveManager
  XP/          XPOrb, XPSystem, LevelUpManager, UpgradeManager
  Systems/     HealthSystem, ObjectPool, PoolManager, SettingsController
  UI/          UIManager, AbilityButton, BasicAttackButton, UpgradeCard,
               FloatingDamageNumber, DamageNumberService, SceneLoader
  Data/        HeroData, AbilityData, EnemyData, WaveData, UpgradeData

Assets/Editor/
  ContentBuilder, GameSceneBuilder, MainMenuBuilder   (menu: SurvivalMoba/...)
```

Key ideas:

- **Object pooling everywhere** (`PoolManager`) for enemies, projectiles, XP orbs, VFX and
  damage numbers — no Instantiate/Destroy churn during play.
- **`EnemyRegistry`** keeps a live list of enemies so "nearest enemy" / radius queries are
  cheap (no `FindObjectsOfType` per frame).
- **`AbilitySystem`** resolves all ability behaviours from `AbilityData` via an effect enum,
  so most new abilities are new assets, not new code.
- **`PlayerStats`** computes effective stats from the hero plus stacked upgrades; UI and
  systems always read the up-to-date value.

---

## Extending the game

- **New upgrade:** `Assets > Create > SurvivalMoba > Upgrade Data`, then add it to the
  `UpgradeManager.upgradePool` in the scene.
- **New enemy:** create an `EnemyData` asset + a prefab with `HealthSystem` + `EnemyController`,
  then reference it from a `WaveData` entry.
- **New ability:** create an `AbilityData` asset, pick its targeting + effect type, and
  assign the prefab it spawns (a `Projectile`, an `AreaEffectZone`, or an impact VFX).

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) and [`docs/SETUP.md`](docs/SETUP.md) for details.
