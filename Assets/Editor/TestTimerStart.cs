using UnityEngine;
using UnityEditor;

public class TestTimerStart
{
    public static void Execute()
    {
        if (FocusSystem.Instance == null)
        {
            Debug.LogError("FocusSystem.Instance is null");
            return;
        }
        FocusSystem.Instance.StartTimer();
        Debug.Log($"[Test] Timer started! Running={FocusSystem.Instance.IsRunning}, Time={FocusSystem.Instance.TimeRemaining:F0}s");
    }
}
