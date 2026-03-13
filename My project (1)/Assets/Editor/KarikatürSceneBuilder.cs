// ============================================================
//  KarikatürSceneBuilder.cs
//  Konum: Assets/Editor/KarikatürSceneBuilder.cs
//
//  Kullanım:
//  Tools → GLITCHRUNNER → Build Karikatür Scene
// ============================================================
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class KarikatürSceneBuilder
{
    [MenuItem("Tools/GLITCHRUNNER/Build Karikatür Scene")]
    public static void BuildScene()
    {
        // ── 1. Yeni boş sahne ──────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── 2. Main Camera ─────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = Color.black;
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.depth            = -1;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ── 3. EventSystem ─────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ── 4. Canvas ──────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── 5. DisplayImage — tam ekranı kaplayan Image ─────────────────
        var imgGO = new GameObject("DisplayImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        var imgRT = imgGO.AddComponent<RectTransform>();
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = Vector2.zero;
        imgRT.offsetMax = Vector2.zero;
        var img = imgGO.AddComponent<Image>();
        img.color          = Color.white;
        img.preserveAspect = true;

        // ── 6. GlitchrunnerIntro script'ini yükle ve bağla ─────────────
        var introGO = new GameObject("GlitchrunnerIntro");
        var introScript = AddScriptIfExists(introGO, "GlitchrunnerIntro");

        if (introScript != null)
        {
            // displayImage alanını bağla
            var so = new SerializedObject(introScript);

            var displayImageProp = so.FindProperty("displayImage");
            if (displayImageProp != null)
                displayImageProp.objectReferenceValue = img;

            // nextSceneName — BlockchainMenu veya MainMenu yaz
            var nextSceneProp = so.FindProperty("nextSceneName");
            if (nextSceneProp != null)
                nextSceneProp.stringValue = "BlockchainMenu";

            so.ApplyModifiedProperties();

            Debug.Log("[KarikatürBuilder] GlitchrunnerIntro bağlandı. " +
                      "Inspector'dan introSprites dizisine 4 görseli sürükle!");
        }

        // ── 7. Sahneyi kaydet ──────────────────────────────────────────
        string scenesDir = "Assets/Scenes";
        if (!Directory.Exists(scenesDir))
            Directory.CreateDirectory(scenesDir);

        string scenePath = $"{scenesDir}/karikatür.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        // Build Settings'e ekle
        AddSceneToBuildSettings(scenePath);

        Debug.Log($"[KarikatürBuilder] ✓ Sahne oluşturuldu: {scenePath}");
        EditorUtility.DisplayDialog(
            "GLITCHRUNNER",
            "karikatür.unity başarıyla oluşturuldu!\n\n" +
            "YAPMAN GEREKEN TEK ŞEY:\n" +
            "Hierarchy'de GlitchrunnerIntro objesini seç\n" +
            "Inspector'da introSprites dizisine 4 karikatür görselini sürükle.\n\n" +
            "nextSceneName = 'BlockchainMenu' olarak ayarlandı.",
            "Tamam");
    }

    // ── Yardımcılar ────────────────────────────────────────────────────

    static Component AddScriptIfExists(GameObject go, string scriptName)
    {
        string[] guids = AssetDatabase.FindAssets($"{scriptName} t:Script");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[KarikatürBuilder] '{scriptName}.cs' bulunamadı! " +
                             "Assets/Scripts/ klasörüne koy.");
            return null;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        if (mono?.GetClass() == null) return null;
        return go.AddComponent(mono.GetClass());
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == scenePath) return;
        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[KarikatürBuilder] ✓ Build Settings'e eklendi: {scenePath}");
    }
}
#endif
