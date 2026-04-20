using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[DefaultExecutionOrder(50)]  // run after FocusSystem(-100) and ResourceSystem(-90)
public class PomoWidget : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI Refs")]
    public TextMeshProUGUI timerLabel;
    public TextMeshProUGUI phaseLabel;
    public TextMeshProUGUI fpLabel;
    public Image           progressRing;
    public Button          startBtn;
    public Button          resetBtn;
    public Button          collapseBtn;
    public GameObject      bodyGO;

    RectTransform _rt;
    Canvas        _canvas;
    Vector2       _dragOffset;
    bool          _dragging;
    bool          _collapsed;
    bool          _subscribed;
    float         _fpTimer;

    void Awake()
    {
        _rt     = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        if (startBtn    != null) startBtn.onClick.AddListener(OnStart);
        if (resetBtn    != null) resetBtn.onClick.AddListener(OnReset);
        if (collapseBtn != null) collapseBtn.onClick.AddListener(ToggleCollapse);

        // Try immediately — FocusSystem has DefaultExecutionOrder(-100) so Instance is set
        TrySubscribe();
        RefreshAll();
    }

    void Update()
    {
        // Fallback poll in case ordering was wrong
        if (!_subscribed)
        {
            TrySubscribe();
            if (_subscribed) RefreshAll();
        }

        // Update FP every 0.5s
        _fpTimer -= Time.deltaTime;
        if (_fpTimer <= 0f)
        {
            _fpTimer = 0.5f;
            UpdateFP();
        }
    }

    void TrySubscribe()
    {
        if (_subscribed || FocusSystem.Instance == null) return;
        FocusSystem.Instance.OnTimerTick      += OnTick;
        FocusSystem.Instance.OnRunningChanged += OnRunning;
        FocusSystem.Instance.OnPhaseChanged   += OnPhase;
        _subscribed = true;
    }

    void OnDisable()
    {
        if (!_subscribed || FocusSystem.Instance == null) return;
        FocusSystem.Instance.OnTimerTick      -= OnTick;
        FocusSystem.Instance.OnRunningChanged -= OnRunning;
        FocusSystem.Instance.OnPhaseChanged   -= OnPhase;
        _subscribed = false;
    }

    // ── Callbacks ─────────────────────────────────────────────────────
    void OnTick(float rem, float total)
    {
        SetTimerText(rem);
        if (progressRing != null && total > 0)
            progressRing.fillAmount = rem / total;
    }

    void OnRunning(bool running)
    {
        var t = startBtn?.GetComponentInChildren<TextMeshProUGUI>();
        if (t != null) t.text = running ? "PAUSE" : "START";
    }

    void OnPhase(bool resting)
    {
        if (phaseLabel != null) phaseLabel.text = resting ? "REST" : "FOCUS";
        if (progressRing != null)
            progressRing.color = resting
                ? new Color(0.2f, 0.5f, 1f)
                : new Color(0.1f, 0.85f, 0.4f);
    }

    void RefreshAll()
    {
        if (FocusSystem.Instance == null) return;
        float rem   = FocusSystem.Instance.TimeRemaining;
        float total = (FocusSystem.Instance.IsResting
            ? FocusSystem.Instance.restDurationMinutes
            : FocusSystem.Instance.focusDurationMinutes) * 60f;
        SetTimerText(rem);
        if (progressRing != null)
            progressRing.fillAmount = total > 0 ? rem / total : 1f;
        OnRunning(FocusSystem.Instance.IsRunning);
        OnPhase(FocusSystem.Instance.IsResting);
        UpdateFP();
    }

    void SetTimerText(float sec)
    {
        if (timerLabel == null) return;
        sec = Mathf.Max(0, sec);
        timerLabel.text = $"{(int)(sec / 60):00}:{(int)(sec % 60):00}";
    }

    void UpdateFP()
    {
        if (fpLabel == null) return;
        float fp = ResourceSystem.Instance != null ? ResourceSystem.Instance.FocusPoints : 0f;
        fpLabel.text = $"FP  {(int)fp}";
    }

    // ── Buttons ───────────────────────────────────────────────────────
    void OnStart() => FocusSystem.Instance?.ToggleTimer();
    void OnReset() => FocusSystem.Instance?.ResetTimer();

    void ToggleCollapse()
    {
        _collapsed = !_collapsed;
        if (bodyGO != null) bodyGO.SetActive(!_collapsed);
        var t = collapseBtn?.GetComponentInChildren<TextMeshProUGUI>();
        if (t != null) t.text = _collapsed ? "▼" : "▲";
        if (_rt != null)
            _rt.sizeDelta = new Vector2(155, _collapsed ? 28 : 195);
    }

    // ── Drag ──────────────────────────────────────────────────────────
    public void OnPointerDown(PointerEventData e)
    {
        // Only drag from header (top 28px) or when collapsed
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, e.position, e.pressEventCamera, out var local);
        if (local.y > _rt.rect.height - 28 || _collapsed)
        {
            _dragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                e.position, e.pressEventCamera, out var cl);
            _dragOffset = _rt.anchoredPosition - cl;
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_dragging) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            e.position, e.pressEventCamera, out var cl);
        _rt.anchoredPosition = cl + _dragOffset;
    }

    public void OnPointerUp(PointerEventData e) => _dragging = false;
}
