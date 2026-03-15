// ============================================================
//  HUDBuilder.cs
//  Konum: Assets/Editor/HUDBuilder.cs
//  Tools → GLITCHRUNNER → Build Player HUD
// ============================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class HUDBuilder
{
    [MenuItem("Tools/GLITCHRUNNER/Build Player HUD")]
    public static void BuildHUD()
    {
        // Sahnede HUDCanvas var mı?
        var existing = GameObject.Find("HUDCanvas");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("GLITCHRUNNER",
                "Sahnede zaten HUDCanvas var. Yeniden oluşturulsun mu?", "Evet", "Hayır"))
                return;
            Object.DestroyImmediate(existing);
        }

        // ── HUDCanvas ────────────────────────────────────────────────
        var canvasGO = new GameObject("HUDCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(640, 360);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ── HUDPanel (sağ üst — border = dış Image, içerik = inner) ───
        var panelGO = new GameObject("HUDPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(1f, 1f);
        panelRT.anchorMax        = new Vector2(1f, 1f);
        panelRT.pivot            = new Vector2(1f, 1f);
        panelRT.anchoredPosition = new Vector2(-9f, -9f);
        panelRT.sizeDelta        = new Vector2(135f, 82f);
        panelGO.AddComponent<Image>().color = new Color(0f, 0.85f, 0.3f, 0.55f); // Border (Linear)

        var panelInner = new GameObject("HUDPanel_Inner");
        panelInner.transform.SetParent(panelGO.transform, false);
        var innerRT = panelInner.AddComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(1f, 1f);
        innerRT.offsetMax = new Vector2(-1f, -1f);
        panelInner.AddComponent<Image>().color = new Color(0f, 0.015f, 0.005f, 0.92f);

        AddCorner(panelGO.transform, new Vector2(0f,  1f), new Vector2( 1f, -1f));
        AddCorner(panelGO.transform, new Vector2(1f,  1f), new Vector2(-1f, -1f));
        AddCorner(panelGO.transform, new Vector2(0f,  0f), new Vector2( 1f,  1f));
        AddCorner(panelGO.transform, new Vector2(1f,  0f), new Vector2(-1f,  1f));

        float yOffset = -9f;
        var (hpFill,   hpText)      = CreateBar(panelInner.transform, "HP",      new Color(0f, 1f, 0.31f, 1f),   new Color(0f, 1f, 0.31f, 0.12f), yOffset);
        yOffset -= 24f;
        var (stFill,   stText)      = CreateBar(panelInner.transform, "STAMINA", new Color(0f, 0.9f, 1f, 1f),    new Color(0f, 0.9f, 1f, 0.10f),  yOffset);
        yOffset -= 24f;
        var (balFill,  balText)     = CreateBar(panelInner.transform, "BALANCE", new Color(0.69f, 0.37f, 1f, 1f),new Color(0.5f, 0.2f, 1f, 0.10f),yOffset);

        var subGO = new GameObject("BalanceSub");
        subGO.transform.SetParent(panelInner.transform, false);
        var subRT = subGO.AddComponent<RectTransform>();
        subRT.anchorMin        = new Vector2(0f, 1f);
        subRT.anchorMax        = new Vector2(1f, 1f);
        subRT.pivot            = new Vector2(0.5f, 1f);
        subRT.anchoredPosition = new Vector2(0f, yOffset - 9f);
        subRT.sizeDelta        = new Vector2(0f, 9f);
        var subTxt = subGO.AddComponent<TextMeshProUGUI>();
        subTxt.text      = "FUJI TESTNET";
        subTxt.fontSize  = 7f;
        subTxt.color     = new Color(0.69f, 0.37f, 1f, 0.55f);
        subTxt.alignment = TextAlignmentOptions.Right;
        AssignMonoFont(subTxt);

        var hud = panelGO.AddComponent<PlayerHUD>();
        hud.hpFill       = hpFill;
        hud.hpText       = hpText;
        hud.staminaFill  = stFill;
        hud.staminaText  = stText;
        hud.balanceFill  = balFill;
        hud.balanceText  = balText;

        // BalanceSubText'i SerializedObject ile bağla
        var so = new SerializedObject(hud);
        so.FindProperty("balanceSubText")?.SetObjectReferenceValue(subTxt);
        so.ApplyModifiedProperties();

        // ── WalletBalanceReader ───────────────────────────────────────
        var readerGO = new GameObject("WalletBalanceReader");
        readerGO.transform.SetParent(canvasGO.transform, false);
        var reader = readerGO.AddComponent<WalletBalanceReader>();

        // PlayerHUD'a reader'ı bağla
        var so2 = new SerializedObject(hud);
        so2.FindProperty("balanceReader")?.SetObjectReferenceValue(reader);
        so2.ApplyModifiedProperties();

        EditorUtility.SetDirty(canvasGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[HUDBuilder] ✓ Player HUD oluşturuldu!");
        EditorUtility.DisplayDialog("GLITCHRUNNER",
            "Player HUD başarıyla oluşturuldu!\n\n" +
            "YAPMAN GEREKEN:\n" +
            "WalletBalanceReader Inspector'dan:\n" +
            "  • tokenContractAddress = GRC token adresi\n" +
            "  • tokenDecimals = 18\n\n" +
            "HP ve Stamina barları hazır — codebase'den\n" +
            "hud.SetHp() ve hud.SetStamina() çağır.",
            "Tamam");
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────

    static (RectTransform fill, TextMeshProUGUI label) CreateBar(
        Transform parent, string name, Color fillColor, Color trackColor, float yOffset)
    {
        float barWidth  = 117f;
        float barHeight = 4.5f;
        float labelH    = 10.5f;

        // Label row
        var labelRowGO = new GameObject($"{name}_LabelRow");
        labelRowGO.transform.SetParent(parent, false);
        var lrRT = labelRowGO.AddComponent<RectTransform>();
        lrRT.anchorMin        = new Vector2(0f, 1f);
        lrRT.anchorMax        = new Vector2(1f, 1f);
        lrRT.pivot            = new Vector2(0.5f, 1f);
        lrRT.anchoredPosition = new Vector2(0f, yOffset);
        lrRT.sizeDelta        = new Vector2(-18f, labelH);

        // Bar name label (sol)
        var nameTxtGO = new GameObject($"{name}_Name");
        nameTxtGO.transform.SetParent(labelRowGO.transform, false);
        var nRT = nameTxtGO.AddComponent<RectTransform>();
        nRT.anchorMin = Vector2.zero; nRT.anchorMax = new Vector2(0.5f, 1f);
        nRT.offsetMin = nRT.offsetMax = Vector2.zero;
        var nameTxt = nameTxtGO.AddComponent<TextMeshProUGUI>();
        nameTxt.text      = name;
        nameTxt.fontSize  = 8f;
        nameTxt.color     = new Color(fillColor.r, fillColor.g, fillColor.b, 0.6f);
        nameTxt.alignment = TextAlignmentOptions.BottomLeft;
        AssignMonoFont(nameTxt);

        // Bar value label (sağ)
        var valTxtGO = new GameObject($"{name}_Value");
        valTxtGO.transform.SetParent(labelRowGO.transform, false);
        var vRT = valTxtGO.AddComponent<RectTransform>();
        vRT.anchorMin = new Vector2(0.5f, 0f); vRT.anchorMax = Vector2.one;
        vRT.offsetMin = vRT.offsetMax = Vector2.zero;
        var valTxt = valTxtGO.AddComponent<TextMeshProUGUI>();
        valTxt.text      = name == "BALANCE" ? "0 GRC" : "100/100";
        valTxt.fontSize  = 8f;
        valTxt.color     = new Color(fillColor.r, fillColor.g, fillColor.b, 0.95f);
        valTxt.fontStyle = FontStyles.Bold;
        valTxt.alignment = TextAlignmentOptions.BottomRight;
        AssignMonoFont(valTxt);

        // Track (arka plan)
        var trackGO = new GameObject($"{name}_Track");
        trackGO.transform.SetParent(parent, false);
        var tRT = trackGO.AddComponent<RectTransform>();
        tRT.anchorMin        = new Vector2(0f, 1f);
        tRT.anchorMax        = new Vector2(1f, 1f);
        tRT.pivot            = new Vector2(0.5f, 1f);
        tRT.anchoredPosition = new Vector2(0f, yOffset - labelH - 1.5f);
        tRT.sizeDelta        = new Vector2(-18f, barHeight);
        var trackImg = trackGO.AddComponent<Image>();
        trackImg.color = trackColor;

        // Track outline
        var tOutline = trackGO.AddComponent<Outline>();
        tOutline.effectColor    = new Color(fillColor.r, fillColor.g, fillColor.b, 0.35f);
        tOutline.effectDistance = new Vector2(0.5f, -0.5f);

        // Fill (dolu kısım) — anchor tabanlı dolum
        var fillGO = new GameObject($"{name}_Fill");
        fillGO.transform.SetParent(trackGO.transform, false);
        var fRT = fillGO.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero;
        fRT.anchorMax = new Vector2(1f, 1f); // %100 başlangıç (HP ve Stamina için)
        if (name == "BALANCE") fRT.anchorMax = new Vector2(0f, 1f); // Balance 0'dan başlar
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = fillColor;

        return (fRT, valTxt);
    }

    static void AddCorner(Transform parent, Vector2 anchor, Vector2 direction)
    {
        var go = new GameObject("Corner");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.sizeDelta = new Vector2(4.5f, 4.5f);
        rt.anchoredPosition = new Vector2(direction.x * 0.75f, direction.y * 0.75f);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0.85f, 0.3f, 0.55f);
    }

    static void AssignMonoFont(TextMeshProUGUI txt)
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset ShareTechMono");
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("t:TMP_FontAsset LiberationMono");
        if (guids.Length > 0)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
            if (font) txt.font = font;
        }
    }
}

static class SerializedPropertyExtensions
{
    public static void SetObjectReferenceValue(this UnityEditor.SerializedProperty prop, Object val)
    {
        if (prop != null) prop.objectReferenceValue = val;
    }
}
#endif
