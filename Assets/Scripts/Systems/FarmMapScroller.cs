using UnityEngine;
using UnityEngine.InputSystem;

/// Right-click drag to pan the farm camera left/right.
/// 放置模式下右键用来取消放置，不拖动地图。
public class FarmMapScroller : MonoBehaviour
{
    public static FarmMapScroller Instance { get; private set; }

    [Header("Scroll Settings")]
    public float scrollSpeed   = 8f;
    public float mapMinX       = -20f;
    public float mapMaxX       =  20f;
    public float snapSmoothing =  8f;

    [Header("Grass Background")]
    public Texture2D grassTexture;  // 留空 = 不创建背景（用Tilemap替代）
    public float bgHeight = 8f;
    public float bgWidth  = 200f;
    public float bgY      = 0f;
    public float bgZ      = 0.5f;
    public float tilingX  = 40f;

    private Camera         mainCam;
    private float          targetX;
    private bool           isDragging;
    private Vector2        dragStartScreen;
    private float          dragStartCamX;
    private GameObject     _bgGO;
    private SpriteRenderer _bgSR;
    private Material       _bgMat;

    public bool CanScrollLeft  { get; private set; }
    public bool CanScrollRight { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;
        targetX = mainCam != null ? mainCam.transform.position.x : 0f;

        // 只有设置了贴图才创建背景（现在用Tilemap，所以留空）
        if (grassTexture != null) BuildGrassBackground();
    }

    void BuildGrassBackground()
    {
        var old = GameObject.Find("__GrassBG__");
        if (old != null) Destroy(old);
        _bgGO = new GameObject("__GrassBG__");
        _bgSR = _bgGO.AddComponent<SpriteRenderer>();
        _bgSR.sortingOrder = -10;
        var spr = Sprite.Create(grassTexture,
            new Rect(0, 0, grassTexture.width, grassTexture.height),
            new Vector2(0.5f, 0.5f), 100f);
        _bgSR.sprite = spr;
        _bgMat = new Material(Shader.Find("Sprites/Default"));
        _bgMat.mainTexture      = grassTexture;
        _bgMat.mainTextureScale = new Vector2(tilingX, 1f);
        _bgSR.material = _bgMat;
        float ppu    = 100f;
        float scaleX = bgWidth  / (grassTexture.width  / ppu);
        float scaleY = bgHeight / (grassTexture.height / ppu);
        _bgGO.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    void Update()
    {
        if (mainCam == null) return;

        // 放置模式下右键不拖动（由PlacementManager处理右键取消）
        bool inPlacement = PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing;
        if (!inPlacement) HandleDrag();

        HandleKeyboard();

        float cur  = mainCam.transform.position.x;
        float newX = Mathf.Lerp(cur, targetX, Time.deltaTime * snapSmoothing);
        mainCam.transform.position = new Vector3(
            newX, mainCam.transform.position.y, mainCam.transform.position.z);

        CanScrollLeft  = mainCam.transform.position.x > mapMinX + 0.05f;
        CanScrollRight = mainCam.transform.position.x < mapMaxX - 0.05f;

        if (_bgGO != null)
            _bgGO.transform.position = new Vector3(mainCam.transform.position.x, bgY, bgZ);
    }

    void HandleDrag()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        Vector2 mousePos = mouse.position.ReadValue();
        if (mouse.rightButton.wasPressedThisFrame)
        {
            isDragging      = true;
            dragStartScreen = mousePos;
            dragStartCamX   = mainCam.transform.position.x;
        }
        if (mouse.rightButton.wasReleasedThisFrame) isDragging = false;
        if (isDragging)
        {
            float dx      = mousePos.x - dragStartScreen.x;
            float worldDx = (dx / Screen.width) * mainCam.orthographicSize * 2f
                            * ((float)Screen.width / Screen.height);
            targetX = Mathf.Clamp(dragStartCamX - worldDx, mapMinX, mapMaxX);
        }
    }

    void HandleKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)
            targetX = Mathf.Max(targetX - scrollSpeed * Time.deltaTime, mapMinX);
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed)
            targetX = Mathf.Min(targetX + scrollSpeed * Time.deltaTime, mapMaxX);
    }

    public void ScrollTo(float worldX) =>
        targetX = Mathf.Clamp(worldX, mapMinX, mapMaxX);
}
