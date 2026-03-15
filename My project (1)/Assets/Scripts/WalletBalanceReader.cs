using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// WalletBalanceReader.cs
/// Fuji Testnet'te bağlı cüzdanın GRC token bakiyesini çeker.
/// Reown AppKit'ten adresi alır, eth_call ile ERC-20 balanceOf okur.
/// Konum: Assets/Scripts/WalletBalanceReader.cs
/// </summary>
public class WalletBalanceReader : MonoBehaviour
{
    [Header("Token Ayarları")]
    [Tooltip("Fuji Testnet'teki GRC token contract adresi")]
    public string tokenContractAddress = "0x3394Bf1cC2D6C6C51A4Da2245CB398C969De1ed4"; // TODO: gerçek adres

    [Tooltip("Token'ın decimal sayısı (genellikle 18)")]
    public int tokenDecimals = 18;

    [Header("RPC")]
    [Tooltip("Fuji Testnet RPC URL")]
    public string rpcUrl = "https://api.avax-test.network/ext/bc/C/rpc";

    [Header("Güncelleme")]
    [Tooltip("Kaç saniyede bir bakiye kontrol edilsin")]
    public float updateInterval = 15f;

    // Event — PlayerHUD bu event'i dinler
    public event Action<float> OnBalanceUpdated;

    private string _walletAddress = "";
    private float  _lastBalance   = 0f;
    private bool   _fetching      = false;

    // balanceOf(address) fonksiyon selector'ı
    private const string BalanceOfSelector = "0x70a08231";

    void Start()
    {
        // Reown AppKit'ten adres al — bağlı değilse tekrar dene
        StartCoroutine(WaitForWalletAndStart());
    }

    IEnumerator WaitForWalletAndStart()
    {
        int attempts = 0;
        while (attempts < 20)
        {
            yield return new WaitForSeconds(1f);
            attempts++;

            try
            {
                if (Reown.AppKit.Unity.AppKit.IsInitialized)
                {
                    string addr = Reown.AppKit.Unity.AppKit.AccountController?.Address ?? "";
                    if (!string.IsNullOrEmpty(addr) && addr != "0x0000000000000000000000000000000000000000")
                    {
                        _walletAddress = addr;
                        Debug.Log($"[BalanceReader] Cüzdan adresi alındı: {addr}");
                        StartCoroutine(BalanceLoop());
                        yield break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BalanceReader] AppKit kontrol: {ex.Message}");
            }
        }
        Debug.LogWarning("[BalanceReader] Cüzdan adresi alınamadı — balance gösterilmeyecek.");
    }

    IEnumerator BalanceLoop()
    {
        while (true)
        {
            if (!_fetching)
                yield return StartCoroutine(FetchBalance());
            yield return new WaitForSeconds(updateInterval);
        }
    }

    IEnumerator FetchBalance()
    {
        _fetching = true;

        // balanceOf(address) call data oluştur
        // Adres 32 byte'a pad edilir
        string paddedAddr = _walletAddress.Replace("0x", "").ToLower().PadLeft(64, '0');
        string data = BalanceOfSelector + paddedAddr;

        string json = "{\"jsonrpc\":\"2.0\",\"method\":\"eth_call\",\"params\":[{" +
                      $"\"to\":\"{tokenContractAddress}\"," +
                      $"\"data\":\"{data}\"" +
                      "},\"latest\"],\"id\":1}";

        byte[] bodyBytes = Encoding.UTF8.GetBytes(json);
        var req = new UnityWebRequest(rpcUrl, "POST");
        req.uploadHandler   = new UploadHandlerRaw(bodyBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string response = req.downloadHandler.text;
            float balance = ParseBalanceFromResponse(response);
            if (Math.Abs(balance - _lastBalance) > 0.001f)
            {
                _lastBalance = balance;
                OnBalanceUpdated?.Invoke(balance);
                Debug.Log($"[BalanceReader] Bakiye güncellendi: {balance} GRC");
            }
        }
        else
        {
            Debug.LogWarning($"[BalanceReader] RPC hatası: {req.error}");
        }

        _fetching = false;
    }

    float ParseBalanceFromResponse(string json)
    {
        // {"jsonrpc":"2.0","id":1,"result":"0x000...1234"}
        try
        {
            int resultIdx = json.IndexOf("\"result\":\"0x", StringComparison.Ordinal);
            if (resultIdx < 0) return 0f;

            int start = resultIdx + 10; // "result":"
            int end   = json.IndexOf("\"", start);
            if (end < 0) return 0f;

            string hexVal = json.Substring(start, end - start);
            if (hexVal == "0x") return 0f;

            // Hex → BigInteger → float (18 decimal)
            // Unity'de System.Numerics.BigInteger kullanılamayabilir
            // 32 byte hex → son 16 hex'i al (uint64 yeterli GRC miktarı için)
            string clean = hexVal.Replace("0x", "").TrimStart('0');
            if (clean.Length == 0) return 0f;

            // Güvenli: son 15 hex karakteri al (2^60 ~ 1.15 * 10^18 token)
            if (clean.Length > 15) clean = clean.Substring(clean.Length - 15);
            ulong raw = Convert.ToUInt64(clean, 16);

            // 18 decimal için 10^18 böl
            double divisor = Math.Pow(10, tokenDecimals);
            return (float)(raw / divisor);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BalanceReader] Parse hatası: {ex.Message}");
            return 0f;
        }
    }

    /// <summary>Dışarıdan cüzdan adresi set etmek için (Reown event'inden)</summary>
    public void SetWalletAddress(string address)
    {
        if (string.IsNullOrEmpty(address)) return;
        _walletAddress = address;
        Debug.Log($"[BalanceReader] Adres set edildi: {address}");
        StopAllCoroutines();
        StartCoroutine(BalanceLoop());
    }

    /// <summary>Anlık bakiye — son okunan değer</summary>
    public float GetLastBalance() => _lastBalance;
}
