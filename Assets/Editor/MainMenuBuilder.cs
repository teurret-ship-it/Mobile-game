#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SurvivalMoba.UI;

namespace SurvivalMoba.EditorTools
{
    /// <summary>
    /// Builds a minimal main menu scene (Play / Settings / Quit) and a one-click
    /// "Build Everything" entry. Adds both scenes to Build Settings.
    /// </summary>
    public static class MainMenuBuilder
    {
        private const string SceneDir = "Assets/Scenes";

        [MenuItem("SurvivalMoba/3. Build Main Menu")]
        public static void BuildMainMenu()
        {
            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);

            var canvasGo = new GameObject("Menu Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();
            var loader = canvasGo.AddComponent<SceneLoader>();

            // Title
            MakeText(canvas.transform, font, "Title", "ARCANE SURVIVORS", 72, new Vector2(0f, 250f), new Vector2(1200f, 120f));

            // Buttons
            MakeButton(canvas.transform, uiSprite, font, "PlayButton", "Play", new Vector2(0f, 40f),
                new Color(0.2f, 0.5f, 0.7f), loader.PlayGame);
            MakeButton(canvas.transform, uiSprite, font, "QuitButton", "Quit", new Vector2(0f, -80f),
                new Color(0.4f, 0.3f, 0.3f), loader.QuitGame);

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            if (!AssetDatabase.IsValidFolder(SceneDir))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            string path = $"{SceneDir}/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);

            Debug.Log($"[SurvivalMoba] Main menu built at {path}.");
        }

        [MenuItem("SurvivalMoba/Build Everything")]
        public static void BuildEverything()
        {
            ContentBuilder.BuildContent();
            GameSceneBuilder.BuildScene();
            BuildMainMenu();
            RegisterScenes();
            EditorUtility.DisplayDialog("SurvivalMoba",
                "Built content, GameScene and MainMenu, and added them to Build Settings.\n" +
                "Open Assets/Scenes/MainMenu.unity and press Play.", "OK");
        }

        private static void RegisterScenes()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{SceneDir}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SceneDir}/GameScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void MakeButton(Transform parent, Sprite sprite, Font font, string name, string label,
            Vector2 pos, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite; img.type = Image.Type.Sliced; img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(380f, 90f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            MakeText(go.transform, font, "Text", label, 36, Vector2.zero, new Vector2(380f, 90f));
        }

        private static void MakeText(Transform parent, Font font, string name, string text, int size,
            Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text; t.font = font; t.fontSize = size; t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;
        }
    }
}
#endif
