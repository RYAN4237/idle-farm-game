using UnityEngine;

/// Attach to GameManager in TimerScene.
/// Every second, writes FocusSystem + ResourceSystem state to the shared file
/// so the Farm App can react to focus sessions completing.
public class TimerStateWriter : MonoBehaviour
{
    [Header("Write interval (seconds)")]
    public float writeInterval = 1f;

    float _timer;
    FocusFarmState _state = new FocusFarmState();

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < writeInterval) return;
        _timer = 0f;
        WriteState();
    }

    void OnDisable()         => ClearState();
    void OnApplicationQuit() => ClearState();

    void WriteState()
    {
        var fs = FocusSystem.Instance;
        var rs = ResourceSystem.Instance;
        var fb = FarmBoostReceiver.Instance;
        if (fs == null) return;

        _state.isRunning       = fs.IsRunning;
        _state.isResting       = fs.IsResting;
        _state.timeRemaining   = fs.TimeRemaining;
        _state.totalDuration   = fs.IsResting
            ? fs.restDurationMinutes  * 60f
            : fs.focusDurationMinutes * 60f;
        _state.completedCycles = fs.CompletedCycles;
        _state.focusPoints     = rs != null ? rs.FocusPoints : 0f;
        _state.totalSessions   = rs != null ? rs.TotalSessionsCompleted : 0;
        _state.boostActive     = fb != null && fb.GrowthMultiplier > 1f;
        _state.boostMultiplier = fb != null ? fb.GrowthMultiplier : 1f;

        SharedState.Write(_state);
    }

    // Write a zeroed/stale state so Farm knows Timer App closed
    void ClearState()
    {
        _state.isRunning  = false;
        _state.timestamp  = 0; // immediately stale
        SharedState.Write(_state);
    }
}
