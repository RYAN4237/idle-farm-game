using System;
using UnityEngine;

/// Lightweight event bus for loose coupling between Farm and Timer modules.
/// Any system can publish/subscribe without direct references.
public static class GameEventBus
{
    // ── Focus / Timer events ──────────────────────────────────────────
    /// Timer: a focus session completed (25 min done)
    public static event Action<int> OnFocusSessionComplete; // param: streak count

    /// Timer: focus session started
    public static event Action OnFocusStart;

    /// Timer: focus paused or stopped
    public static event Action OnFocusPause;

    /// Timer: rest period started
    public static event Action<float> OnRestStart; // param: duration seconds

    // ── Farm boost events ─────────────────────────────────────────────
    /// Farm: request a temporary growth speed multiplier
    public static event Action<float, float> OnBoostRequested; // multiplier, duration

    // ── Publish helpers ───────────────────────────────────────────────
    public static void PublishFocusComplete(int streak)
    {
        Debug.Log($"[EventBus] focus_complete streak={streak}");
        OnFocusSessionComplete?.Invoke(streak);
    }

    public static void PublishFocusStart()
    {
        Debug.Log("[EventBus] focus_start");
        OnFocusStart?.Invoke();
    }

    public static void PublishFocusPause()
    {
        Debug.Log("[EventBus] focus_pause");
        OnFocusPause?.Invoke();
    }

    public static void PublishRestStart(float duration)
    {
        Debug.Log($"[EventBus] rest_start duration={duration}s");
        OnRestStart?.Invoke(duration);
    }

    public static void PublishBoost(float multiplier, float duration)
    {
        Debug.Log($"[EventBus] boost x{multiplier} for {duration}s");
        OnBoostRequested?.Invoke(multiplier, duration);
    }
}
