using UnityEngine;

/// Drives the Sprout Lands plant sprite on a FarmPlot.
/// Uses Basic Plants.png from Resources folder.
/// Naming: "Basic Plants_{index}" where index = row*cols + col
/// Row 0 (top in Unity, index 0-5): seed through ripe
[RequireComponent(typeof(FarmPlot))]
public class FarmPlotVisual : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer plantRenderer;

    // Basic Plants.png: 96x32, 16px tiles = 6 cols x 2 rows
    // Resources copy uses {baseName}_{rowIndex*cols + colIndex} naming
    // Row 0 (indices 0-5): seed→sprout→small→mid→tall→ripe
    // Row 1 (indices 6-11): alternate crop set
    static readonly int[] GrowIndices = { 0, 1, 2, 3, 4 };
    const int ReadyIndex = 5;

    static Sprite[]  _growSprites;
    static Sprite    _readySprite;
    static bool      _loaded;

    FarmPlot _plot;
    float    _lastProgress = -1f;

    void Awake()
    {
        _plot = GetComponent<FarmPlot>();
        if (!_loaded) LoadSprites();
    }

    void Start() => Refresh(0f);

    void Update()
    {
        if (_plot == null || plantRenderer == null) return;

        float progress = 0f;
        if (_plot.State == FarmPlot.PlotState.Growing)
        {
            float rem = _plot.GetGrowTimerRemaining();
            progress  = 1f - Mathf.Clamp01(rem / Mathf.Max(_plot.growthDuration, 0.01f));
        }
        else if (_plot.State == FarmPlot.PlotState.Ready)
        {
            progress = 1f;
        }

        if (Mathf.Abs(progress - _lastProgress) > 0.009f)
        {
            _lastProgress = progress;
            Refresh(progress);
        }
    }

    void Refresh(float progress)
    {
        if (plantRenderer == null) return;

        if (_plot.State == FarmPlot.PlotState.Empty)
        {
            plantRenderer.color = Color.clear;
            return;
        }

        if (_plot.State == FarmPlot.PlotState.Ready)
        {
            plantRenderer.color  = Color.white;
            plantRenderer.sprite = _readySprite;
            return;
        }

        int frameIdx = Mathf.Clamp(
            Mathf.FloorToInt(progress * GrowIndices.Length),
            0, GrowIndices.Length - 1);

        plantRenderer.color  = Color.white;
        if (_growSprites != null && frameIdx < _growSprites.Length)
            plantRenderer.sprite = _growSprites[frameIdx];
    }

    static void LoadSprites()
    {
        _loaded = true;
        var all = Resources.LoadAll<Sprite>("Basic Plants");

        if (all == null || all.Length == 0)
        {
            Debug.LogWarning("[FarmPlotVisual] 'Basic Plants' not found in Resources. " +
                             "Run Farm > Setup Sprout Lands Resources from the menu.");
            _growSprites = new Sprite[0];
            return;
        }

        // Build index lookup: "Basic Plants_{i}" → sprite
        var byIndex = new System.Collections.Generic.Dictionary<int, Sprite>();
        foreach (var s in all)
        {
            // Expected format: "Basic Plants_{N}"
            var parts = s.name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[parts.Length - 1], out int idx))
                byIndex[idx] = s;
        }

        var growList = new System.Collections.Generic.List<Sprite>();
        foreach (int i in GrowIndices)
            if (byIndex.TryGetValue(i, out var s)) growList.Add(s);
        _growSprites = growList.ToArray();

        byIndex.TryGetValue(ReadyIndex, out _readySprite);

        Debug.Log($"[FarmPlotVisual] Loaded {growList.Count} grow frames, ready={_readySprite != null}");
    }
}
