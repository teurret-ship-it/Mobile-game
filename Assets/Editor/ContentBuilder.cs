#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SurvivalMoba.Abilities;
using SurvivalMoba.Combat;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Enemies;
using SurvivalMoba.Systems;
using SurvivalMoba.UI;

namespace SurvivalMoba.EditorTools
{
    /// <summary>
    /// One-click generator for the MVP's ScriptableObject data and placeholder
    /// prefabs (primitive art). Run from the menu: SurvivalMoba > 1. Build Content.
    /// This lets the game run end-to-end without hand-authoring assets.
    /// </summary>
    public static class ContentBuilder
    {
        private const string Root = "Assets/Generated";
        private const string PrefabDir = Root + "/Prefabs";
        private const string DataDir = Root + "/Data";
        private const string MatDir = Root + "/Materials";

        [MenuItem("SurvivalMoba/1. Build Content (Data + Prefabs)")]
        public static void BuildContent()
        {
            EnsureFolders();

            // --- Shared feedback / pickup prefabs ---
            GameObject hitVfx = MakeVfx("VFX_Hit", new Color(1f, 0.9f, 0.4f), 0.4f);
            GameObject deathVfx = MakeVfx("VFX_Death", new Color(1f, 0.3f, 0.3f), 0.7f);
            GameObject meteorVfx = MakeVfx("VFX_Meteor", new Color(1f, 0.5f, 0.1f), 2.5f);
            GameObject dashVfx = MakeVfx("VFX_Dash", new Color(0.5f, 0.9f, 1f), 0.8f);

            GameObject xpOrb = MakeXpOrb();
            GameObject damageNumber = MakeDamageNumber();

            // --- Projectiles ---
            GameObject basicProj = MakeProjectile("Projectile_MagicBolt", new Color(0.4f, 0.7f, 1f), 0.3f, hitVfx);
            GameObject beamProj = MakeProjectile("Projectile_PiercingBeam", new Color(0.9f, 0.4f, 1f), 0.5f, hitVfx, elongated: true);

            // --- Frost zone ---
            GameObject frostZone = MakeFrostZone();

            // --- Abilities ---
            AbilityData magicBolt = MakeAbility("MagicBolt", "Magic Bolt", "Auto-target magic projectile.",
                TargetingType.TargetedAutoAim, AbilityEffectType.Projectile,
                cooldown: 0.8f, damage: 10f, range: 6f, projectileSpeed: 12f, vfx: basicProj);

            AbilityData piercingBeam = MakeAbility("PiercingBeam", "Piercing Beam", "Pierces all enemies in a line.",
                TargetingType.LineSkillshot, AbilityEffectType.PiercingLine,
                cooldown: 5f, damage: 30f, range: 8f, projectileSpeed: 18f, vfx: beamProj);

            AbilityData frost = MakeAbility("FrostZone", "Frost Zone", "Slows and damages enemies in an area.",
                TargetingType.AreaPlacement, AbilityEffectType.AreaOverTime,
                cooldown: 8f, damage: 10f, range: 5f, projectileSpeed: 0f, vfx: frostZone);
            frost.duration = 4f; frost.radius = 3f; frost.ticksPerSecond = 2f;
            frost.slowFraction = 0.4f; frost.slowDuration = 1f;
            EditorUtility.SetDirty(frost);

            AbilityData dash = MakeAbility("DashStrike", "Dash Strike", "Dash forward, damaging enemies in the path.",
                TargetingType.Dash, AbilityEffectType.Dash,
                cooldown: 10f, damage: 20f, range: 4f, projectileSpeed: 0f, vfx: dashVfx);
            dash.radius = 1f;
            EditorUtility.SetDirty(dash);

            AbilityData meteor = MakeAbility("MeteorStorm", "Meteor Storm", "Calls meteors around the hero.",
                TargetingType.InstantCast, AbilityEffectType.MeteorStorm,
                cooldown: 45f, damage: 40f, range: 6f, projectileSpeed: 0f, vfx: meteorVfx);
            meteor.duration = 5f; meteor.radius = 2f; meteor.hitCount = 12;
            EditorUtility.SetDirty(meteor);

            // --- Hero ---
            HeroData hero = CreateOrLoad<HeroData>(DataDir + "/Hero_ArcaneHunter.asset");
            hero.heroName = "Arcane Hunter";
            hero.role = "Ranged survival mage";
            hero.maxHealth = 100f;
            hero.moveSpeed = 5f;
            hero.attackSpeed = 1f;
            hero.baseDamage = 10f;
            hero.basicAttackRange = 6f;
            hero.basicAttack = magicBolt;
            hero.ability1 = piercingBeam;
            hero.ability2 = frost;
            hero.ability3 = dash;
            hero.ultimate = meteor;
            EditorUtility.SetDirty(hero);

            // --- Enemies (data + prefabs) ---
            EnemyData grunt = MakeEnemy("Grunt", EnemyType.Grunt, 20f, 5f, 2f, 1, new Color(0.8f, 0.3f, 0.3f),
                PrimitiveType.Capsule, Vector3.one, xpOrb, deathVfx);
            EnemyData runner = MakeEnemy("Runner", EnemyType.Runner, 12f, 4f, 3.5f, 1, new Color(0.9f, 0.7f, 0.2f),
                PrimitiveType.Capsule, new Vector3(0.7f, 1.1f, 0.7f), xpOrb, deathVfx);
            EnemyData brute = MakeEnemy("Brute", EnemyType.Brute, 80f, 12f, 1.4f, 3, new Color(0.4f, 0.3f, 0.6f),
                PrimitiveType.Cube, new Vector3(1.6f, 1.6f, 1.6f), xpOrb, deathVfx);
            EnemyData boss = MakeEnemy("Boss", EnemyType.Boss, 1000f, 20f, 1.5f, 50, new Color(0.6f, 0.1f, 0.1f),
                PrimitiveType.Cube, new Vector3(3f, 3f, 3f), xpOrb, deathVfx);

            // --- Waves ---
            WaveData waves = CreateOrLoad<WaveData>(DataDir + "/Waves_FiveMinute.asset");
            waves.matchDuration = 300f;
            waves.entries = new List<WaveEntry>
            {
                new WaveEntry { time = 0f,   enemy = grunt,  spawnRate = 1.0f, maxAlive = 20 },
                new WaveEntry { time = 45f,  enemy = grunt,  spawnRate = 2.0f, maxAlive = 30 },
                new WaveEntry { time = 90f,  enemy = runner, spawnRate = 2.5f, maxAlive = 40 },
                new WaveEntry { time = 135f, enemy = brute,  spawnRate = 1.0f, maxAlive = 45 },
                new WaveEntry { time = 180f, enemy = runner, spawnRate = 3.0f, maxAlive = 60 },
                new WaveEntry { time = 255f, enemy = boss,   spawnRate = 0f,   maxAlive = 1, isBossWave = true },
            };
            EditorUtility.SetDirty(waves);

            // --- Upgrades ---
            BuildUpgrades();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SurvivalMoba] Content built under Assets/Generated. Next: run 'SurvivalMoba > 2. Build Game Scene'.");
        }

        // ---------- Upgrades ----------

        private static void BuildUpgrades()
        {
            MakeUpgrade("power_surge", "Power Surge", "+10% ability damage", Rarity.Common, UpgradeEffectType.AbilityDamage, 0.10f, 5);
            MakeUpgrade("quick_hands", "Quick Hands", "+10% attack speed", Rarity.Common, UpgradeEffectType.AttackSpeed, 0.10f, 5);
            MakeUpgrade("swift_boots", "Swift Boots", "+10% move speed", Rarity.Common, UpgradeEffectType.MoveSpeed, 0.10f, 5);
            MakeUpgrade("vitality", "Vitality", "+25 max health", Rarity.Common, UpgradeEffectType.MaxHealth, 25f, 5);
            MakeUpgrade("arcane_focus", "Arcane Focus", "-8% ability cooldowns", Rarity.Rare, UpgradeEffectType.CooldownReduction, 0.08f, 5);
            MakeUpgrade("split_shot", "Split Shot", "Basic attack fires one extra projectile", Rarity.Epic, UpgradeEffectType.ExtraProjectile, 1f, 3);
            MakeUpgrade("piercing_magic", "Piercing Magic", "Basic attacks pierce one extra enemy", Rarity.Rare, UpgradeEffectType.ProjectilePierce, 1f, 3);
            MakeUpgrade("frost_mastery", "Frost Mastery", "Frost Zone radius +20%", Rarity.Rare, UpgradeEffectType.FrostRadius, 0.20f, 3);
            MakeUpgrade("dash_recovery", "Dash Recovery", "Dash cooldown -20%", Rarity.Epic, UpgradeEffectType.DashCooldown, 0.20f, 3);
            MakeUpgrade("ultimate_charge", "Ultimate Charge", "Ultimate cooldown -15%", Rarity.Legendary, UpgradeEffectType.UltimateCooldown, 0.15f, 3);
        }

        private static void MakeUpgrade(string id, string name, string desc, Rarity rarity,
            UpgradeEffectType effect, float value, int maxStacks)
        {
            UpgradeData u = CreateOrLoad<UpgradeData>($"{DataDir}/Upgrade_{id}.asset");
            u.upgradeId = id; u.upgradeName = name; u.description = desc;
            u.rarity = rarity; u.effectType = effect; u.value = value; u.maxStacks = maxStacks;
            EditorUtility.SetDirty(u);
        }

        // ---------- Abilities / enemies ----------

        private static AbilityData MakeAbility(string file, string name, string desc,
            TargetingType targeting, AbilityEffectType effect,
            float cooldown, float damage, float range, float projectileSpeed, GameObject vfx)
        {
            AbilityData a = CreateOrLoad<AbilityData>($"{DataDir}/Ability_{file}.asset");
            a.abilityId = file.ToLower();
            a.abilityName = name;
            a.description = desc;
            a.targeting = targeting;
            a.effect = effect;
            a.cooldown = cooldown;
            a.damage = damage;
            a.range = range;
            a.projectileSpeed = projectileSpeed;
            a.vfxPrefab = vfx;
            EditorUtility.SetDirty(a);
            return a;
        }

        private static EnemyData MakeEnemy(string name, EnemyType type, float hp, float dmg, float speed,
            int xp, Color color, PrimitiveType shape, Vector3 scale, GameObject xpOrb, GameObject deathVfx)
        {
            EnemyData data = CreateOrLoad<EnemyData>($"{DataDir}/Enemy_{name}.asset");
            data.enemyName = name;
            data.enemyType = type;
            data.maxHealth = hp;
            data.contactDamage = dmg;
            data.moveSpeed = speed;
            data.attackRange = 1.2f + scale.x * 0.4f;
            data.attackCooldown = 1f;
            data.xpReward = xp;

            // Build the enemy prefab.
            GameObject go = GameObject.CreatePrimitive(shape);
            go.name = $"Enemy_{name}";
            go.transform.localScale = scale;
            Paint(go, color);

            // Keep the collider (non-trigger) so projectiles can hit it; no rigidbody needed.
            var health = go.AddComponent<HealthSystem>();
            health.SetFaction(Faction.Enemy);
            health.SetMaxHealth(hp, true);

            var ctrl = go.AddComponent<EnemyController>();
            SetPrivate(ctrl, "data", data);
            SetPrivate(ctrl, "xpOrbPrefab", xpOrb);
            SetPrivate(ctrl, "deathVfx", deathVfx);

            GameObject prefab = SavePrefab(go, $"{PrefabDir}/Enemy_{name}.prefab");
            data.prefab = prefab;
            EditorUtility.SetDirty(data);
            return data;
        }

        // ---------- Primitive prefab helpers ----------

        private static GameObject MakeProjectile(string name, Color color, float scale, GameObject hitVfx, bool elongated = false)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = elongated ? new Vector3(scale, scale, scale * 3f) : Vector3.one * scale;
            Paint(go, color);

            var col = go.GetComponent<Collider>();
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var proj = go.AddComponent<Projectile>();
            SetPrivate(proj, "hitVfx", hitVfx);

            return SavePrefab(go, $"{PrefabDir}/{name}.prefab");
        }

        private static GameObject MakeFrostZone()
        {
            GameObject root = new GameObject("FX_FrostZone");
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Visual";
            disc.transform.SetParent(root.transform, false);
            disc.transform.localScale = new Vector3(1f, 0.05f, 1f); // unit-diameter, flat
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            Paint(disc, new Color(0.4f, 0.8f, 1f, 0.6f));

            var zone = root.AddComponent<AreaEffectZone>();
            SetPrivate(zone, "visual", disc.transform);

            return SavePrefab(root, $"{PrefabDir}/FX_FrostZone.prefab");
        }

        private static GameObject MakeVfx(string name, Color color, float scale)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Paint(go, color);
            return SavePrefab(go, $"{PrefabDir}/{name}.prefab");
        }

        private static GameObject MakeXpOrb()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "XPOrb";
            go.transform.localScale = Vector3.one * 0.35f;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Paint(go, new Color(0.3f, 1f, 0.5f));
            go.AddComponent<SurvivalMoba.XP.XPOrb>();
            return SavePrefab(go, $"{PrefabDir}/XPOrb.prefab");
        }

        private static GameObject MakeDamageNumber()
        {
            GameObject go = new GameObject("DamageNumber");
            var tm = go.AddComponent<TextMesh>();
            tm.text = "0";
            tm.characterSize = 0.15f;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;
            go.AddComponent<FloatingDamageNumber>();
            return SavePrefab(go, $"{PrefabDir}/DamageNumber.prefab");
        }

        // ---------- Low-level utilities ----------

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "Generated");
            CreateFolder(Root, "Prefabs");
            CreateFolder(Root, "Data");
            CreateFolder(Root, "Materials");
        }

        private static void CreateFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static Shader LitShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Sprites/Default");
        }

        private static void Paint(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;

            string matPath = $"{MatDir}/Mat_{ColorKey(color)}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(LitShader());
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            renderer.sharedMaterial = mat;
        }

        private static string ColorKey(Color c) =>
            $"{Mathf.RoundToInt(c.r * 255)}_{Mathf.RoundToInt(c.g * 255)}_{Mathf.RoundToInt(c.b * 255)}";

        private static GameObject SavePrefab(GameObject instance, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        /// <summary>Set a private serialized field via SerializedObject so prefab links persist.</summary>
        private static void SetPrivate(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
