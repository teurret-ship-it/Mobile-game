# Complete One-File Plan: 2.5D Isometric Android Survival MOBA Game

## Purpose

Build an original Android game inspired by:

- Vampire Survivors-style survival waves
- Mobile MOBA controls similar to Wild Rift
- 2.5D isometric camera
- Manual basic attack, skillshots, 3 abilities, and ultimate

Do not copy copyrighted characters, names, UI, icons, maps, assets, music, or exact mechanics from any existing game. Use original art, original heroes, original enemies, and original ability names.

---

# 1. Game Summary

Create a mobile Android action-survival game where the player controls one hero in an isometric arena. Enemies spawn continuously and chase the player. The player uses MOBA-style controls to move, attack, cast abilities, level up, choose upgrades, survive waves, and defeat bosses.

Core idea:

```text
Move with left joystick.
Attack and cast abilities with right-side buttons.
Survive enemy waves.
Gain XP.
Choose upgrades.
Defeat boss.
Win the match.
```

MVP match length:

```text
5 minutes
```

Full game match length later:

```text
10 to 15 minutes
```

---

# 2. Recommended Engine

Use Unity.

Recommended setup:

```text
Engine: Unity 2022 LTS or Unity 6 LTS
Language: C#
Render Pipeline: URP
Platform: Android
Camera: Orthographic isometric
World Type: 3D world with 2.5D/isometric camera
Movement Plane: X/Z plane
Height Axis: Y axis
```

Why Unity:

```text
Good Android support
Good mobile UI tools
Easy joystick controls
Good 3D and 2.5D workflow
Good asset pipeline
Large tutorial ecosystem
```

---

# 3. Camera and Visual Style

Use a 3D world with an orthographic isometric camera.

Camera settings:

```text
Projection: Orthographic
Rotation X: 45
Rotation Y: 45
Rotation Z: 0
Orthographic Size: 7 to 10
```

Player movement:

```text
Player moves on X/Z plane.
Y is vertical height.
```

Recommended art style for MVP:

```text
Low-poly stylized fantasy
Simple characters
Readable enemies
Bright ability effects
Simple arena environment
```

---

# 4. Core Gameplay Loop

```text
Start match
Player spawns in arena
Enemies spawn outside camera view
Enemies chase player
Player attacks and casts abilities
Enemies die and drop XP orbs
Player collects XP
Player levels up
Game pauses or slows
Player chooses 1 of 3 upgrades
Game resumes
Enemies get stronger over time
Mini-boss or boss appears
Player survives until timer ends or defeats final boss
Victory or defeat screen appears
Return to menu
```

---

# 5. Controls

Use mobile MOBA-style controls.

Left side:

```text
Virtual joystick for movement
```

Right side:

```text
Basic Attack button
Ability 1 button
Ability 2 button
Ability 3 button
Ultimate button
Optional dodge button later
```

Input behavior:

```text
Joystick drag = move player
Tap basic attack = attack nearest enemy in range
Hold ability button = show aiming indicator if needed
Drag ability button = aim skillshot
Release ability button = cast ability
Quick tap ability = auto-aim toward nearest enemy if possible
```

---

# 6. Player Stats

MVP stats:

```text
Max Health
Current Health
Damage
Move Speed
Attack Speed
Basic Attack Range
Ability Cooldowns
XP
Level
```

Full game stats later:

```text
Armor
Magic Resistance
Critical Chance
Cooldown Reduction
Life Steal
Ability Power
Energy or Mana
Projectile Speed
Area Size
Pickup Radius
```

---

# 7. Basic Attack System

Use auto-target basic attack for MVP.

Basic attack logic:

```text
When basic attack button is pressed:
    If attack is on cooldown:
        Do nothing
    Else:
        Find nearest enemy within attack range
        If enemy exists:
            Rotate player toward enemy
            Fire projectile at enemy
        Else:
            Fire projectile forward
        Start attack cooldown
```

Example basic attack:

```text
Name: Magic Bolt
Type: Auto-target projectile
Damage: 10
Cooldown: 0.8 seconds
Range: 6 units
Projectile Speed: 12
```

---

# 8. Ability System

The player has:

```text
Ability 1
Ability 2
Ability 3
Ultimate
```

Each ability should have:

```text
Ability ID
Name
Description
Icon
Cooldown
Damage
Range
Targeting Type
Effect Type
VFX Prefab
SFX Clip
Current Cooldown
```

Ability targeting types:

```text
Instant Cast
Line Skillshot
Cone Skillshot
Area Placement
Dash
Self Buff
Targeted Auto-Aim
```

Ability input logic:

```text
When ability button is pressed:
    If ability is on cooldown:
        Do nothing
    Else if ability needs aiming:
        Show aiming indicator
        Track drag direction
    Else:
        Cast ability immediately
        Start cooldown

When ability button is released:
    If ability needs aiming:
        Cast ability in aimed direction
        Hide aiming indicator
        Start cooldown
```

---

# 9. MVP Hero

Hero name:

```text
Arcane Hunter
```

Role:

```text
Ranged survival mage
```

Basic Attack:

```text
Name: Magic Bolt
Type: Auto-target projectile
Damage: 10
Cooldown: 0.8 seconds
Range: 6
```

Ability 1:

```text
Name: Piercing Beam
Type: Line skillshot
Cooldown: 5 seconds
Damage: 30
Range: 8
Effect: Pierces all enemies in a straight line
```

Ability 2:

```text
Name: Frost Zone
Type: Area placement
Cooldown: 8 seconds
Damage: 10 per second
Duration: 4 seconds
Radius: 3
Effect: Slows enemies by 40 percent
```

Ability 3:

```text
Name: Dash Strike
Type: Dash
Cooldown: 10 seconds
Dash Distance: 4
Damage: 20
Effect: Damages enemies passed through
```

Ultimate:

```text
Name: Meteor Storm
Type: Large area ultimate
Cooldown: 45 seconds
Duration: 5 seconds
Effect: Random meteors hit enemies near player
Damage: High
```

---

# 10. Enemy System

Enemies spawn continuously and move toward the player.

MVP enemy types:

## Enemy 1: Grunt

```text
Health: 20
Damage: 5
Speed: 2
Behavior: Chase player directly
```

## Enemy 2: Runner

```text
Health: 12
Damage: 4
Speed: 3.5
Behavior: Fast chase enemy
```

## Enemy 3: Brute

```text
Health: 80
Damage: 12
Speed: 1.4
Behavior: Slow tank enemy
```

## Boss: Corrupted Guardian

```text
Health: 1000
Damage: 20
Speed: 1.5
Abilities:
- Ground slam
- Summon enemies
- Charge attack
```

Basic enemy AI:

```text
Find player position
Move toward player
If close enough to player:
    Deal contact damage with cooldown
If health reaches zero:
    Die and drop XP
```

Ranged enemies can be added later.

---

# 11. Enemy Spawning

Enemies should spawn outside the visible camera area.

Spawn logic:

```text
Every spawn interval:
    If active enemy count is below max alive enemies:
        Choose random position around player
        Make sure position is outside camera view
        Spawn selected enemy type
```

Spawn radius:

```text
Minimum radius from player: 12 units
Maximum radius from player: 18 units
```

Do not spawn enemies directly on the player.

Use object pooling for enemies.

---

# 12. Wave Timeline for MVP

5-minute MVP wave plan:

```text
00:00 - Grunts spawn slowly
00:45 - Grunts spawn faster
01:30 - Runners appear
02:15 - Brutes appear
03:00 - Enemy count increases
04:00 - Mini-boss warning
04:15 - Boss appears
05:00 - Victory if player survives or boss is defeated
```

Example wave data:

```json
{
  "time": 90,
  "enemyType": "Runner",
  "spawnRate": 1.5,
  "maxAlive": 50
}
```

---

# 13. XP and Level Up

Enemies drop XP orbs when they die.

XP logic:

```text
When enemy dies:
    Spawn XP orb

When player touches XP orb:
    Add XP to player

If current XP >= required XP:
    Increase player level
    Pause or slow game
    Show 3 random upgrades

When player selects upgrade:
    Apply upgrade
    Resume game
```

XP requirement example:

```text
Level 1 to 2: 20 XP
Level 2 to 3: 35 XP
Level 3 to 4: 55 XP
Each level requires more XP
```

---

# 14. Upgrade System

Each upgrade has:

```text
Upgrade ID
Name
Description
Rarity
Effect Type
Value
Max Stacks
Current Stacks
```

MVP upgrades:

```text
1. Power Surge: +10% ability damage
2. Quick Hands: +10% attack speed
3. Swift Boots: +10% move speed
4. Vitality: +25 max health
5. Arcane Focus: -8% ability cooldowns
6. Split Shot: Basic attack fires one extra projectile
7. Piercing Magic: Basic attacks pierce one extra enemy
8. Frost Mastery: Frost Zone radius +20%
9. Dash Recovery: Dash cooldown -20%
10. Ultimate Charge: Ultimate cooldown -15%
```

Rarities:

```text
Common
Rare
Epic
Legendary
```

---

# 15. UI Requirements

In-game UI:

```text
Health bar
XP bar
Level number
Match timer
Basic attack button
Ability 1 button
Ability 2 button
Ability 3 button
Ultimate button
Cooldown overlays on buttons
Joystick
Pause button
Boss health bar
Optional damage numbers
```

Main menu UI:

```text
Play button
Hero selection button, later
Settings button
Quit button, optional
```

Settings:

```text
Music volume
SFX volume
Graphics quality
Joystick size
Button layout scale
Damage numbers on/off
Screen shake on/off
```

---

# 16. Technical Architecture

Recommended Unity scripts:

```text
PlayerController.cs
CameraFollow.cs
MobileJoystick.cs
BasicAttack.cs
HealthSystem.cs
DamageSystem.cs
AbilitySystem.cs
AbilityBase.cs
ProjectileAbility.cs
AreaAbility.cs
DashAbility.cs
AbilityButton.cs
EnemyController.cs
EnemySpawner.cs
WaveManager.cs
XPOrb.cs
XPSystem.cs
LevelUpManager.cs
UpgradeManager.cs
GameManager.cs
UIManager.cs
ObjectPool.cs
Projectile.cs
```

Recommended folder structure:

```text
Assets/
  Art/
    Characters/
    Enemies/
    Environment/
    UI/
    VFX/
  Audio/
    Music/
    SFX/
  Materials/
  Prefabs/
    Player/
    Enemies/
    Projectiles/
    Abilities/
    UI/
  Scenes/
    MainMenu.unity
    GameScene.unity
  Scripts/
    Core/
    Player/
    Enemies/
    Abilities/
    UI/
    Systems/
    Data/
  ScriptableObjects/
    Heroes/
    Abilities/
    Enemies/
    Waves/
    Upgrades/
  Animations/
```

---

# 17. Data-Driven Design

Use ScriptableObjects for:

```text
Hero data
Ability data
Enemy data
Wave data
Upgrade data
Boss data
```

Example ability data fields:

```text
Ability Name
Ability ID
Icon
Cooldown
Damage
Range
Projectile Speed
Area Radius
Duration
Targeting Type
VFX Prefab
SFX Clip
```

Example enemy data fields:

```text
Enemy Name
Health
Damage
Move Speed
Attack Range
Attack Cooldown
XP Reward
Prefab
```

This makes it easier to add new heroes, enemies, waves, and abilities later.

---

# 18. Object Pooling

Use object pooling for performance.

Pool these objects:

```text
Enemies
Projectiles
XP orbs
Hit effects
Floating damage numbers
Particles
```

Do not constantly instantiate and destroy objects during gameplay.

Object pooling is required because many enemies and projectiles will exist at the same time.

---

# 19. Android Performance Plan

Target performance:

```text
Low-end Android: 30 FPS
Mid/high Android: 60 FPS
```

Optimization rules:

```text
Use object pooling
Limit active enemies
Use simple enemy AI
Use low-poly models
Use simple shaders
Avoid expensive real-time shadows
Use baked lighting if possible
Limit particles
Avoid garbage collection spikes
Avoid too many physics checks
Use texture atlases
Use LOD or simple models
Profile on real Android devices
```

Enemy limits:

```text
Low-end devices: 60 active enemies
Mid devices: 120 active enemies
High devices: 200 active enemies
```

---

# 20. MVP Scope

Build only this first:

```text
One hero
One arena map
One basic attack
Three abilities
One ultimate
Three enemy types
One boss
XP and level-up system
Ten upgrades
Five-minute survival mode
Mobile joystick
Ability buttons with cooldown UI
Basic sound effects
Simple main menu
Android build
Object pooling
```

Do not build these in MVP:

```text
Multiplayer
Ranked mode
Online accounts
Guilds
Complex shop
Many heroes
Skins
Battle pass
Live events
Advanced monetization
```

---

# 21. Development Milestones

## Milestone 1: Movement and Camera

Goal:

```text
Player can move in an isometric arena using a mobile joystick.
Camera follows player.
```

Tasks:

```text
Create Unity project
Create GameScene
Add flat arena plane
Add player object
Add orthographic isometric camera
Add joystick UI
Write PlayerController.cs
Write CameraFollow.cs
Test movement in Unity editor
Test on Android
```

## Milestone 2: Basic Combat

Goal:

```text
Player can attack enemies.
Enemies can take damage and die.
```

Tasks:

```text
Create enemy prefab
Create HealthSystem.cs
Create BasicAttack.cs
Create Projectile.cs
Create basic attack button
Find nearest enemy in range
Fire projectile
Damage enemy
Kill enemy when health is zero
```

## Milestone 3: Enemy AI and Spawning

Goal:

```text
Enemies spawn and chase the player.
```

Tasks:

```text
Write EnemyController.cs
Write EnemySpawner.cs
Spawn enemies outside camera view
Enemies chase player
Enemies damage player on contact
Add player health bar
Add defeat condition
```

## Milestone 4: Ability System

Goal:

```text
Player can cast 3 abilities and ultimate.
```

Tasks:

```text
Write AbilitySystem.cs
Write AbilityBase.cs
Write ProjectileAbility.cs
Write AreaAbility.cs
Write DashAbility.cs
Create ability buttons
Create cooldown UI
Create aiming indicators
Implement Piercing Beam
Implement Frost Zone
Implement Dash Strike
Implement Meteor Storm
```

## Milestone 5: XP and Level Up

Goal:

```text
Player gains XP, levels up, and chooses upgrades.
```

Tasks:

```text
Create XP orb prefab
Write XPOrb.cs
Write XPSystem.cs
Write LevelUpManager.cs
Write UpgradeManager.cs
Create level-up UI
Show 3 upgrade choices
Apply selected upgrade
Resume game
```

## Milestone 6: Waves and Boss

Goal:

```text
Enemy waves change over time and boss appears.
```

Tasks:

```text
Write WaveManager.cs
Add match timer
Spawn Grunts first
Add Runners at 1:30
Add Brutes at 2:15
Increase spawn rate over time
Spawn boss at 4:15
Add boss health bar
Add victory condition
```

## Milestone 7: Polish

Goal:

```text
Game feels playable and understandable.
```

Tasks:

```text
Add main menu
Add pause menu
Add settings menu
Add ability icons
Add sound effects
Add hit VFX
Add enemy death VFX
Add damage numbers
Add boss warning
Add victory screen
Add defeat screen
```

## Milestone 8: Android Optimization

Goal:

```text
Game runs well on Android.
```

Tasks:

```text
Add object pooling
Profile performance
Reduce particle counts
Limit enemy count
Optimize physics checks
Optimize UI updates
Test on low-end Android device
Fix frame drops
Add graphics settings
```

---

# 22. Important Pseudocode

## Player Movement

```text
Read joystick input
Convert joystick direction to camera-relative world direction
Move player on X/Z plane
If moving, rotate player toward movement direction
```

## Basic Attack

```text
Function BasicAttack():
    If attack cooldown is not ready:
        return

    target = FindNearestEnemyInRange()

    If target exists:
        direction = target.position - player.position
        Rotate player toward target
        Fire projectile in direction
    Else:
        Fire projectile forward

    Start attack cooldown
```

## Ability Cast

```text
When ability button pressed:
    If ability cooldown is not ready:
        return

    If ability needs aiming:
        Show indicator
        Start aiming mode
    Else:
        Cast ability
        Start cooldown

When ability button released:
    If aiming mode is active:
        Cast ability using aim direction
        Hide indicator
        Start cooldown
```

## Enemy AI

```text
Every frame:
    direction = player.position - enemy.position
    Move toward player

    If distance to player <= attack range:
        If attack cooldown ready:
            Damage player
            Start attack cooldown
```

## Enemy Spawn

```text
Every spawn interval:
    If active enemies < max enemies:
        angle = random 0 to 360 degrees
        distance = random between 12 and 18
        spawnPosition = player.position + directionFromAngle * distance
        Spawn enemy at spawnPosition
```

## XP Level Up

```text
When enemy dies:
    Drop XP orb

When player collects XP orb:
    currentXP += orbXP

If currentXP >= requiredXP:
    currentXP -= requiredXP
    level += 1
    Pause game
    Show upgrade choices

When upgrade selected:
    Apply upgrade
    Resume game
```

---

# 23. First Sprint Prompt for Gemini 3.5 Flash

Copy this prompt into Gemini when starting implementation:

```text
You are helping me build a Unity Android game.

The game is an original 2.5D isometric survival action game inspired by Vampire Survivors-style waves and mobile MOBA controls similar to Wild Rift.

Do not copy copyrighted characters, assets, names, icons, maps, UI, music, or exact mechanics from any existing game.

Core features:
- Unity Android game
- 3D world with orthographic isometric camera
- Player moves on X/Z plane
- Camera follows player
- Player uses mobile virtual joystick
- Player has basic attack, 3 abilities, and 1 ultimate
- Some abilities are skillshots with aiming indicators
- Enemies spawn in waves and chase the player
- Player gains XP, levels up, and chooses upgrades
- MVP match lasts 5 minutes
- Use object pooling for enemies, projectiles, XP orbs, and VFX
- Optimize for Android performance

Start with Milestone 1 only.

Please provide:
1. Unity scene setup instructions.
2. Required GameObjects.
3. Required UI objects.
4. PlayerController.cs for joystick movement on the X/Z plane.
5. CameraFollow.cs for orthographic isometric camera follow.
6. Clear comments in the C# code.
7. Exact folder locations for scripts.
8. How to test the result in Unity editor and Android.

Keep the code simple, modular, and beginner-friendly.
```

---

# 24. Final MVP Checklist

The MVP is complete when it has:

```text
Playable Android build
Stable 30 FPS minimum
One hero
One map
Basic attack
Three abilities
Ultimate
Cooldown UI
Joystick movement
Enemy AI
Enemy spawning
Three enemy types
One boss
XP orbs
Level-up upgrade choices
Victory screen
Defeat screen
Simple main menu
Basic sound effects
Object pooling
Android performance settings
```

---

# 25. Build Order

Build in this exact order:

```text
1. Unity project setup
2. Isometric camera
3. Player movement
4. Mobile joystick
5. Basic attack
6. Enemy health
7. Enemy AI
8. Enemy spawning
9. Projectile system
10. Ability system
11. Skillshot aiming indicators
12. XP orbs
13. Level-up upgrades
14. Wave manager
15. Boss enemy
16. UI polish
17. Audio and VFX
18. Object pooling
19. Android optimization
20. Final MVP build
```

---

# 26. Main Rule

Start small.

First make this playable:

```text
Player moves.
Enemies chase.
Player shoots.
Enemies die.
Enemies keep spawning.
```

Only after that, add abilities, XP, upgrades, waves, bosses, polish, and optimization.
