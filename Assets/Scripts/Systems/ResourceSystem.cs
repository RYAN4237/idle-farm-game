using UnityEngine;
using System;

[DefaultExecutionOrder(-90)]
public class ResourceSystem : MonoBehaviour
{
    public static ResourceSystem Instance { get; private set; }
    public float FocusPoints             { get; private set; } = 0f;
    public int   TotalSessionsCompleted  { get; private set; } = 0;

    public event Action<float> OnFocusPointsChanged;
    public event Action<float> OnPointsEarned;
    public event Action<int>   OnSessionsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddFocusPoints(float amount)
    {
        if (amount <= 0f) return;
        FocusPoints += amount;
        OnFocusPointsChanged?.Invoke(FocusPoints);
        OnPointsEarned?.Invoke(amount);
    }

    public bool SpendFocusPoints(float amount)
    {
        if (amount <= 0f || FocusPoints < amount) return false;
        FocusPoints -= amount;
        OnFocusPointsChanged?.Invoke(FocusPoints);
        return true;
    }

    public void SetFocusPoints(float amount)
    {
        FocusPoints = Mathf.Max(0f, amount);
        OnFocusPointsChanged?.Invoke(FocusPoints);
    }

    public void IncrementSessions()
    {
        TotalSessionsCompleted++;
        OnSessionsChanged?.Invoke(TotalSessionsCompleted);
    }

    public void SetSessions(int count)
    {
        TotalSessionsCompleted = Mathf.Max(0, count);
        OnSessionsChanged?.Invoke(TotalSessionsCompleted);
    }

    void OnDestroy()
    {
        OnFocusPointsChanged = null;
        OnPointsEarned       = null;
        OnSessionsChanged    = null;
        if (Instance == this) Instance = null;
    }
}
