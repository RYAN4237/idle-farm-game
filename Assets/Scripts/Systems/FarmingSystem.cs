using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FarmingSystem : MonoBehaviour
{
    public static FarmingSystem Instance { get; private set; }

    private static readonly List<FarmPlot> plots = new List<FarmPlot>();
    private float pulseTimer;
    private bool subscribedToFocus;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        TrySubscribeToFocus();
    }

    private void TrySubscribeToFocus()
    {
        if (subscribedToFocus || FocusSystem.Instance == null) return;

        FocusSystem.Instance.OnRunningChanged += OnFocusRunningChanged;
        FocusSystem.Instance.OnFocusCompleted += OnFocusSessionCompleted;
        subscribedToFocus = true;
    }

    void OnDestroy()
    {
        if (subscribedToFocus && FocusSystem.Instance != null)
        {
            FocusSystem.Instance.OnRunningChanged -= OnFocusRunningChanged;
            FocusSystem.Instance.OnFocusCompleted -= OnFocusSessionCompleted;
        }

        // 单例销毁时清理静态列表，防止场景重载时残留
        if (Instance == this)
        {
            plots.Clear();
            Instance = null;
        }
    }

    // ── Focus bonus callbacks ──────────────────────────────
    void OnFocusRunningChanged(bool isRunning)
    {
        // 只在专注阶段（非休息）才给加速，休息或暂停时恢复正常
        bool isFocusing = isRunning
                          && FocusSystem.Instance != null
                          && !FocusSystem.Instance.IsResting;
        FarmPlot.GrowthSpeedMult = isFocusing ? 2f : 1f;
        Debug.Log($"FarmingSystem: GrowthSpeedMult = {FarmPlot.GrowthSpeedMult}x");
    }

    void OnFocusSessionCompleted()
    {
        // Instantly ripen ALL growing plots as session reward
        int ripened = 0;
        foreach (var plot in plots)
        {
            if (plot != null && plot.State == FarmPlot.PlotState.Growing)
            {
                plot.ForceRipen();
                ripened++;
            }
        }
        if (ripened > 0)
            Debug.Log($"FarmingSystem: Session complete! {ripened} plots force-ripened.");
    }

    // ── Pulse animation ───────────────────────────────────
    void Update()
    {
        // 延迟订阅：Start 时 FocusSystem 可能还没初始化
        if (!subscribedToFocus)
            TrySubscribeToFocus();

        pulseTimer += Time.deltaTime * 1.2f;
        if (pulseTimer > 1f) pulseTimer -= 1f;

        foreach (var plot in plots)
            if (plot != null) plot.PulseTick(pulseTimer);

#if UNITY_EDITOR
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.pKey.wasPressedThisFrame) DebugPlantAll();
        if (kb.rKey.wasPressedThisFrame) DebugReadyAll();
        if (kb.hKey.wasPressedThisFrame) DebugHarvestAll();
        if (kb.fKey.wasPressedThisFrame)
        {
            ResourceSystem.Instance?.AddFocusPoints(50f);
            Debug.Log("DEBUG: +50 FP");
        }
#endif
    }

    public static void Register(FarmPlot plot)
    {
        if (!plots.Contains(plot)) plots.Add(plot);
    }

    public static void Unregister(FarmPlot plot)
    {
        plots.Remove(plot);
    }

#if UNITY_EDITOR
    void DebugPlantAll()
    {
        foreach (var p in plots)
            if (p != null && p.State == FarmPlot.PlotState.Empty) p.Plant();
    }
    void DebugReadyAll()
    {
        foreach (var p in plots)
            if (p != null && p.State == FarmPlot.PlotState.Growing)
                p.SetState(FarmPlot.PlotState.Ready);
    }
    void DebugHarvestAll()
    {
        foreach (var p in plots)
            if (p != null && p.State == FarmPlot.PlotState.Ready) p.Harvest();
    }
#endif
}
