using UnityEngine;
using System;

[DefaultExecutionOrder(-90)]
public class ResourceSystem : MonoBehaviour
{
    public static ResourceSystem Instance { get; private set; }
    public float FocusPoints             { get; private set; } = 0f;
    public int   TotalSessionsCompleted  { get; private set; } = 0;
    public float GlobalMultiplier        { get; private set; } = 1f;

    public event Action<float> OnFocusPointsChanged;
    public event Action<float> OnPointsEarned;
    public event Action<int>   OnSessionsChanged;
    public event Action<float> OnMultiplierChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (UnlockTree.Instance != null)
        {
            UnlockTree.Instance.OnNodeUnlocked += HandleNodeUnlocked;
            UnlockTree.Instance.OnUnlockTreeRestored += RecalculateMultiplier;
        }
    }

    void OnDisable()
    {
        if (UnlockTree.Instance != null)
        {
            UnlockTree.Instance.OnNodeUnlocked -= HandleNodeUnlocked;
            UnlockTree.Instance.OnUnlockTreeRestored -= RecalculateMultiplier;
        }
    }

    void Start()
    {
        RecalculateMultiplier();
    }

    private void HandleNodeUnlocked(string nodeId) => RecalculateMultiplier();

    public void RecalculateMultiplier()
    {
        float prev = GlobalMultiplier;
        GlobalMultiplier = UnlockTree.Instance != null
            ? UnlockTree.Instance.ComputeGlobalMultiplier()
            : 1f;
        if (Mathf.Abs(GlobalMultiplier - prev) > 0.001f)
            OnMultiplierChanged?.Invoke(GlobalMultiplier);
    }

    public void SetGlobalMultiplier(float value)
    {
        GlobalMultiplier = Mathf.Max(1f, value);
        OnMultiplierChanged?.Invoke(GlobalMultiplier);
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
        OnMultiplierChanged  = null;
        if (Instance == this) Instance = null;
    }
}
