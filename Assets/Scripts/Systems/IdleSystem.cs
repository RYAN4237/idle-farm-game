using UnityEngine;

public class IdleSystem : MonoBehaviour
{
    public static IdleSystem Instance { get; private set; }
    public float baseIncomePerSecond = 1f;
    public float focusMultiplier = 2f;
    private float accumulator = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (ResourceSystem.Instance == null) return;

        float rate = GetCurrentRate();
        accumulator += rate * Time.deltaTime;
        if (accumulator >= 1f)
        {
            float points = Mathf.Floor(accumulator);
            accumulator -= points;
            ResourceSystem.Instance.AddFocusPoints(points);
        }
    }

    public float GetCurrentRate()
    {
        float rate = baseIncomePerSecond;
        if (FocusSystem.Instance != null && FocusSystem.Instance.IsRunning
            && !FocusSystem.Instance.IsResting)
            rate *= focusMultiplier;
        return rate;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
