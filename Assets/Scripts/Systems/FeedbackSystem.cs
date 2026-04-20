using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackSystem : MonoBehaviour
{
    public static FeedbackSystem Instance { get; private set; }
    public Transform popupParent;
    public float popupDuration = 1.2f;
    public float popupRiseSpeed = 80f;
    private TMP_FontAsset cachedFont;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        var existing = FindFirstObjectByType<TextMeshProUGUI>();
        if (existing != null) cachedFont = existing.font;
        if (ResourceSystem.Instance != null)
            ResourceSystem.Instance.OnPointsEarned += OnPointsEarned;
        if (FocusSystem.Instance != null)
            FocusSystem.Instance.OnFocusCompleted += OnSessionComplete;
    }

    // Called by FarmPlot with a world-space position
    public void ShowPopup(Vector3 worldPos, string text)
    {
        SpawnPopup(text, new Color(1f, 0.9f, 0.3f, 1f), 32f, 1.2f);
    }

    void OnPointsEarned(float amount)
    {
        if (amount <= 1f)
            SpawnPopup("+" + Mathf.FloorToInt(amount), Color.white, 22f, 0.8f);
        else
            SpawnPopup("+" + Mathf.FloorToInt(amount) + " FP!", new Color(1f, 0.9f, 0.3f, 1f), 36f, 1.3f);
    }

    void OnSessionComplete()
    {
        SpawnPopup("SESSION COMPLETE!", new Color(0.3f, 1f, 0.5f, 1f), 42f, 1.8f);
    }

    void SpawnPopup(string text, Color color, float fontSize, float scale)
    {
        if (popupParent == null) return;
        GameObject go = new GameObject("Popup", typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(popupParent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(UnityEngine.Random.Range(-40f, 40f), 0f);
        rect.sizeDelta = new Vector2(400, 60);
        rect.localScale = Vector3.one * scale;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (cachedFont != null) tmp.font = cachedFont;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        StartCoroutine(AnimatePopup(go, rect, tmp));
    }

    IEnumerator AnimatePopup(GameObject go, RectTransform rect, TextMeshProUGUI tmp)
    {
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Color startColor = tmp.color;
        float startScale = rect.localScale.x;
        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;
            rect.anchoredPosition = startPos + Vector2.up * (popupRiseSpeed * t);
            float alpha = t < 0.3f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.3f) / 0.7f);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            float s = t < 0.15f ? Mathf.Lerp(0.5f, 1f, t / 0.15f) : 1f;
            rect.localScale = Vector3.one * startScale * s;
            yield return null;
        }
        Destroy(go);
    }

    void OnDestroy()
    {
        if (ResourceSystem.Instance != null)
            ResourceSystem.Instance.OnPointsEarned -= OnPointsEarned;
        if (FocusSystem.Instance != null)
            FocusSystem.Instance.OnFocusCompleted -= OnSessionComplete;
    }
}
