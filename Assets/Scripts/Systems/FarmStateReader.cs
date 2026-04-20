using UnityEngine;

/// Attach to GameManager in FarmScene.
/// Polls the shared state file every second and:
///   - Syncs FocusPoints from Timer App (Timer is source of truth)
///   - Fires farm boosts when new focus cycles complete
///   - Publishes focus start/pause events to GameEventBus
public class FarmStateReader : MonoBehaviour
{
    [Header("Poll interval (seconds)")]
    public float pollInterval    = 1f;

    [Header("Boost settings")]
    public float boostMultiplier = 2f;
    public float boostDuration   = 300f; // 5 min

    [Header("Optional: connection indicator label")]
    public TMPro.TextMeshProUGUI connectionLabel;

    float _pollTimer;
    bool  _wasRunning;
    int   _lastCycles   = -1; // -1 = not yet synced
    bool  _timerConnected;

    void Update()
    {
        _pollTimer += Time.deltaTime;
        if (_pollTimer < pollInterval) return;
        _pollTimer = 0f;
        Poll();
    }

    void Poll()
    {
        var state = SharedState.Read();

        // ── Connection status ────────────────────────────────────────
        bool connected = state != null;
        if (connected != _timerConnected)
        {
            _timerConnected = connected;
            if (connectionLabel != null)
                connectionLabel.text = connected ? "● Timer" : "○ Timer";
            Debug.Log($"[FarmReader] Timer {(connected ? "connected" : "disconnected")}");
        }

        if (state == null) { _wasRunning = false; return; }

        // ── Sync focus points ────────────────────────────────────────
        if (ResourceSystem.Instance != null)
            ResourceSystem.Instance.SetFocusPoints(state.focusPoints);

        // ── First poll: just record baseline, don't fire spurious boosts
        if (_lastCycles == -1)
        {
            _lastCycles = state.completedCycles;
            _wasRunning = state.isRunning;
            return;
        }

        // ── Detect new completed focus cycles → boost farm ───────────
        if (state.completedCycles > _lastCycles)
        {
            int n = state.completedCycles - _lastCycles;
            _lastCycles = state.completedCycles;
            for (int i = 0; i < n; i++)
                GameEventBus.PublishBoost(boostMultiplier, boostDuration);
            Debug.Log($"[FarmReader] {n} cycle(s) complete → boost x{boostMultiplier} for {boostDuration}s");
        }

        // ── Detect focus start / pause → ambient effects ─────────────
        if (state.isRunning && !_wasRunning)
            GameEventBus.PublishFocusStart();
        else if (!state.isRunning && _wasRunning)
            GameEventBus.PublishFocusPause();

        _wasRunning = state.isRunning;
    }
}
