#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivalMoba.Abilities;
using SurvivalMoba.Combat;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Enemies;
using SurvivalMoba.InputControl;
using SurvivalMoba.Player;
using SurvivalMoba.Systems;
using SurvivalMoba.UI;
using SurvivalMoba.XP;

namespace SurvivalMoba.EditorTools
{
    /// <summary>
    /// Builds a fully-wired, playable GameScene from the generated content
    /// (run ContentBuilder first). Creates the world, player, managers and a
    /// complete mobile HUD, then saves the scene to Assets/Scenes/GameScene.unity.
    /// Menu: SurvivalMoba > 2. Build Game Scene.
    /// </summary>
    public static class GameSceneBuilder
    {
        private const string DataDir = "Assets/Generated/Data";
        private const string PrefabDir = "Assets/Generated/Prefabs";
        private const string SceneDir = "Assets/Scenes";

        private static Sprite _uiSprite;

        [MenuItem("SurvivalMoba/2. Build Game Scene")]
        public static void BuildScene()
        {
            HeroData hero = Load<HeroData>($"{DataDir}/Hero_ArcaneHunter.asset");
            if (hero == null)
            {
                EditorUtility.DisplayDialog("SurvivalMoba",
                    "Run 'SurvivalMoba > 1. Build Content' first.", "OK");
                return;
            }

            _uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- World ---
            BuildLighting();
            BuildGround();
            Camera cam = BuildCamera();

            // --- Player ---
            GameObject player = BuildPlayer(hero, cam.transform);

            // --- Managers ---
            GameObject managers = new GameObject("Managers");
            var gameManager = managers.AddComponent<GameManager>();
            managers.AddComponent<PoolManager>();
            var xpSystem = managers.AddComponent<XPSystem>();
            var upgradeManager = managers.AddComponent<UpgradeManager>();
            var levelUpManager = managers.AddComponent<LevelUpManager>();
            var spawner = managers.AddComponent<EnemySpawner>();
            var waveManager = managers.AddComponent<WaveManager>();
            var damageService = managers.AddComponent<DamageNumberService>();

            SetRef(gameManager, "player", player.transform);
            SetRef(damageService, "numberPrefab", Load<GameObject>($"{PrefabDir}/DamageNumber.prefab"));

            // Spawner / waves
            SetRef(spawner, "xpOrbPrefab", Load<GameObject>($"{PrefabDir}/XPOrb.prefab"));
            SetRef(waveManager, "waveData", Load<WaveData>($"{DataDir}/Waves_FiveMinute.asset"));
            SetRef(waveManager, "spawner", spawner);

            // Level-up
            SetRef(levelUpManager, "xpSystem", xpSystem);
            SetRef(levelUpManager, "upgradeManager", upgradeManager);
            SetList(upgradeManager, "upgradePool", LoadAllUpgrades());

            // Cam follow target
            SetRef(cam.GetComponent<CameraFollow>(), "target", player.transform);

            // --- UI ---
            BuildUI(player, levelUpManager, waveManager);

            // --- EventSystem ---
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // --- Save ---
            if (!AssetDatabase.IsValidFolder(SceneDir))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            string scenePath = $"{SceneDir}/GameScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SurvivalMoba] Built playable scene at {scenePath}. Open it and press Play.");
            EditorUtility.DisplayDialog("SurvivalMoba",
                "GameScene built at Assets/Scenes/GameScene.unity.\nOpen it and press Play.", "OK");
        }

        // ---------- World ----------

        private static void BuildLighting()
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void BuildGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena";
            ground.transform.localScale = new Vector3(8f, 1f, 8f); // 80x80 units
            var mat = ground.GetComponent<Renderer>();
            if (mat != null)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                m.color = new Color(0.18f, 0.20f, 0.24f);
                mat.sharedMaterial = m;
            }
        }

        private static Camera BuildCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);

            Quaternion rot = Quaternion.Euler(45f, 45f, 0f);
            camGo.transform.rotation = rot;
            camGo.transform.position = rot * new Vector3(0f, 0f, -25f); // looks at origin
            camGo.AddComponent<CameraFollow>();
            return cam;
        }

        // ---------- Player ----------

        private static GameObject BuildPlayer(HeroData hero, Transform camTransform)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<Collider>()); // movement is transform-based
            var rend = player.GetComponent<Renderer>();
            if (rend != null)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                m.color = new Color(0.3f, 0.8f, 1f);
                rend.sharedMaterial = m;
            }

            var health = player.AddComponent<HealthSystem>();
            health.SetFaction(Faction.Player);
            health.SetMaxHealth(hero.maxHealth, true);

            var character = player.AddComponent<PlayerCharacter>();
            SetRef(character, "heroData", hero);

            var controller = player.AddComponent<PlayerController>();
            SetRef(controller, "cameraTransform", camTransform);

            // Muzzle at the front of the capsule.
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(player.transform, false);
            muzzle.localPosition = new Vector3(0f, 0.5f, 0.6f);

            var basic = player.AddComponent<BasicAttack>();
            SetRef(basic, "character", character);
            SetRef(basic, "muzzle", muzzle);
            SetRef(basic, "projectilePrefab", Load<GameObject>($"{PrefabDir}/Projectile_MagicBolt.prefab"));

            var abilitySystem = player.AddComponent<AbilitySystem>();
            SetRef(abilitySystem, "character", character);
            SetRef(abilitySystem, "cameraTransform", camTransform);
            SetRef(abilitySystem, "castOrigin", muzzle);

            // Aim indicator (line + circle children).
            BuildAimIndicator(player, abilitySystem);

            return player;
        }

        private static void BuildAimIndicator(GameObject player, AbilitySystem abilitySystem)
        {
            var root = new GameObject("AimIndicator");
            root.transform.SetParent(player.transform, false);
            var indicator = root.AddComponent<AimIndicator>();

            // Line: a thin elongated quad-like cube anchored at origin, pointing +Z.
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "Line";
            Object.DestroyImmediate(line.GetComponent<Collider>());
            line.transform.SetParent(root.transform, false);
            line.transform.localScale = new Vector3(0.3f, 0.02f, 1f);
            TintUnlit(line, new Color(1f, 1f, 0.3f, 0.6f));

            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "Circle";
            Object.DestroyImmediate(circle.GetComponent<Collider>());
            circle.transform.SetParent(root.transform, false);
            circle.transform.localScale = new Vector3(1f, 0.02f, 1f);
            TintUnlit(circle, new Color(0.4f, 0.8f, 1f, 0.5f));

            SetRef(indicator, "lineVisual", line.transform);
            SetRef(indicator, "circleVisual", circle.transform);
            SetRef(abilitySystem, "aimIndicator", indicator);
        }

        private static void TintUnlit(GameObject go, Color color)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            m.color = color;
            rend.sharedMaterial = m;
        }

        // ---------- UI ----------

        private static void BuildUI(GameObject player, LevelUpManager levelUpManager, WaveManager waveManager)
        {
            // Canvas
            var canvasGo = new GameObject("HUD Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var uiManager = canvasGo.AddComponent<UIManager>();

            // ----- Top HUD -----
            Image healthBg = Panel(canvas.transform, "HealthBar", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(400f, 40f), new Color(0f, 0f, 0f, 0.5f));
            Image healthFill = Fill(healthBg.transform, "Fill", new Color(0.9f, 0.2f, 0.2f));
            Text healthText = Label(healthBg.transform, "Text", "100/100", 22, TextAnchor.MiddleCenter);

            Image xpBg = Panel(canvas.transform, "XPBar", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(-60f, 18f), new Color(0f, 0f, 0f, 0.5f));
            Image xpFill = Fill(xpBg.transform, "Fill", new Color(0.3f, 0.7f, 1f));

            Text levelText = Label(canvas.transform, "LevelText", "Lv 1", 28, TextAnchor.MiddleLeft);
            Anchor(levelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30f, -95f), new Vector2(200f, 40f));

            Text timerText = Label(canvas.transform, "TimerText", "05:00", 34, TextAnchor.MiddleCenter);
            Anchor(timerText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(220f, 50f));

            // Boss bar (hidden by default)
            Image bossBg = Panel(canvas.transform, "BossBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(700f, 26f), new Color(0f, 0f, 0f, 0.6f));
            Image bossFill = Fill(bossBg.transform, "Fill", new Color(0.8f, 0.1f, 0.1f));
            bossBg.gameObject.SetActive(false);

            // Pause button
            Button pauseBtn = TextButton(canvas.transform, "PauseButton", "II", 28, new Color(0f, 0f, 0f, 0.5f));
            Anchor(pauseBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-50f, -40f), new Vector2(70f, 70f));
            pauseBtn.onClick.AddListener(uiManager.OnPausePressed);

            // ----- Joystick (bottom-left) -----
            MobileJoystick joystick = BuildJoystick(canvas.transform);
            SetRef(player.GetComponent<PlayerController>(), "joystick", joystick);

            // ----- Ability buttons (bottom-right) -----
            AbilitySystem abilitySystem = player.GetComponent<AbilitySystem>();
            BasicAttack basicAttack = player.GetComponent<BasicAttack>();

            BuildAbilityButton(canvas.transform, "BtnBasic", "A", -1, abilitySystem, basicAttack,
                new Vector2(-120f, 120f), 110f, new Color(0.8f, 0.8f, 0.8f));
            BuildAbilityButton(canvas.transform, "Btn1", "1", 0, abilitySystem, null,
                new Vector2(-300f, 150f), 100f, new Color(0.5f, 0.6f, 1f));
            BuildAbilityButton(canvas.transform, "Btn2", "2", 1, abilitySystem, null,
                new Vector2(-260f, 290f), 100f, new Color(0.5f, 0.9f, 1f));
            BuildAbilityButton(canvas.transform, "Btn3", "3", 2, abilitySystem, null,
                new Vector2(-120f, 350f), 100f, new Color(1f, 0.7f, 0.4f));
            BuildAbilityButton(canvas.transform, "BtnUlt", "R", 3, abilitySystem, null,
                new Vector2(-150f, 510f), 120f, new Color(1f, 0.4f, 0.4f));

            // ----- Level-up panel -----
            GameObject levelPanel;
            List<UpgradeCard> cards = BuildLevelUpPanel(canvas.transform, out levelPanel);
            levelPanel.SetActive(false);

            // ----- End screens -----
            GameObject victory = BuildResultPanel(canvas.transform, "VictoryPanel", "VICTORY", new Color(0.1f, 0.3f, 0.1f, 0.9f), uiManager);
            GameObject defeat = BuildResultPanel(canvas.transform, "DefeatPanel", "DEFEAT", new Color(0.3f, 0.1f, 0.1f, 0.9f), uiManager);
            GameObject pause = BuildResultPanel(canvas.transform, "PausePanel", "PAUSED", new Color(0.1f, 0.1f, 0.15f, 0.9f), uiManager, isPause: true);
            victory.SetActive(false); defeat.SetActive(false); pause.SetActive(false);

            // ----- Wire UIManager -----
            SetRef(uiManager, "healthFill", healthFill);
            SetRef(uiManager, "healthText", healthText);
            SetRef(uiManager, "xpFill", xpFill);
            SetRef(uiManager, "levelText", levelText);
            SetRef(uiManager, "timerText", timerText);
            SetRef(uiManager, "bossBarRoot", bossBg.gameObject);
            SetRef(uiManager, "bossFill", bossFill);
            SetRef(uiManager, "levelUpPanel", levelPanel);
            SetList(uiManager, "upgradeCards", cards.ConvertAll(c => (Object)c));
            SetRef(uiManager, "victoryPanel", victory);
            SetRef(uiManager, "defeatPanel", defeat);
            SetRef(uiManager, "pausePanel", pause);
            SetRef(uiManager, "levelUpManager", levelUpManager);
            SetRef(uiManager, "waveManager", waveManager);
        }

        private static MobileJoystick BuildJoystick(Transform parent)
        {
            Image bg = Panel(parent, "Joystick", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0.5f, 0.5f), new Vector2(220f, 220f), new Vector2(260f, 260f), new Color(1f, 1f, 1f, 0.18f));
            Image handle = Panel(bg.transform, "Handle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f), new Color(1f, 1f, 1f, 0.5f));

            var joystick = bg.gameObject.AddComponent<MobileJoystick>();
            SetRef(joystick, "background", bg.rectTransform);
            SetRef(joystick, "handle", handle.rectTransform);
            return joystick;
        }

        private static void BuildAbilityButton(Transform parent, string name, string label, int slot,
            AbilitySystem abilitySystem, BasicAttack basicAttack, Vector2 anchoredPos, float size, Color color)
        {
            Image bg = Panel(parent, name, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), anchoredPos, new Vector2(size, size), color);
            // Circular look isn't required; the UISprite rounded square reads fine.

            // Cooldown overlay (radial fill, dark).
            Image overlay = Fill(bg.transform, "Cooldown", new Color(0f, 0f, 0f, 0.6f));
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillOrigin = (int)Image.Origin360.Top;
            overlay.fillClockwise = false;
            overlay.fillAmount = 0f;

            Label(bg.transform, "Label", label, 30, TextAnchor.MiddleCenter);
            Text cdText = Label(bg.transform, "CDText", "", 26, TextAnchor.MiddleCenter);
            cdText.enabled = false;

            if (slot >= 0)
            {
                var btn = bg.gameObject.AddComponent<AbilityButton>();
                SetInt(btn, "slot", slot);
                SetRef(btn, "abilitySystem", abilitySystem);
                SetRef(btn, "cooldownOverlay", overlay);
                SetRef(btn, "cooldownText", cdText);
            }
            else
            {
                var btn = bg.gameObject.AddComponent<BasicAttackButton>();
                SetRef(btn, "basicAttack", basicAttack);
            }
        }

        private static List<UpgradeCard> BuildLevelUpPanel(Transform parent, out GameObject panelGo)
        {
            Image panel = Panel(parent, "LevelUpPanel", new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.75f));
            panelGo = panel.gameObject;

            Text title = Label(panel.transform, "Title", "LEVEL UP — Choose an Upgrade", 40, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -160f), new Vector2(1200f, 60f));

            var cards = new List<UpgradeCard>();
            float spacing = 480f;
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * spacing;
                Image card = Panel(panel.transform, $"Card{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(420f, 520f), new Color(0.15f, 0.15f, 0.2f, 0.95f));

                Text nameT = Label(card.transform, "Name", "Upgrade", 32, TextAnchor.UpperCenter);
                Anchor(nameT.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -40f), new Vector2(380f, 60f));

                Text descT = Label(card.transform, "Desc", "Description", 24, TextAnchor.MiddleCenter);
                Anchor(descT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 0f), new Vector2(380f, 200f));

                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;

                var uc = card.gameObject.AddComponent<UpgradeCard>();
                SetRef(uc, "background", card);
                SetRef(uc, "nameText", nameT);
                SetRef(uc, "descriptionText", descT);
                SetRef(uc, "button", button);
                cards.Add(uc);
            }
            return cards;
        }

        private static GameObject BuildResultPanel(Transform parent, string name, string title, Color bg,
            UIManager uiManager, bool isPause = false)
        {
            Image panel = Panel(parent, name, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, bg);

            Text titleT = Label(panel.transform, "Title", title, 64, TextAnchor.MiddleCenter);
            Anchor(titleT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 120f), new Vector2(800f, 120f));

            var loader = panel.gameObject.AddComponent<SceneLoader>();

            Button retry = TextButton(panel.transform, "RetryButton", isPause ? "Resume" : "Retry", 30, new Color(0.2f, 0.4f, 0.6f));
            Anchor(retry.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f), new Vector2(320f, 80f));
            if (isPause) retry.onClick.AddListener(uiManager.OnResumePressed);
            else retry.onClick.AddListener(loader.RestartMatch);

            Button menu = TextButton(panel.transform, "MenuButton", "Main Menu", 30, new Color(0.4f, 0.3f, 0.3f));
            Anchor(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -150f), new Vector2(320f, 80f));
            menu.onClick.AddListener(loader.GoToMenu);

            return panel.gameObject;
        }

        // ---------- UI primitives ----------

        private static Image Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = _uiSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            Anchor(img.rectTransform, aMin, aMax, pivot, anchoredPos, size);
            return img;
        }

        private static Image Fill(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = _uiSprite;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
            img.color = color;
            // Stretch to parent.
            Anchor(img.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var rt = img.rectTransform;
            rt.offsetMin = new Vector2(4f, 4f);
            rt.offsetMax = new Vector2(-4f, -4f);
            return img;
        }

        private static Text Label(Transform parent, string name, string text, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            Anchor(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            t.rectTransform.offsetMin = Vector2.zero;
            t.rectTransform.offsetMax = Vector2.zero;
            return t;
        }

        private static Button TextButton(Transform parent, string name, string label, int size, Color color)
        {
            Image bg = Panel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 70f), color);
            var btn = bg.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            Label(bg.transform, "Text", label, size, TextAnchor.MiddleCenter);
            return btn;
        }

        private static void Anchor(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        // ---------- Asset / reflection helpers ----------

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        private static List<Object> LoadAllUpgrades()
        {
            var list = new List<Object>();
            string[] guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { DataDir });
            foreach (string g in guids)
            {
                var u = AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(g));
                if (u != null) list.Add(u);
            }
            return list;
        }

        private static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
            else Debug.LogWarning($"[SurvivalMoba] Field '{field}' not found on {target.GetType().Name}");
        }

        private static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null) { prop.intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static void SetList(Object target, string field, List<Object> values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"[SurvivalMoba] List field '{field}' not found"); return; }
            prop.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
