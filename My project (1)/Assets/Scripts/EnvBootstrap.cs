using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// WebGL build'de Vercel /api/config'ten env yükler, sonra GameState'i oluşturur.
/// Config 404 olsa bile varsayılan REOWN_PROJECT_ID atanır; cüzdan bağlantısı çalışabilir.
/// </summary>
public class EnvBootstrap : MonoBehaviour
{
    const string DefaultReownProjectId = "98c021d7980856feb52faa0f9c1d314c";

    [Serializable]
    class ConfigResponse
    {
        public string REOWN_PROJECT_ID;
        public string AVALANCHE_RPC_URL;
        public string AVALANCHE_TOKEN_ADDRESS;
        public string AVALANCHE_TOKEN_DECIMALS;
        public string AVALANCHE_DISTRIBUTOR_ADDRESS;
    }

    void Start()
    {
        StartCoroutine(FetchConfigAndCreateGameState());
    }

    System.Collections.IEnumerator FetchConfigAndCreateGameState()
    {
        string configUrl = GetConfigUrl();
        bool configOk = false;
        using (var req = UnityWebRequest.Get(configUrl))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(req.downloadHandler?.text))
            {
                try
                {
                    var data = JsonUtility.FromJson<ConfigResponse>(req.downloadHandler.text);
                    var dict = new Dictionary<string, string>
                    {
                        ["REOWN_PROJECT_ID"] = data.REOWN_PROJECT_ID ?? "",
                        ["AVALANCHE_RPC_URL"] = data.AVALANCHE_RPC_URL ?? "",
                        ["AVALANCHE_TOKEN_ADDRESS"] = data.AVALANCHE_TOKEN_ADDRESS ?? "",
                        ["AVALANCHE_TOKEN_DECIMALS"] = data.AVALANCHE_TOKEN_DECIMALS ?? "",
                        ["AVALANCHE_DISTRIBUTOR_ADDRESS"] = data.AVALANCHE_DISTRIBUTOR_ADDRESS ?? ""
                    };
                    EnvLoader.SetFromDictionary(dict);
                    configOk = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[EnvBootstrap] Config parse failed: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("[EnvBootstrap] Config request failed: " + req.error + " — using default REOWN_PROJECT_ID.");
            }
        }

        if (!configOk)
            EnvLoader.SetFromDictionary(new Dictionary<string, string> { ["REOWN_PROJECT_ID"] = DefaultReownProjectId });

        CreateGameState();
        Destroy(gameObject);
    }

    static string GetConfigUrl()
    {
        try
        {
            string absolute = Application.absoluteURL;
            if (!string.IsNullOrEmpty(absolute))
            {
                var uri = new Uri(absolute);
                return uri.GetLeftPart(UriPartial.Authority) + "/api/config";
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[EnvBootstrap] GetConfigUrl: " + e.Message);
        }
        return "/api/config";
    }

    static void CreateGameState()
    {
        if (GameState.Instance != null) return;
        var go = new GameObject("GameState");
        go.AddComponent<GameState>();
        go.AddComponent<MarketUI>();
        go.AddComponent<AvalancheWallet>();
    }
}
