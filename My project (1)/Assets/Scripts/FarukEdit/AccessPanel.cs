using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// KURULUM:
/// 1. Panel objesi (Sprite) → bu script + DoorController yok, sadece bu
/// 2. TMP Text child → "ERİŞİM REDDEDİLDİ"
/// 3. Trigger collider (herhangi bir obje) → AccessPanelTrigger.cs
/// </summary>
public class AccessPanel : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private TMP_Text    panelText;
    [SerializeField] private SpriteRenderer panelSprite;

    [Header("Renkler")]
    [SerializeField] private Color redColor  = new Color(1f, 0.06f, 0.25f);   // #FF0F40
    [SerializeField] private Color scanColor = new Color(0f, 0.9f,  1f, 0.15f); // cyan scan

    [Header("Efekt")]
    [SerializeField] private float glitchDuration  = 0.5f;
    [SerializeField] private float fadeDuration    = 0.4f;

    // Yazı varyasyonları — glitch efekti için
    readonly string[] _lines =
    {
        "ERİŞİM\nREDDEDİLDİ",
        "3Rİ$İM\nR3DD3D İLDİ",
        "ER|Ş|M\nR▓DD▓D|LD|",
        "E̷R̷İ̷Ş̷İ̷M̷\nR̷E̷D̷D̷E̷D̷İ̷L̷D̷İ̷",
        "█RİŞİM\n█EDDEDILDI",
    };

    bool _triggered;

    // ── Pulsing kırmızı — Update'te ─────────────────────────
    float _pulseT;

    void Update()
    {
        if (_triggered) return;

        // Yazı parlaklık nabzı
        _pulseT += Time.deltaTime * 2.2f;
        float brightness = 0.6f + 0.4f * Mathf.Sin(_pulseT);
        if (panelText != null)
            panelText.color = redColor * brightness + Color.white * (brightness * 0.08f);
    }

    // ── Dışarıdan tetiklenir ─────────────────────────────────
    public void Dismiss()
    {
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(DismissSequence());
    }

    IEnumerator DismissSequence()
    {
        // 1. Hızlı glitch
        yield return StartCoroutine(GlitchOut());

        // 2. Fade out
        yield return StartCoroutine(FadeOut());

        // 3. Tamamen yok et
        gameObject.SetActive(false);
    }

    IEnumerator GlitchOut()
    {
        if (panelText == null) yield break;

        float t = 0f;
        while (t < glitchDuration)
        {
            panelText.text  = _lines[Random.Range(0, _lines.Length)];
            panelText.color = Random.value > 0.5f
                ? redColor
                : new Color(1f, 1f, 1f, Random.Range(0.3f, 1f));

            // Panel hafif sarsıntısı
            if (panelSprite != null)
                panelSprite.transform.localPosition = new Vector3(
                    Random.Range(-0.04f, 0.04f), 0f, 0f);

            t += Time.deltaTime;
            yield return new WaitForSeconds(0.04f);
        }

        // Sıfırla
        if (panelSprite != null)
            panelSprite.transform.localPosition = Vector3.zero;
    }

    IEnumerator FadeOut()
    {
        float t = 0f;

        Color startText   = panelText   != null ? panelText.color   : Color.white;
        Color startSprite = panelSprite != null ? panelSprite.color  : Color.white;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float alpha = 1f - t;

            if (panelText   != null) { var c = startText;   c.a = alpha; panelText.color   = c; }
            if (panelSprite != null) { var c = startSprite; c.a = alpha; panelSprite.color = c; }

            yield return null;
        }
    }
}
