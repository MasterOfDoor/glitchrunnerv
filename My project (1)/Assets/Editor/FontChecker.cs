// ============================================================
// FontChecker.cs — Projede hangi TMP fontların olduğunu listeler
// Tools → GLITCHRUNNER → Check Fonts
// ============================================================
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FontChecker
{
    [MenuItem("Tools/GLITCHRUNNER/Check Fonts")]
    public static void Check()
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        if (guids.Length == 0)
        {
            Debug.Log("FONT: Projede hiç TMP Font Asset yok. ShareTechMono veya LiberationMono ekleyin.");
            return;
        }
        foreach (var g in guids)
            Debug.Log("FONT: " + AssetDatabase.GUIDToAssetPath(g));
    }
}
#endif
