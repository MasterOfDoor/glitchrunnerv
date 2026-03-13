using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Reown.AppKit.Unity;

/// <summary>
<<<<<<< HEAD
/// WalletModalUI.cs — WebGL uyumlu, derleme hataları giderildi
///
/// DÜZELTMELER:
///   - ViewType.SocialLogin yok → AppKit.OpenModal() kullanılıyor
///   - ConnectorController.Connectors yok → GetAvailableWallets() sadece KnownWallets döndürüyor
///   - Google WebGL: SocialLogin.Google.Open() → AppKit.OpenModal() ile Reown kendi UI'ında Google sunuyor
=======
/// WalletModalUI.cs
/// Reown AppKit'in kendi modal'ı yerine kendi distopik UI'ımızı kullanır.
/// Kullanıcının yüklü wallet'larını listeler, logolar + isimlerle gösterir.
/// Google giriş butonu en üstte sabit durur.
/// Konum: Assets/Scripts/WalletModalUI.cs
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
/// </summary>
public class WalletModalUI : MonoBehaviour
{
    [Header("Modal Kök")]
<<<<<<< HEAD
    public CanvasGroup    modalGroup;
    public RectTransform  modalPanel;

    [Header("Google Butonu")]
    public Button          googleButton;
    public TextMeshProUGUI googleLabel;

    [Header("Wallet Listesi")]
    public RectTransform walletListContent;
    public GameObject    walletRowPrefab;
=======
    public CanvasGroup modalGroup;       // Modal'ın tüm CanvasGroup'u
    public RectTransform modalPanel;     // Ana panel RectTransform

    [Header("Google Butonu")]
    public Button      googleButton;
    public TextMeshProUGUI googleLabel;

    [Header("Wallet Listesi")]
    public RectTransform  walletListContent;  // ScrollRect > Viewport > Content
    public GameObject     walletRowPrefab;    // Wallet satır prefab'ı
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce

    [Header("Status")]
    public TextMeshProUGUI statusText;

    [Header("Kapat Butonu")]
    public Button closeButton;

    [Header("Sahne Geçişi")]
<<<<<<< HEAD
    public string      targetSceneName = "MainMenu";
    public CanvasGroup fadeGroup;
    public float       fadeDuration    = 1.0f;

    private static readonly Color CIdle  = new Color(0f, 1f,    0.35f, 1.00f);
    private static readonly Color CWarn  = new Color(1f, 0.90f, 0f,    1.00f);
    private static readonly Color CGreen = new Color(0f, 1f,    0.40f, 1.00f);
    private static readonly Color CError = new Color(1f, 0.10f, 0.05f, 1.00f);

    // WalletConnect her zaman başta — WebGL'de QR ile çalışır
    private static readonly WalletInfo[] KnownWallets =
    {
        new WalletInfo("WalletConnect",   "walletconnect"),
        new WalletInfo("MetaMask",        "metamask"),
=======
    [Tooltip("Bağlantı sonrası gidilecek sahne adı (Build Settings'te olmalı)")]
    public string targetSceneName = "MainMenu";
    [Tooltip("Geçiş öncesi FadeCanvas'ın CanvasGroup'u")]
    public CanvasGroup fadeGroup;
    [Tooltip("Geçiş süresi (saniye)")]
    public float fadeDuration = 1.0f;

    // Renk sabitleri — parlak ve net
    private static readonly Color CIdle  = new Color(0f, 1f, 0.35f, 1.00f);
    private static readonly Color CWarn  = new Color(1f, 0.90f, 0f, 1.00f);
    private static readonly Color CGreen = new Color(0f, 1f, 0.40f, 1.00f);
    private static readonly Color CDim   = new Color(0f, 0.80f, 0.25f, 0.90f);

    // Desteklenen wallet listesi — Reown SDK'nın algıladığı isimlerle eşleşmeli
    private static readonly WalletInfo[] KnownWallets =
    {
        new WalletInfo("MetaMask",        "metamask"),
        new WalletInfo("WalletConnect",   "walletconnect"),
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
        new WalletInfo("Coinbase Wallet", "coinbase"),
        new WalletInfo("Trust Wallet",    "trust"),
        new WalletInfo("Phantom",         "phantom"),
        new WalletInfo("Rainbow",         "rainbow"),
<<<<<<< HEAD
    };

    private bool _open = false;
    private bool _busy = false;
    private readonly List<GameObject> _rows = new List<GameObject>();

    // ── Lifecycle ─────────────────────────────────────────────────────────
=======
        new WalletInfo("Ledger",          "ledger"),
        new WalletInfo("Trezor",          "trezor"),
    };

    private bool _open   = false;
    private bool _busy   = false;
    private readonly List<GameObject> _rows = new List<GameObject>();

>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    void Awake()
    {
        if (modalGroup)
        {
            modalGroup.alpha          = 0f;
            modalGroup.interactable   = false;
            modalGroup.blocksRaycasts = false;
        }
<<<<<<< HEAD
        if (closeButton)  closeButton.onClick.AddListener(CloseModal);
        if (googleButton) googleButton.onClick.AddListener(OnGoogleClick);
        if (statusText)   statusText.text = "";
=======

        if (closeButton) closeButton.onClick.AddListener(CloseModal);
        if (googleButton) googleButton.onClick.AddListener(OnGoogleClick);
        if (statusText) statusText.text = "";
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    }

    void Start()
    {
<<<<<<< HEAD
        TrySubscribeAppKit();
    }

    void TrySubscribeAppKit()
    {
        try
        {
            if (AppKit.IsInitialized)
            {
                AppKit.AccountConnected    += OnReownConnected;
                AppKit.AccountDisconnected += OnReownDisconnected;
            }
            else
            {
                StartCoroutine(RetrySubscribe());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[WalletModal] AppKit subscribe: {ex.Message}");
            StartCoroutine(RetrySubscribe());
        }
    }

    IEnumerator RetrySubscribe()
    {
        yield return new WaitForSeconds(1.5f);
        TrySubscribeAppKit();
=======
        if (AppKit.IsInitialized)
        {
            AppKit.AccountConnected    += OnReownConnected;
            AppKit.AccountDisconnected += OnReownDisconnected;
        }
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    }

    void OnDestroy()
    {
        try
        {
            if (AppKit.IsInitialized)
            {
                AppKit.AccountConnected    -= OnReownConnected;
                AppKit.AccountDisconnected -= OnReownDisconnected;
            }
        }
<<<<<<< HEAD
        catch { }
    }

    // ── Reown Events ─────────────────────────────────────────────────────
=======
        catch (System.Exception)
        {
            // AppKit zaten kaldırılmış olabilir (örn. Play mode bitince)
        }
    }

>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    void OnReownConnected(object sender, Connector.AccountConnectedEventArgs e)
    {
        string addr = e != null ? (e.Account.Address ?? "") : "";
        string shortAddr = addr.Length > 10
            ? $"{addr.Substring(0, 6)}...{addr.Substring(addr.Length - 4)}"
            : addr;
<<<<<<< HEAD
        SetStatus($"> ACCESS GRANTED // {shortAddr}");
        SetStatusColor(CGreen);
        _busy = false;
=======

        SetStatus($"> ACCESS GRANTED // {shortAddr}");
        SetStatusColor(new Color(0f, 1f, 0.35f, 1f));
        _busy = false;

>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
        StartCoroutine(SuccessAndLoad());
    }

    void OnReownDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
    {
        SetStatus("> DISCONNECTED");
        _busy = false;
    }

    IEnumerator SuccessAndLoad()
    {
        yield return new WaitForSeconds(1.2f);

        if (fadeGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(t + Time.deltaTime / fadeDuration, 1f);
                fadeGroup.alpha = t;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        if (!string.IsNullOrEmpty(targetSceneName))
            SceneManager.LoadScene(targetSceneName);
        else
<<<<<<< HEAD
            Debug.LogWarning("[WalletModal] targetSceneName boş!");
    }

    // ── Modal Aç / Kapat ─────────────────────────────────────────────────
=======
            Debug.LogWarning("[WalletModal] targetSceneName boş! Inspector'dan sahne adını gir.");
    }

    IEnumerator DelayedClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseModal();
    }

    // ── Dışarıdan çağrılır (BootPanelController'dan) ─────────────────────
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    public void OpenModal()
    {
        if (_open) return;
        _open = true;
        BuildWalletList();
        StartCoroutine(FadeIn());
    }

    public void CloseModal()
    {
        if (!_open) return;
        _open = false;
        StartCoroutine(FadeOut());
    }

<<<<<<< HEAD
    // ── Wallet Listesi ───────────────────────────────────────────────────
    void BuildWalletList()
    {
        foreach (var r in _rows) Destroy(r);
        _rows.Clear();

        var wallets = GetAvailableWallets();
        SetStatus($"> {wallets.Count} WALLET PROVIDER DETECTED");

        foreach (var w in wallets)
        {
            if (walletRowPrefab == null || walletListContent == null) break;
=======
    // ── Wallet listesini oluştur ──────────────────────────────────────────
    void BuildWalletList()
    {
        // Eski satırları temizle
        foreach (var r in _rows) Destroy(r);
        _rows.Clear();

        // Reown'dan mevcut wallet'ları al
        // NOT: Bu kısım Cursor tarafından Reown SDK'ya göre doldurulacak
        // Şimdilik KnownWallets listesini filtrele (SDK entegrasyonu sonrası güncellenir)
        var available = GetAvailableWallets();

        SetStatus($"> {available.Count} WALLET PROVIDER DETECTED");

        foreach (var w in available)
        {
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
            var row = Instantiate(walletRowPrefab, walletListContent);
            _rows.Add(row);

            var label = row.GetComponentInChildren<TextMeshProUGUI>();
            if (label) { label.text = w.DisplayName.ToUpper(); label.color = CIdle; }

<<<<<<< HEAD
=======
            // Logo sprite — Resources/WalletLogos/{w.Id} adında yükle
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
            var img = row.transform.Find("Logo")?.GetComponent<Image>();
            if (img)
            {
                var sprite = Resources.Load<Sprite>($"WalletLogos/{w.Id}");
                if (sprite) img.sprite = sprite;
<<<<<<< HEAD
                else        img.color  = new Color(0f, 1f, 0.3f, 0.3f);
            }

            string capturedId   = w.Id;
            string capturedName = w.DisplayName;
            var btn = row.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => OnWalletClick(capturedId, capturedName));
            AddHoverEffect(row);
        }

        EnsureLayout();
    }

    List<WalletInfo> GetAvailableWallets()
    {
        // ConnectorController.Connectors bu SDK sürümünde yok.
        // KnownWallets listesi kullanılıyor — WebGL'de her wallet AppKit.OpenModal() ile açılır.
        return new List<WalletInfo>(KnownWallets);
    }

    void EnsureLayout()
    {
        if (!walletListContent) return;
=======
                else img.color = new Color(0f, 1f, 0.3f, 0.3f); // bulunamazsa yeşil placeholder
            }

            // Tıklama — walletId'yi capture et
            string id = w.Id;
            var btn = row.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => OnWalletClick(id, w.DisplayName));

            // Hover efekti
            AddHoverEffect(row);
        }

        // İçerik yüksekliğini ayarla (scroll için)
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
        var layout = walletListContent.GetComponent<VerticalLayoutGroup>();
        if (!layout)
        {
            layout = walletListContent.gameObject.AddComponent<VerticalLayoutGroup>();
<<<<<<< HEAD
            layout.spacing               = 4f;
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = false;
=======
            layout.spacing          = 4f;
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(0, 0, 0, 0);
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
        }
        var fitter = walletListContent.GetComponent<ContentSizeFitter>();
        if (!fitter)
        {
            fitter = walletListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

<<<<<<< HEAD
    // ── Google Butonu ─────────────────────────────────────────────────────
    // WebGL'de SocialLogin.Google.Open() browser popup bloğuna takılır.
    // Çözüm: AppKit.OpenModal() açılır, Reown kendi UI'ında Google seçeneğini
    // otomatik gösterir. Kullanıcı oradan Google'ı seçer.
    void OnGoogleClick()
    {
        if (_busy) return;
        StartCoroutine(ConnectGoogle());
    }

    IEnumerator ConnectGoogle()
    {
        _busy = true;
        SetStatus("> OPENING GOOGLE AUTH...");
        SetStatusColor(CWarn);
        yield return new WaitForSeconds(0.2f);

        if (!AppKit.IsInitialized)
        {
            SetStatus("> SYSTEM NOT READY — TRY AGAIN IN A MOMENT");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
            yield break;
        }

        bool err = false;
        try
        {
            // WebGL'de en güvenli yol: normal modal açılır.
            // Reown'un WebGL UI'ında Google seçeneği otomatik çıkar.
            AppKit.OpenModal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WalletModal] Google modal hatası: {ex.Message}");
            err = true;
        }

        if (err)
        {
            SetStatus("> ERROR: GOOGLE AUTH UNAVAILABLE");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
        }
        // Başarı → OnReownConnected event'i
    }

    // ── Wallet Tıklama ────────────────────────────────────────────────────
    void OnWalletClick(string walletId, string displayName)
    {
        if (_busy) return;
        StartCoroutine(ConnectWallet(walletId, displayName));
    }

    IEnumerator ConnectWallet(string walletId, string displayName)
    {
        _busy = true;
        SetStatus($"> CONNECTING TO {displayName.ToUpper()}...");
=======
    List<WalletInfo> GetAvailableWallets()
    {
        // Bu SDK sürümünde ConnectorController.AvailableWallets yok; bilinen liste kullanılıyor
        return new List<WalletInfo>(KnownWallets);
    }

    // ── Google ile giriş ─────────────────────────────────────────────────
    void OnGoogleClick()
    {
        if (_busy) return;
        StartCoroutine(ConnectSequence("google", "GOOGLE"));
    }

    // ── Wallet tıklama ────────────────────────────────────────────────────
    void OnWalletClick(string walletId, string displayName)
    {
        if (_busy) return;
        StartCoroutine(ConnectSequence(walletId, displayName.ToUpper()));
    }

    IEnumerator ConnectSequence(string walletId, string displayName)
    {
        _busy = true;
        SetStatus($"> CONNECTING TO {displayName}...");
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
        SetStatusColor(CWarn);
        yield return new WaitForSeconds(0.3f);

        if (!AppKit.IsInitialized)
        {
<<<<<<< HEAD
            SetStatus("> SYSTEM NOT READY — TRY AGAIN IN A MOMENT");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
=======
            SetStatus("> APP NOT READY — Start from main menu to enable wallet.");
            SetStatusColor(new Color(1f, 0.5f, 0f, 0.9f));
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
            _busy = false;
            yield break;
        }

<<<<<<< HEAD
        SetStatus($"> AWAITING {displayName.ToUpper()} SIGNATURE...");

        bool err = false;
        try
        {
            // WebGL'de tüm wallet'lar için AppKit.OpenModal() kullanılır.
            // Reown kendi QR/deep-link akışını yönetir.
            AppKit.OpenModal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WalletModal] Wallet connect hatası: {ex.Message}");
            err = true;
        }

        if (err)
        {
            SetStatus("> ERROR: CONNECTION FAILED");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
        }
        // Başarı → OnReownConnected event'i
    }

    // ── Fade ─────────────────────────────────────────────────────────────
=======
        bool connectError = false;
        if (walletId == "google")
        {
            SetStatus("> OPENING GOOGLE AUTH...");
            yield return new WaitForSeconds(0.2f);
            try { SocialLogin.Google.Open(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[WalletModal] Google connect hatası: {e.Message}");
                connectError = true;
            }
        }
        else
        {
            SetStatus($"> AWAITING {displayName} SIGNATURE...");
            yield return new WaitForSeconds(0.2f);
            try { AppKit.OpenModal(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[WalletModal] Wallet connect hatası: {e.Message}");
                connectError = true;
            }
        }

        if (connectError)
        {
            yield return StartCoroutine(ShowConnectError());
            yield break;
        }
        // Başarı OnReownConnected event'i ile gelir
    }

    IEnumerator ShowConnectError()
    {
        SetStatus("> ERROR: CONNECTION FAILED");
        SetStatusColor(new Color(1f, 0.1f, 0.05f, 0.9f));
        yield return new WaitForSeconds(2f);
        SetStatus("");
        _busy = false;
    }

    // ── Fade animasyonları ────────────────────────────────────────────────
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    IEnumerator FadeIn()
    {
        modalGroup.interactable   = true;
        modalGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime * 5f, 1f);
            modalGroup.alpha = t;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        modalGroup.interactable   = false;
        modalGroup.blocksRaycasts = false;
        float t = 1f;
        while (t > 0f)
        {
            t = Mathf.Max(t - Time.deltaTime * 5f, 0f);
            modalGroup.alpha = t;
            yield return null;
        }
    }

<<<<<<< HEAD
    // ── Hover ─────────────────────────────────────────────────────────────
=======
    // ── Hover efekti ─────────────────────────────────────────────────────
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    void AddHoverEffect(GameObject row)
    {
        var trigger = row.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (!trigger) trigger = row.AddComponent<UnityEngine.EventSystems.EventTrigger>();

<<<<<<< HEAD
        var enter = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            var ol  = row.GetComponent<Outline>();
            if (img) img.color      = new Color(0f, 1f, 0.3f, 0.07f);
            if (ol)  ol.effectColor = new Color(0f, 1f, 0.3f, 0.6f);
        });

        var exit = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exit.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            var ol  = row.GetComponent<Outline>();
            if (img) img.color      = new Color(0f, 0f, 0f, 0f);
            if (ol)  ol.effectColor = new Color(0f, 1f, 0.3f, 0.25f);
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    void SetStatus(string msg)     { if (statusText) statusText.text  = msg; }
    void SetStatusColor(Color col) { if (statusText) statusText.color = col; }

=======
        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            if (img) img.color = new Color(0f, 1f, 0.3f, 0.07f);
            var outline = row.GetComponent<Outline>();
            if (outline) outline.effectColor = new Color(0f, 1f, 0.3f, 0.6f);
        });

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            if (img) img.color = new Color(0f, 0f, 0f, 0f);
            var outline = row.GetComponent<Outline>();
            if (outline) outline.effectColor = new Color(0f, 1f, 0.3f, 0.25f);
        });

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    void SetStatus(string msg)      { if (statusText) statusText.text  = msg; }
    void SetStatusColor(Color col)  { if (statusText) statusText.color = col; }

    static TMP_FontAsset LoadMonoFont()
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts/ShareTechMono-Regular SDF");
        if (font != null) return font;
        return TMP_Settings.defaultFontAsset;
    }

    // ── Wallet bilgi yapısı ───────────────────────────────────────────────
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    [System.Serializable]
    public class WalletInfo
    {
        public string DisplayName;
        public string Id;
<<<<<<< HEAD
        public WalletInfo(string d, string i) { DisplayName = d; Id = i; }
=======
        public WalletInfo(string displayName, string id) { DisplayName = displayName; Id = id; }
>>>>>>> 36b6cf30049d4f4c30389724499d4253fff5bcce
    }
}
