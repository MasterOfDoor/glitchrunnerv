using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// PlayerHUD.cs
/// Sağ üstte HP, Stamina ve Balance barlarını yönetir.
/// BlockchainMenu ve MainMenu sahnelerinde otomatik gizlenir.
/// Konum: Assets/Scripts/PlayerHUD.cs
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    [Header("HP Bar")]
    public RectTransform   hpFill;
    public TextMeshProUGUI hpText;
    public float maxHp      = 100f;
    [Range(0f, 100f)] public float currentHp = 100f;

    [Header("Stamina Bar")]
    public RectTransform   staminaFill;
    public TextMeshProUGUI staminaText;
    public float maxStamina = 100f;
    [Range(0f, 100f)] public float currentStamina = 100f;

    [Header("Balance Bar")]
    public RectTransform   balanceFill;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI balanceSubText;
    public float maxBalanceDisplay = 10000f;

    [Header("Gizlenecek Sahneler")]
    [Tooltip("HUD'un gizleneceği sahne adları")]
    public List<string> hiddenScenes = new List<string> { "BlockchainMenu", "MainMenu", "karikatür" };

    [Header("Renk Eşikleri — HP")]
    public Color hpHighColor = new Color(0f,  1f,    0.31f, 1f);
    public Color hpMidColor  = new Color(1f,  0.67f, 0f,    1f);
    public Color hpLowColor  = new Color(1f,  0.18f, 0.05f, 1f);

    [Header("WalletBalanceReader")]
    public WalletBalanceReader balanceReader;

    private static readonly Color StaminaColor = new Color(0f,   0.9f,  1f,   1f);
    private static readonly Color BalanceColor = new Color(0.69f, 0.37f, 1f,  1f);

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        Instance = this;

        // DontDestroyOnLoad — tüm sahnelerde kalır
        DontDestroyOnLoad(transform.root.gameObject);

        // CanvasGroup ekle — kolay show/hide için
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Sahne değişikliklerini dinle
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateHpBar(currentHp);
        UpdateStaminaBar(currentStamina);
        UpdateBalanceBar(0f);

        if (balanceReader != null)
            balanceReader.OnBalanceUpdated += UpdateBalanceBar;

        // Başlangıç sahnesini kontrol et
        CheckSceneVisibility(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (balanceReader != null)
            balanceReader.OnBalanceUpdated -= UpdateBalanceBar;
        if (Instance == this)
            Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneVisibility(scene.name);
    }

    void CheckSceneVisibility(string sceneName)
    {
        bool shouldHide = hiddenScenes.Contains(sceneName);
        SetHUDVisible(!shouldHide);

        Debug.Log($"[PlayerHUD] Sahne: {sceneName} → HUD {(shouldHide ? "GİZLİ" : "GÖRÜNÜR")}");
    }

    void SetHUDVisible(bool visible)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha          = visible ? 1f : 0f;
        _canvasGroup.interactable   = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    // ── HP ───────────────────────────────────────────────────────────────
    public void SetHp(float value)
    {
        currentHp = Mathf.Clamp(value, 0f, maxHp);
        UpdateHpBar(currentHp);
    }

    public void DamageHp(float amount)  => SetHp(currentHp - amount);
    public void HealHp(float amount)    => SetHp(currentHp + amount);

    void UpdateHpBar(float value)
    {
        float pct = Mathf.Clamp01(value / maxHp);
        SetBarFill(hpFill, pct);
        if (hpText) hpText.text = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(maxHp)}";
        Color c = pct > 0.5f ? hpHighColor : pct > 0.25f ? hpMidColor : hpLowColor;
        SetBarColor(hpFill, c);
    }

    // ── Stamina ──────────────────────────────────────────────────────────
    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        UpdateStaminaBar(currentStamina);
    }

    public void UseStamina(float amount)     => SetStamina(currentStamina - amount);
    public void RecoverStamina(float amount) => SetStamina(currentStamina + amount);

    void UpdateStaminaBar(float value)
    {
        float pct = Mathf.Clamp01(value / maxStamina);
        SetBarFill(staminaFill, pct);
        if (staminaText) staminaText.text = $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(maxStamina)}";
        SetBarColor(staminaFill, StaminaColor);
    }

    // ── Balance ──────────────────────────────────────────────────────────
    void UpdateBalanceBar(float grcAmount)
    {
        float pct = Mathf.Clamp01(grcAmount / maxBalanceDisplay);
        SetBarFill(balanceFill, pct);

        if (balanceText)
            balanceText.text = grcAmount >= 1000f
                ? $"{(grcAmount / 1000f):0.0}K GRC"
                : $"{Mathf.RoundToInt(grcAmount)} GRC";

        if (balanceSubText)
            balanceSubText.text = "FUJI TESTNET";

        SetBarColor(balanceFill, BalanceColor);
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────
    void SetBarFill(RectTransform fill, float pct)
    {
        if (!fill) return;
        var anc = fill.anchorMax;
        anc.x = pct;
        fill.anchorMax = anc;
    }

    void SetBarColor(RectTransform fill, Color color)
    {
        if (!fill) return;
        var img = fill.GetComponent<Image>();
        if (img) img.color = color;
    }
}
