using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using System.IO;
#endif

public class GlitchrunnerIntro : MonoBehaviour
{
    [Header("Giriş Ayarları")]
    public Sprite[] introSprites; // 4 görseli buraya sürükle
    public Image displayImage;    // UI Image bileşenini buraya sürükle
    public float fadeDuration = 0.8f; // Geçiş hızı (saniye)
    
    [Header("Sahne Geçişi")]
    [Tooltip("Karikatür bitince her zaman OgreticiSahne açılır; bu alan şu an kullanılmıyor.")]
    public string nextSceneName = "OgreticiSahne";

    private int currentIndex = 0;
    private bool isTransitioning = false;
    private bool _hasRequestedSceneLoad;

    void Start()
    {
        // İlk görseli göstererek başla
        if (introSprites.Length > 0 && displayImage != null)
        {
            displayImage.sprite = introSprites[0];
            // Resmin şeffaf kalmaması için rengini full beyaz yap
            displayImage.color = Color.white; 
        }
    }

    void Update()
    {
        if (_hasRequestedSceneLoad) return;
        // Mouse sol veya sağ tık, Space veya Enter tuşuyla ilerle
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && !isTransitioning)
        {
            AdvanceIntro();
        }
    }

    void AdvanceIntro()
    {
        // #region agent log
        AgentLog("AdvanceIntro.entry", "called", "currentIndex=" + currentIndex + " len=" + (introSprites != null ? introSprites.Length : 0) + " hasRequested=" + _hasRequestedSceneLoad, "H1");
        // #endregion
        if (_hasRequestedSceneLoad) return;
        currentIndex++;

        if (currentIndex < introSprites.Length)
        {
            // Bir sonraki görsele yumuşak geçiş yap
            StartCoroutine(FadeToNextSprite(introSprites[currentIndex]));
        }
        else
        {
            // Karikatür bitince her zaman öğretici sahneye geç (Inspector’daki değer yok sayılır)
            AgentLog("AdvanceIntro.else", "defer LoadScene next frame", "currentIndex=" + currentIndex, "H2");
            _hasRequestedSceneLoad = true;
            isTransitioning = true;
            StartCoroutine(DeferredLoadOgreticiSahne());
        }
    }

    const string TutorialSceneName = "OgreticiSahne";

    // #region agent log
    static void AgentLog(string location, string message, string data, string hypothesisId)
    {
        long ts = (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalMilliseconds;
        string line = "{\"sessionId\":\"ca9fc4\",\"hypothesisId\":\"" + hypothesisId + "\",\"location\":\"" + location + "\",\"message\":\"" + message + "\",\"data\":\"" + (data ?? "") + "\",\"timestamp\":" + ts + "}";
#if UNITY_EDITOR
        try { File.AppendAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../../debug-ca9fc4.log")), line + "\n"); } catch { }
#endif
        Debug.Log("[DEBUG " + hypothesisId + "] " + location + " | " + message + " | " + data);
    }
    // #endregion

    IEnumerator DeferredLoadOgreticiSahne()
    {
        yield return null; // bir frame bekle: aynı frame re-entry önlenir
        // #region agent log
        AgentLog("DeferredLoadOgreticiSahne", "calling LoadScene", TutorialSceneName, "H5");
        // #endregion
        SceneManager.LoadScene(TutorialSceneName);
    }

    IEnumerator FadeToNextSprite(Sprite nextSprite)
    {
        isTransitioning = true;
        
        float timer = 0;
        // 1. Kararma (Fade Out)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1 - (timer / fadeDuration);
            displayImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        displayImage.sprite = nextSprite;

        // 2. Geri Açılma (Fade In)
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            displayImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        isTransitioning = false;
    }
}