# Architecture

This document maps the runtime systems and how they communicate. The codebase favours
small, single-responsibility components wired through a few singletons and C# events,
with gameplay content driven by ScriptableObjects.

## Layers

### Data (ScriptableObjects) — `Assets/Scripts/Data`
- **HeroData** — base stats + references to the basic attack and four abilities.
- **AbilityData** — one ability definition. `TargetingType` (how it is aimed) +
  `AbilityEffectType` (how it resolves). `vfxPrefab` doubles as the spawned object:
  a `Projectile`, an `AreaEffectZone`, or an impact VFX, depending on the effect.
- **EnemyData** — stats, XP reward and the prefab to spawn.
- **WaveData** — an ordered timeline of `WaveEntry` (time → enemy/rate/cap), plus match length.
- **UpgradeData** — a level-up choice mapped to an `UpgradeEffectType`.

### Core — `Assets/Scripts/Core`
- **GameManager** (singleton) — match timer, `GameState`, pause/resume (via `Time.timeScale`),
  victory/defeat. Emits `StateChanged`, `TimerTick`, `Victory`, `Defeat`.
- **IDamageable** — the contract projectiles/abilities use to deal damage and apply slows.
- **GameEnums** — shared enums.

### Player — `Assets/Scripts/Player`
- **PlayerCharacter** (singleton) — builds `PlayerStats` from `HeroData`, owns the
  `HealthSystem`, reports death to `GameManager`.
- **PlayerStats** — effective stats computed from base + stacked upgrades; raises `StatsChanged`.
- **PlayerController** — joystick → camera-relative movement on the X/Z plane.
- **CameraFollow** — translates the fixed-angle isometric camera to follow the hero.

### Combat & Abilities — `Assets/Scripts/Combat`, `Assets/Scripts/Abilities`
- **BasicAttack** — auto-targets the nearest enemy (via `EnemyRegistry`) and fires pooled
  `Projectile`s; honours extra-projectile and pierce upgrades.
- **Projectile** — pooled straight-flyer; damages the opposing faction, supports pierce + slow.
- **AbilitySystem** — owns the four ability slots and the aiming flow; resolves each
  `AbilityEffectType` (projectile, piercing line, area-over-time, dash, meteor storm).
- **AbilityRuntime** — per-slot cooldown wrapper around an `AbilityData`.
- **AreaEffectZone** — pooled persistent ground effect (e.g. Frost Zone) that ticks damage/slow.
- **AimIndicator** — shows a line or circle reticle while dragging an ability button.

### Enemies — `Assets/Scripts/Enemies`
- **EnemyController** — chase, contact damage, death → drop XP + despawn; boss death → victory.
- **EnemyRegistry** (static) — live enemy list for nearest/radius queries.
- **EnemySpawner** — spawns the active enemy type in a ring around the player under a cap.
- **WaveManager** — walks the `WaveData` timeline, reconfigures the spawner, spawns the boss.

### XP & progression — `Assets/Scripts/XP`
- **XPOrb** — pooled magnet pickup.
- **XPSystem** (singleton) — XP curve, level, `XPChanged` / `LeveledUp`.
- **LevelUpManager** — on level-up, pauses and asks UI to present rolled upgrade choices.
- **UpgradeManager** — owns the upgrade pool, rolls 3 choices, tracks stacks, applies picks.

### Systems — `Assets/Scripts/Systems`
- **HealthSystem** — reusable health/slow container implementing `IDamageable`.
- **ObjectPool** / **PoolManager** — pooling; `PoolManager` lazily makes one pool per prefab.
- **SettingsController** — music/SFX/quality persisted via PlayerPrefs.

### UI — `Assets/Scripts/UI`
- **UIManager** — binds HUD (health, XP, level, timer, boss bar), the level-up panel and the
  victory/defeat/pause screens to the systems that drive them.
- **AbilityButton** / **BasicAttackButton** — forward touch input; show cooldown overlays.
- **UpgradeCard** — one selectable level-up card.
- **FloatingDamageNumber** / **DamageNumberService** — optional pooled damage numbers.
- **SceneLoader** — menu / restart / quit.

## Communication patterns

- **Singletons** for the few global hubs: `GameManager`, `PoolManager`, `XPSystem`,
  `UpgradeManager`, `PlayerCharacter`, `DamageNumberService`.
- **C# events** for fan-out (health changed, XP changed, level up, boss spawned, state
  changed) so UI and gameplay stay decoupled.
- **Static registry** (`EnemyRegistry`) for hot spatial queries.
- **`Time.timeScale = 0`** to freeze gameplay for level-up and pause; per-system `IsPlaying`
  guards prevent input/AI from running while frozen.

## Frame flow (simplified)

```
PlayerController        -> reads joystick, moves hero
BasicAttack             -> auto-fires at nearest enemy (pooled projectile)
AbilitySystem           -> ticks cooldowns; casts on button release
EnemySpawner/WaveManager-> spawn enemies per the timeline
EnemyController          -> chase + contact damage; on death drop XP orb
XPOrb -> XPSystem        -> XP, level up -> LevelUpManager pauses -> UI shows upgrades
UpgradeManager           -> applies pick to PlayerStats -> resume
GameManager              -> timer ends or boss dies -> Victory / hero dies -> Defeat
```
