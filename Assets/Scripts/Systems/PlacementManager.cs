using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    public bool IsPlacing { get; private set; }

    private GameObject    _ghost;
    private SpriteRenderer _ghostSR;
    private Camera        _cam;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _cam = Camera.main;
        Debug.Log("[PlacementManager] Awake - Instance set");
    }

    public void StartPlacing()
    {
        IsPlacing = true;
        CreateGhost();
        Debug.Log("[PlacementManager] Placement ON - left click grid to place, right click to cancel");
    }

    public void StopPlacing()
    {
        IsPlacing = false;
        DestroyGhost();
        if (FarmGrid.Instance != null)
            FarmGrid.Instance.HoverCell = new Vector2Int(-1, -1);
        Debug.Log("[PlacementManager] Placement OFF");
    }

    void CreateGhost()
    {
        DestroyGhost();
        _ghost   = new GameObject("PlotGhost");
        _ghostSR = _ghost.AddComponent<SpriteRenderer>();
        _ghostSR.sortingOrder = 10;

        float cs = FarmGrid.Instance != null ? FarmGrid.Instance.cellSize : 1.28f;

        // 生成圆角方形sprite
        var tex = MakeRoundedTex(64, 0.12f, Color.white);
        _ghostSR.sprite = Sprite.Create(tex, new Rect(0,0,64,64), new Vector2(0.5f,0.5f), 64f);
        _ghostSR.color  = new Color(0.4f, 0.9f, 0.3f, 0.5f);
        _ghost.transform.localScale = new Vector3(cs, cs, 1f);
    }

    void DestroyGhost()
    {
        if (_ghost != null) { Destroy(_ghost); _ghost = null; }
    }

    void Update()
    {
        if (!IsPlacing) return;

        var mouse = Mouse.current;
        if (mouse == null) return;
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        var grid = FarmGrid.Instance;
        if (grid == null) return;

        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 world = _cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(_cam.transform.position.z)));
        world.z = 0f;

        Vector2Int cell = grid.WorldToCell(world);
        grid.HoverCell = cell;

        // 更新ghost
        if (_ghost != null)
        {
            bool valid = grid.IsValid(cell);
            _ghost.SetActive(valid);
            if (valid)
            {
                _ghost.transform.position = grid.CellToWorld(cell);
                _ghostSR.color = grid.IsOccupied(cell)
                    ? new Color(1f, 0.3f, 0.3f, 0.5f)
                    : new Color(0.4f, 0.9f, 0.3f, 0.5f);
            }
        }

        // 左键放置
        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (grid.IsValid(cell) && !grid.IsOccupied(cell))
            {
                PlotFactory.Create(grid.CellToWorld(cell), grid.cellSize);
                grid.SetOccupied(cell, true);
                Debug.Log($"[PlacementManager] Placed at cell {cell}");
            }
        }

        // 右键取消
        if (mouse.rightButton.wasPressedThisFrame)
            StopPlacing();
    }

    static Texture2D MakeRoundedTex(int size, float cornerRatio, Color col)
    {
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        float r  = size * cornerRatio;
        float cx = size * 0.5f, cy = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx   = Mathf.Max(0, Mathf.Abs(x - cx) - (cx - r));
            float dy   = Mathf.Max(0, Mathf.Abs(y - cy) - (cy - r));
            float dist = Mathf.Sqrt(dx*dx + dy*dy);
            float a    = Mathf.Clamp01(1f - (dist - r + 1f));
            tex.SetPixel(x, y, new Color(col.r, col.g, col.b, a));
        }
        tex.Apply();
        return tex;
    }
}
