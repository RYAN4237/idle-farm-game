using UnityEngine;

/// Bridges FocusSystem events → GameEventBus.
/// Attach to GameManager or any persistent object.
public class FocusEventBridge : MonoBehaviour
{
    [Header("Boost Settings")]
    [Tooltip("Growth speed multiplier when focus session completes")]
    public float boostMultiplier = 2f;
    [Tooltip("How long the boost lasts (seconds)")]
    public float boostDuration   = 300f; // 5 min

    int _streak;

    void OnEnable()
    {
        if (FocusSystem.Instance == null) return;
        FocusSystem.Instance.OnFocusCompleted  += HandleFocusComplete;
        FocusSystem.Instance.OnRunningChanged  += HandleRunningChanged;
        FocusSystem.Instance.OnPhaseChanged    += HandlePhaseChanged;
    }

    void OnDisable()
    {
        if (FocusSystem.Instance == null) return;
        FocusSystem.Instance.OnFocusCompleted  -= HandleFocusComplete;
        FocusSystem.Instance.OnRunningChanged  -= HandleRunningChanged;
        FocusSystem.Instance.OnPhaseChanged    -= HandlePhaseChanged;
    }

    void Start() => OnEnable(); // re-subscribe if FocusSystem initialised after us

    void HandleFocusComplete()
    {
        _streak++;
        GameEventBus.PublishFocusComplete(_streak);
        // Trigger farm boost — reward for completing a session
        GameEventBus.PublishBoost(boostMultiplier, boostDuration);
    }

    void HandleRunningChanged(bool running)
    {
        if (running) GameEventBus.PublishFocusStart();
        else         GameEventBus.PublishFocusPause();
    }

    void HandlePhaseChanged(bool isResting)
    {
        if (isResting && FocusSystem.Instance != null)
            GameEventBus.PublishRestStart(FocusSystem.Instance.restDurationMinutes * 60f);
    }
}
