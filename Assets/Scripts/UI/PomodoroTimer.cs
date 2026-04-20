using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PomodoroTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phaseLabelText;
    public Button          actionButton;
    public TextMeshProUGUI actionButtonText;
    public Image           progressRing;
    public GameObject[]    cycleDots;

    [Header("Visual Settings")]
    public Color workColor = new Color(0x1D/255f, 0x9E/255f, 0x75/255f);
    public Color restColor = new Color(0x37/255f, 0x8A/255f, 0xDD/255f);
    public bool  enablePulseEffect = true;

    private Coroutine pulseCoroutine;
    private bool      startCalled;
    private bool      subscribedToFocus;
    private readonly char[] timeChars = new char[5]{'0','0',':','0','0'};

    void OnEnable()
    {
        TrySubscribeToFocus();
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonPressed);
        }
        if (startCalled) RefreshUI();
    }

    void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
            if (timerText != null) timerText.transform.localScale = Vector3.one;
        }
        UnsubscribeFromFocus();
        if (actionButton != null) actionButton.onClick.RemoveAllListeners();
    }

    void Start()
    {
        startCalled = true;
        RefreshUI();
    }

    void Update()
    {
        if (!subscribedToFocus)
        {
            TrySubscribeToFocus();
            if (subscribedToFocus) RefreshUI();
        }
    }

    void TrySubscribeToFocus()
    {
        if (subscribedToFocus || FocusSystem.Instance == null) return;
        FocusSystem.Instance.OnTimerTick      += UpdateTimer;
        FocusSystem.Instance.OnRunningChanged += UpdateRunningState;
        FocusSystem.Instance.OnPhaseChanged   += UpdatePhase;
        FocusSystem.Instance.OnFocusCompleted += OnFocusCompleted;
        FocusSystem.Instance.OnRestCompleted  += OnRestCompleted;
        subscribedToFocus = true;
    }

    void UnsubscribeFromFocus()
    {
        if (!subscribedToFocus) return;
        if (FocusSystem.Instance != null)
        {
            FocusSystem.Instance.OnTimerTick      -= UpdateTimer;
            FocusSystem.Instance.OnRunningChanged -= UpdateRunningState;
            FocusSystem.Instance.OnPhaseChanged   -= UpdatePhase;
            FocusSystem.Instance.OnFocusCompleted -= OnFocusCompleted;
            FocusSystem.Instance.OnRestCompleted  -= OnRestCompleted;
        }
        subscribedToFocus = false;
    }

    public void OnActionButtonPressed()
    {
        if (FocusSystem.Instance != null) FocusSystem.Instance.ToggleTimer();
    }

    void UpdateTimer(float remaining, float total)
    {
        UpdateTimerText(remaining);
        UpdateProgressRing(remaining, total);
    }

    void UpdateRunningState(bool isRunning) => UpdateButtonText(isRunning);

    void UpdatePhase(bool isResting)
    {
        UpdatePhaseLabel(isResting);
        UpdateButtonText(FocusSystem.Instance?.IsRunning ?? false);
    }

    void OnFocusCompleted()
    {
        UpdateCycleDots();
        if (enablePulseEffect) StartPulseEffect();
    }

    void OnRestCompleted() { }

    void RefreshUI()
    {
        if (FocusSystem.Instance == null) return;
        float remaining = FocusSystem.Instance.TimeRemaining;
        float total = FocusSystem.Instance.IsResting
            ? FocusSystem.Instance.restDurationMinutes  * 60f
            : FocusSystem.Instance.focusDurationMinutes * 60f;
        UpdateTimerText(remaining);
        UpdateProgressRing(remaining, total);
        UpdatePhaseLabel(FocusSystem.Instance.IsResting);
        UpdateCycleDots();
        UpdateButtonText(FocusSystem.Instance.IsRunning);
    }

    void UpdateTimerText(float remaining)
    {
        if (timerText == null) return;
        remaining    = Mathf.Max(0f, remaining);
        int mins     = Mathf.FloorToInt(remaining / 60f);
        int secs     = Mathf.FloorToInt(remaining % 60f);
        timeChars[0] = (char)('0' + mins / 10);
        timeChars[1] = (char)('0' + mins % 10);
        timeChars[3] = (char)('0' + secs / 10);
        timeChars[4] = (char)('0' + secs % 10);
        timerText.SetText(new string(timeChars));
    }

    void UpdateProgressRing(float remaining, float total)
    {
        if (progressRing == null) return;
        if (total > 0f) progressRing.fillAmount = remaining / total;
        progressRing.color = (FocusSystem.Instance?.IsResting ?? false) ? restColor : workColor;
    }

    void UpdatePhaseLabel(bool isResting)
    {
        if (phaseLabelText != null) phaseLabelText.text = isResting ? "休息" : "专注";
    }

    void UpdateButtonText(bool isRunning)
    {
        if (actionButtonText == null) return;
        bool isResting = FocusSystem.Instance?.IsResting ?? false;
        actionButtonText.text = isRunning ? "暂停" : (isResting ? "开始休息" : "开始专注");
    }

    void UpdateCycleDots()
    {
        if (cycleDots == null || FocusSystem.Instance == null) return;
        int completed = FocusSystem.Instance.CompletedCycles;
        for (int i = 0; i < cycleDots.Length; i++)
            if (cycleDots[i] != null) cycleDots[i].SetActive(i < completed);
    }

    void StartPulseEffect()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseEffect());
    }

    System.Collections.IEnumerator PulseEffect()
    {
        if (timerText == null) yield break;
        Vector3 orig = timerText.transform.localScale;
        float dur = 0.5f, elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float s = 1f + 0.3f * Mathf.Sin((elapsed/dur) * Mathf.PI);
            timerText.transform.localScale = orig * s;
            yield return null;
        }
        timerText.transform.localScale = orig;
        pulseCoroutine = null;
    }
}
