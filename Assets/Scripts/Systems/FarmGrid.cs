using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]  // 确保最早执行
public class FarmGrid : MonoBehaviour
{
    public static FarmGrid Instance { get; private set; }

    [Header("Grid Settings")]
    public float cellSize   = 1.28f;
    public int   gridWidth  = 30;
    public int   gridHeight = 4;
    public float originX    = -19.2f;
    public float originY    = -2.56f;

    [Header("Visual")]
    public Color  gridColor      = new Color(0f, 0f, 0f, 0.3f);
    public Color  highlightColor = new Color(1f, 0.9f, 0.1f, 0.7f);
    public Color  occupiedColor  = new Color(1f, 0.2f, 0.2f, 0.5f);
    public float  lineWidth      = 0.04f;

    private bool[,]            occupied;
    public  Vector2Int         HoverCell { get; set; } = new Vector2Int(-1, -1);

    private List<LineRenderer> _lines     = new List<LineRenderer>();
    private GameObject         _highlightQuad;
    private SpriteRenderer     _highlightSR;
    private GameObject         _gridRoot;

    void Awake()
    {
        // 单例
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        occupied = new bool[gridWidth, gridHeight];
        Debug.Log($"[FarmGrid] Awake - Instance set, cellSize={cellSize}");
    }

    void Start()
    {
        BuildGridLines();
        BuildHighlight();
    }

    void BuildGridLines()
    {
        // 清除旧的
        if (_gridRoot != null) Destroy(_gridRoot);
        _gridRoot = new GameObject("GridLines");

        for (int x = 0; x <= gridWidth; x++)
        {
            float wx = originX + x * cellSize;
            CreateLine(new Vector3(wx, originY, 0),
                       new Vector3(wx, originY + gridHeight * cellSize, 0));
        }
        for (int y = 0; y <= gridHeight; y++)
        {
            float wy = originY + y * cellSize;
            CreateLine(new Vector3(originX, wy, 0),
                       new Vector3(originX + gridWidth * cellSize, wy, 0));
        }
    }

    void CreateLine(Vector3 a, Vector3 b)
    {
        var go = new GameObject("Line");
        go.transform.SetParent(_gridRoot.transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startWidth = lineWidth;
        lr.endWidth   = lineWidth;
        lr.sortingOrder = 5;
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = gridColor;
        lr.material    = mat;
        lr.startColor  = gridColor;
        lr.endColor    = gridColor;
        _lines.Add(lr);
    }

    void BuildHighlight()
    {
        if (_highlightQuad != null) Destroy(_highlightQuad);
        _highlightQuad = new GameObject("GridHighlight");
        _highlightSR   = _highlightQuad.AddComponent<SpriteRenderer>();
        _highlightSR.sortingOrder = 4;

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _highlightSR.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
        _highlightSR.color  = Color.clear;
        _highlightQuad.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);
    }

    void Update()
    {
        if (_highlightSR == null) return;
        if (HoverCell.x >= 0 && IsValid(HoverCell))
        {
            _highlightSR.color = IsOccupied(HoverCell) ? occupiedColor : highlightColor;
            _highlightQuad.transform.position = CellToWorld(HoverCell);
        }
        else
        {
            _highlightSR.color = Color.clear;
        }
    }

    // ── 公开 API ─────────────────────────────────────────
    public Vector2Int WorldToCell(Vector3 world)
    {
        int cx = Mathf.FloorToInt((world.x - originX) / cellSize);
        int cy = Mathf.FloorToInt((world.y - originY) / cellSize);
        return new Vector2Int(cx, cy);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            originX + cell.x * cellSize + cellSize * 0.5f,
            originY + cell.y * cellSize + cellSize * 0.5f,
            0f);
    }

    public bool IsValid(Vector2Int cell)
        => cell.x >= 0 && cell.x < gridWidth &&
           cell.y >= 0 && cell.y < gridHeight;

    public bool IsOccupied(Vector2Int cell)
        => IsValid(cell) && occupied[cell.x, cell.y];

    public void SetOccupied(Vector2Int cell, bool val)
    {
        if (IsValid(cell)) occupied[cell.x, cell.y] = val;
    }
}
