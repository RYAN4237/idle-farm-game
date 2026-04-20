using UnityEngine;
using System;

[DefaultExecutionOrder(-100)]
public class FocusSystem : MonoBehaviour
{
    public static FocusSystem Instance { get; private set; }

    [Header("Timer Settings")]
    public float focusDurationMinutes = 25f;
    public float restDurationMinutes  = 5f;

    [Header("Rewards")]
    public float focusCompletionReward = 10f;

    [Header("Audio")]
    public AudioClip   alarmClip;
    public AudioSource audioSource;

    public float TimeRemaining   { get; private set; }
    public bool  IsRunning       { get; private set; }
    public bool  IsResting       { get; private set; }
    public int   CompletedCycles { get; private set; }

    public event Action<float, float> OnTimerTick;
    public event Action               OnFocusCompleted;
    public event Action               OnRestCompleted;
    public event Action<bool>         OnRunningChanged;
    public event Action<bool>         OnPhaseChanged;

    float _totalDuration;
    int   _lastSec = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        IsResting      = false;
        _totalDuration = focusDurationMinutes * 60f;
        TimeRemaining  = _totalDuration;
    }

    void Start() => ResetTimer();

    void Update()
    {
        if (!IsRunning) return;
        TimeRemaining -= Time.deltaTime;

        int curSec = Mathf.CeilToInt(Mathf.Max(TimeRemaining, 0f));
        if (curSec != _lastSec)
        {
            _lastSec = curSec;
            OnTimerTick?.Invoke(TimeRemaining, _totalDuration);
        }

        if (TimeRemaining <= 0f) CompletePhase();
    }

    void CompletePhase()
    {
        IsRunning = false;
        if (audioSource != null && alarmClip != null)
            audioSource.PlayOneShot(alarmClip);

        if (!IsResting)
        {
            CompletedCycles++;
            if (ResourceSystem.Instance != null)
                ResourceSystem.Instance.AddFocusPoints(focusCompletionReward);
            OnFocusCompleted?.Invoke();
            IsResting      = true;
            _totalDuration = restDurationMinutes * 60f;
            TimeRemaining  = _totalDuration;
            OnPhaseChanged?.Invoke(true);
        }
        else
        {
            OnRestCompleted?.Invoke();
            IsResting      = false;
            _totalDuration = focusDurationMinutes * 60f;
            TimeRemaining  = _totalDuration;
            OnPhaseChanged?.Invoke(false);
        }
        OnRunningChanged?.Invoke(false);
    }

    // ── Public API ────────────────────────────────────────────────────
    public void ToggleTimer()
    {
        IsRunning = !IsRunning;
        OnRunningChanged?.Invoke(IsRunning);
    }

    public void StartTimer()
    {
        IsRunning = true;
        OnRunningChanged?.Invoke(true);
    }

    public void ResetTimer()
    {
        IsRunning      = false;
        IsResting      = false;
        _totalDuration = focusDurationMinutes * 60f;
        TimeRemaining  = _totalDuration;
        _lastSec       = -1;
        OnRunningChanged?.Invoke(false);
        OnPhaseChanged?.Invoke(false);
        OnTimerTick?.Invoke(TimeRemaining, _totalDuration);
    }

    /// Set focus duration in minutes (called by DurationDragger)
    public void SetDuration(float minutes)
    {
        focusDurationMinutes = Mathf.Max(1f, minutes);
        if (!IsRunning && !IsResting) ResetTimer();
    }

    /// Set rest duration in minutes
    public void SetRestDuration(float minutes)
    {
        restDurationMinutes = Mathf.Max(1f, minutes);
    }
}
