using UnityEngine;

/// Core farm plot logic - grows over time, harvests on click.
/// Responds to FarmBoostReceiver for speed multipliers.
/// Full API compatibility with AutoFarmer, CropShop, FarmingSystem, SaveSystem.
public class FarmPlot : MonoBehaviour
{
    // Nested enum so other files access as FarmPlot.PlotState
    public enum PlotState { Empty, Growing, Ready }

    [Header("Settings")]
    public float growthDuration  = 30f;
    public float plantCost       = 10f;
    public float harvestReward   = 25f;
    public bool  isLocked        = false;

    [Header("Colors")]
    public Color emptyColor   = new Color(0.52f, 0.34f, 0.14f);
    public Color growingColor = new Color(0.25f, 0.55f, 0.20f);
    public Color readyColor   = new Color(0.15f, 0.85f, 0.25f);

    // Public state
    public PlotState State { get; private set; } = PlotState.Growing;

    // Global growth multiplier (set by FarmingSystem)
    public static float GrowthSpeedMult { get; set; } = 1f;

    float  _progress;
    string _activeCrop = "Wheat";
    float  _growTimerRemaining;

    SpriteRenderer _sr;
    Transform      _barFill;

    void Awake()
    {
        _sr      = GetComponent<SpriteRenderer>();
        _barFill = transform.Find("ProgressBarBG/ProgressBarFill");
        _growTimerRemaining = growthDuration;
        GameEventBus.OnBoostRequested += OnBoostApplied;
    }

    void OnDestroy() => GameEventBus.OnBoostRequested -= OnBoostApplied;

    void OnBoostApplied(float mult, float dur)
    {
        if (State == PlotState.Growing) StartCoroutine(PulseGreen());
    }

    System.Collections.IEnumerator PulseGreen()
    {
        if (_sr == null) yield break;
        Color orig = _sr.color;
        for (int i = 0; i < 2; i++)
        {
            _sr.color = Color.green;
            yield return new WaitForSeconds(0.12f);
            _sr.color = orig;
            yield return new WaitForSeconds(0.12f);
        }
    }

    void Update()
    {
        if (State != PlotState.Growing) return;

        float mult = GrowthSpeedMult;
        if (FarmBoostReceiver.Instance != null)
            mult *= FarmBoostReceiver.Instance.GrowthMultiplier;

        _progress           += (Time.deltaTime / growthDuration) * mult;
        _growTimerRemaining  = Mathf.Max(0, growthDuration * (1f - _progress));

        if (_barFill != null)
            _barFill.localScale = new Vector3(Mathf.Clamp01(_progress), 1, 1);
        if (_sr != null)
            _sr.color = Color.Lerp(growingColor, readyColor, Mathf.Clamp01(_progress));

        if (_progress >= 1f)
        {
            SetState(PlotState.Ready);
            if (_sr) _sr.color = readyColor;
            StartCoroutine(BobLoop());
        }
    }

    System.Collections.IEnumerator BobLoop()
    {
        Vector3 orig = transform.localScale;
        while (State == PlotState.Ready)
        {
            float t = (Mathf.Sin(Time.time * 2.5f) + 1f) * 0.5f;
            transform.localScale = Vector3.Lerp(orig, orig * 1.06f, t);
            yield return null;
        }
        transform.localScale = orig;
    }

    void OnMouseDown()
    {
        if (State == PlotState.Ready) Harvest();
    }

    // ── Public API ────────────────────────────────────────────────────
    public void Plant(string cropName = "Wheat")
    {
        _activeCrop = cropName; _progress = 0f;
        _growTimerRemaining = growthDuration;
        SetState(PlotState.Growing);
        if (_sr) _sr.color = growingColor;
    }

    public void Harvest()
    {
        if (State != PlotState.Ready) return;
        if (ResourceSystem.Instance != null)
            ResourceSystem.Instance.AddFocusPoints(harvestReward);
        StartCoroutine(HarvestFlash());
        if (HarvestFX.Instance != null)
            HarvestFX.Instance.PlayAt(transform);
        Plant(_activeCrop);
        Debug.Log($"[FarmPlot] Harvested {_activeCrop}! +{harvestReward} FP");
    }

    public void ForceRipen()
    {
        _progress = 1f; SetState(PlotState.Ready);
        if (_sr) _sr.color = readyColor;
    }

    // Overloads to match FarmingSystem signature
    public void PulseTick() => StartCoroutine(PulseGreen());
    public void PulseTick(float dummy) => PulseTick();

    public void SetState(PlotState s) => State = s;

    public string GetActiveCrop() => _activeCrop;
    public float  GetGrowTimerRemaining() => _growTimerRemaining;

    public void RestoreGrowingState(string crop, float timerRemaining)
    {
        _activeCrop = crop; _growTimerRemaining = timerRemaining;
        _progress   = 1f - (timerRemaining / Mathf.Max(growthDuration, 0.01f));
        SetState(PlotState.Growing);
        if (_sr) _sr.color = Color.Lerp(growingColor, readyColor, Mathf.Clamp01(_progress));
    }

    System.Collections.IEnumerator HarvestFlash()
    {
        if (_sr == null) yield break;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        _sr.color = growingColor;
    }
}
