using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Reown.AppKit.Unity;

public class WalletModalUI : MonoBehaviour
{
    [Header("Modal Kök")]
    public CanvasGroup    modalGroup;
    public RectTransform  modalPanel;

    [Header("Google Butonu")]
    public Button          googleButton;
    public TextMeshProUGUI googleLabel;

    [Header("Wallet Listesi")]
    public RectTransform walletListContent;
    public GameObject    walletRowPrefab;

    [Header("Status")]
    public TextMeshProUGUI statusText;

    [Header("Kapat Butonu")]
    public Button closeButton;

    [Header("Sahne Geçişi")]
    public string      targetSceneName = "MainMenu";
    public CanvasGroup fadeGroup;
    public float       fadeDuration    = 1.0f;

    private static readonly Color CIdle  = new Color(0f, 1f,    0.35f, 1.00f);
    private static readonly Color CWarn  = new Color(1f, 0.90f, 0f,    1.00f);
    private static readonly Color CGreen = new Color(0f, 1f,    0.40f, 1.00f);
    private static readonly Color CError = new Color(1f, 0.10f, 0.05f, 1.00f);

    private static readonly WalletInfo[] KnownWallets =
    {
        new WalletInfo("WalletConnect",   "walletconnect"),
        new WalletInfo("MetaMask",        "metamask"),
        new WalletInfo("Coinbase Wallet", "coinbase"),
        new WalletInfo("Trust Wallet",    "trust"),
        new WalletInfo("Phantom",         "phantom"),
        new WalletInfo("Rainbow",         "rainbow"),
    };

    private bool _open = false;
    private bool _busy = false;
    private static bool _initTriggered = false;
    private readonly List<GameObject> _rows = new List<GameObject>();

    void Awake()
    {
        if (modalGroup)
        {
            modalGroup.alpha          = 0f;
            modalGroup.interactable   = false;
            modalGroup.blocksRaycasts = false;
        }
        if (closeButton)  closeButton.onClick.AddListener(CloseModal);
        if (googleButton) googleButton.onClick.AddListener(OnGoogleClick);
        if (statusText)   statusText.text = "";
        if (walletListContent) walletListContent.gameObject.SetActive(true);
    }

    void Start() { StartCoroutine(WaitAndSubscribe()); }

    // AppKit hazır olana kadar bekle; WebGL'de prefab varsa initialize tetikle.
    IEnumerator WaitAndSubscribe()
    {
        int attempts = 0;
        const int maxAttempts = 30;
        const float interval = 0.5f;

        while (attempts < maxAttempts)
        {
            yield return new WaitForSeconds(interval);
            attempts++;
            try
            {
                if (AppKit.IsInitialized)
                {
                    AppKit.AccountConnected    += OnReownConnected;
                    AppKit.AccountDisconnected += OnReownDisconnected;
                    SetStatus("> READY — Choose wallet or Google");
                    SetStatusColor(CIdle);
                    Debug.Log("[WalletModal] AppKit subscribe OK.");
                    yield break;
                }
                if (attempts <= 5 && !_initTriggered && AppKit.Instance != null)
                {
                    _initTriggered = true;
                    WalletConnectBridge.EnsureInitializedAsync();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[WalletModal] AppKit subscribe {attempts}: {ex.Message}");
            }
        }

        if (AppKit.Instance == null)
        {
            SetStatus("> REOWN PREFAB EKSİK — Sahneye Reown AppKit prefab ekleyin");
            SetStatusColor(CError);
            Debug.LogWarning("[WalletModal] AppKit.Instance null — BlockchainMenu sahnesine Reown AppKit prefab'ını ekleyin.");
        }
        else
        {
            SetStatus("> WALLET LOADING — Try again in a moment");
            SetStatusColor(CWarn);
        }
        Debug.LogWarning("[WalletModal] AppKit hazır olmadı — " + maxAttempts + " deneme sonrası.");
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
        catch { }
    }

    void OnReownConnected(object sender, Connector.AccountConnectedEventArgs e)
    {
        string addr = e != null ? (e.Account.Address ?? "") : "";
        string shortAddr = addr.Length > 10
            ? $"{addr.Substring(0, 6)}...{addr.Substring(addr.Length - 4)}"
            : addr;
        SetStatus($"> ACCESS GRANTED // {shortAddr}");
        SetStatusColor(CGreen);
        _busy = false;
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
        else yield return new WaitForSeconds(fadeDuration);

        if (!string.IsNullOrEmpty(targetSceneName))
            SceneManager.LoadScene(targetSceneName);
        else
            Debug.LogWarning("[WalletModal] targetSceneName boş!");
    }

    public void OpenModal()
    {
        if (_open) return;
        _open = true;
        _busy = false;
        BuildWalletList();
        StartCoroutine(FadeIn());
    }

    public void CloseModal()
    {
        if (!_open) return;
        _open = false;
        _busy = false;
        StartCoroutine(FadeOut());
    }

    void BuildWalletList()
    {
        foreach (var r in _rows) Destroy(r);
        _rows.Clear();

        if (walletRowPrefab == null)
        {
            Debug.LogError("[WalletModal] walletRowPrefab atanmamış! Inspector'dan WalletRow.prefab bağla.");
            SetStatus("> ERROR: WALLET UI NOT CONFIGURED");
            SetStatusColor(CError);
            return;
        }
        if (walletListContent == null)
        {
            Debug.LogError("[WalletModal] walletListContent atanmamış! Inspector'dan Content objesini bağla.");
            SetStatus("> ERROR: WALLET LIST NOT CONFIGURED");
            SetStatusColor(CError);
            return;
        }

        var wallets = new List<WalletInfo>(KnownWallets);
        SetStatus($"> {wallets.Count} WALLET PROVIDER DETECTED");

        foreach (var w in wallets)
        {
            var row = Instantiate(walletRowPrefab, walletListContent);
            _rows.Add(row);

            var label = row.GetComponentInChildren<TextMeshProUGUI>();
            if (label) { label.text = w.DisplayName.ToUpper(); label.color = CIdle; }

            var img = row.transform.Find("Logo")?.GetComponent<Image>();
            if (img)
            {
                var sprite = Resources.Load<Sprite>($"WalletLogos/{w.Id}");
                if (sprite) img.sprite = sprite;
                else        img.color  = new Color(0f, 1f, 0.3f, 0.3f);
            }

            string cId   = w.Id;
            string cName = w.DisplayName;
            var btn = row.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => OnWalletClick(cId, cName));
            AddHoverEffect(row);
        }

        EnsureLayout();
    }

    void EnsureLayout()
    {
        if (!walletListContent) return;
        var layout = walletListContent.GetComponent<VerticalLayoutGroup>();
        if (!layout)
        {
            layout = walletListContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing               = 4f;
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = false;
        }
        var fitter = walletListContent.GetComponent<ContentSizeFitter>();
        if (!fitter)
        {
            fitter = walletListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

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
            if (AppKit.Instance != null)
            {
                SetStatus("> INITIALIZING... Try again in 5 sec");
                SetStatusColor(CWarn);
                WalletConnectBridge.EnsureInitializedAsync();
                yield return new WaitForSeconds(5f);
            }
            else
            {
                SetStatus("> REOWN PREFAB EKSİK — Sahneye AppKit ekleyin");
                SetStatusColor(CError);
                yield return new WaitForSeconds(2f);
            }
            SetStatus("");
            _busy = false;
            yield break;
        }

        bool err = false;
        try
        {
            Debug.Log($"[WalletModal] OpenModal (Google). AppKit.IsInitialized={AppKit.IsInitialized}");
            AppKit.OpenModal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WalletModal] Google modal: {ex.Message}");
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
        else
        {
            _busy = false;
        }
    }

    void OnWalletClick(string walletId, string displayName)
    {
        if (_busy) return;
        StartCoroutine(ConnectWallet(walletId, displayName));
    }

    IEnumerator ConnectWallet(string walletId, string displayName)
    {
        _busy = true;
        SetStatus($"> CONNECTING TO {displayName.ToUpper()}...");
        SetStatusColor(CWarn);
        yield return new WaitForSeconds(0.3f);

        if (!AppKit.IsInitialized)
        {
            if (AppKit.Instance != null)
            {
                SetStatus("> INITIALIZING... Try again in 5 sec");
                SetStatusColor(CWarn);
                WalletConnectBridge.EnsureInitializedAsync();
                yield return new WaitForSeconds(5f);
            }
            else
            {
                SetStatus("> REOWN PREFAB EKSİK — Sahneye AppKit ekleyin");
                SetStatusColor(CError);
                yield return new WaitForSeconds(2f);
            }
            SetStatus("");
            _busy = false;
            yield break;
        }

        SetStatus($"> AWAITING {displayName.ToUpper()} SIGNATURE...");
        bool err = false;
        try
        {
            Debug.Log($"[WalletModal] OpenModal çağrılıyor. AppKit.IsInitialized={AppKit.IsInitialized}");
            AppKit.OpenModal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WalletModal] Wallet connect: {ex.Message}");
            err = true;
        }

        if (err)
        {
            SetStatus("> ERROR: CONNECTION FAILED");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
        }

        _busy = false;
    }

    IEnumerator FadeIn()
    {
        modalGroup.interactable   = true;
        modalGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < 1f) { t = Mathf.Min(t + Time.deltaTime * 5f, 1f); modalGroup.alpha = t; yield return null; }
    }

    IEnumerator FadeOut()
    {
        modalGroup.interactable   = false;
        modalGroup.blocksRaycasts = false;
        float t = 1f;
        while (t > 0f) { t = Mathf.Max(t - Time.deltaTime * 5f, 0f); modalGroup.alpha = t; yield return null; }
    }

    void AddHoverEffect(GameObject row)
    {
        var trigger = row.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (!trigger) trigger = row.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var enter = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => {
            var img = row.GetComponent<Image>(); var ol = row.GetComponent<Outline>();
            if (img) img.color = new Color(0f, 1f, 0.3f, 0.07f);
            if (ol)  ol.effectColor = new Color(0f, 1f, 0.3f, 0.6f);
        });
        var exit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => {
            var img = row.GetComponent<Image>(); var ol = row.GetComponent<Outline>();
            if (img) img.color = new Color(0f, 0f, 0f, 0f);
            if (ol)  ol.effectColor = new Color(0f, 1f, 0.3f, 0.25f);
        });
        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    void SetStatus(string msg)     { if (statusText) statusText.text  = msg; }
    void SetStatusColor(Color col) { if (statusText) statusText.color = col; }

    [System.Serializable]
    public class WalletInfo
    {
        public string DisplayName;
        public string Id;
        public WalletInfo(string d, string i) { DisplayName = d; Id = i; }
    }
}
