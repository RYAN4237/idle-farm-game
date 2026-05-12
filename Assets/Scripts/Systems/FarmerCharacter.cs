using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Rusty Retirement-style farmer character that walks between plots,
/// harvests ready ones, and idles when there's nothing to do.
/// Uses Sprout Lands "Basic Charakter Spritesheet" sprites.
[RequireComponent(typeof(SpriteRenderer))]
public class FarmerCharacter : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed    = 2.5f;
    public float harvestPause = 0.6f;
    public float idleWander   = 3f;

    [Header("Sprite Frames (assigned by SetupSproutLandsResources editor script)")]
    public Sprite[] walkDownFrames;   // 4 frames facing down
    public Sprite[] walkUpFrames;     // 4 frames facing up
    public Sprite[] walkLeftFrames;   // 4 frames facing left
    public Sprite[] walkRightFrames;  // 4 frames facing right

    SpriteRenderer _sr;
    Sprite[]       _currentAnim;
    int            _animFrame;
    float          _animTimer;
    const float    AnimFps = 8f;

    enum CharState { Idle, Walking, Harvesting }
    CharState  _state = CharState.Idle;
    FarmPlot   _targetPlot;
    float      _idleTimer;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = 10;

        // Runtime fallback: try loading from Resources if frames not injected
        if (walkDownFrames == null || walkDownFrames.Length == 0)
            TryLoadFromResources();
    }

    void Start()
    {
        if (FarmGrid.Instance != null)
        {
            var g = FarmGrid.Instance;
            transform.position   = new Vector3(g.originX + g.cellSize * 0.5f,
                                               g.originY + g.cellSize * 0.5f, -0.05f);
            transform.localScale = Vector3.one * g.cellSize * 0.9f;
        }
        SetAnim(walkDownFrames);
        StartCoroutine(FarmLoop());
    }

    void Update()
    {
        _animTimer += Time.deltaTime;
        if (_animTimer >= 1f / AnimFps)
        {
            _animTimer = 0f;
            if (_currentAnim != null && _currentAnim.Length > 0)
            {
                _animFrame = (_animFrame + 1) % _currentAnim.Length;
                if (_currentAnim[_animFrame] != null)
                    _sr.sprite = _currentAnim[_animFrame];
            }
        }
    }

    IEnumerator FarmLoop()
    {
        while (true)
        {
            _targetPlot = FindReadyPlot();

            if (_targetPlot != null)
            {
                _idleTimer = 0f;
                yield return WalkTo(_targetPlot.transform.position);

                _state = CharState.Harvesting;
                SetAnim(walkDownFrames);
                yield return new WaitForSeconds(harvestPause);

                if (_targetPlot != null && _targetPlot.State == FarmPlot.PlotState.Ready)
                    _targetPlot.Harvest();

                _state = CharState.Idle;
            }
            else
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= idleWander)
                {
                    _idleTimer = 0f;
                    yield return WalkTo(RandomGridPoint());
                }
                else
                {
                    SetAnim(walkDownFrames);
                    _animFrame = 0;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }

    IEnumerator WalkTo(Vector3 dest)
    {
        dest.z = transform.position.z;
        _state = CharState.Walking;

        while (Vector3.Distance(transform.position, dest) > 0.05f)
        {
            Vector3 dir = (dest - transform.position).normalized;
            float ax = Mathf.Abs(dir.x), ay = Mathf.Abs(dir.y);
            if (ax >= ay) SetAnim(dir.x > 0 ? walkRightFrames : walkLeftFrames);
            else          SetAnim(dir.y > 0 ? walkUpFrames    : walkDownFrames);

            transform.position = Vector3.MoveTowards(transform.position, dest,
                walkSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = dest;
        _state = CharState.Idle;
    }

    void SetAnim(Sprite[] frames)
    {
        if (frames == _currentAnim || frames == null) return;
        _currentAnim = frames;
        _animFrame   = 0;
    }

    FarmPlot FindReadyPlot()
    {
        var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
        FarmPlot best = null;
        float    bestDist = float.MaxValue;
        foreach (var p in plots)
        {
            if (p.State != FarmPlot.PlotState.Ready) continue;
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < bestDist) { bestDist = d; best = p; }
        }
        return best;
    }

    Vector3 RandomGridPoint()
    {
        if (FarmGrid.Instance == null) return transform.position;
        var g = FarmGrid.Instance;
        return g.CellToWorld(new Vector2Int(Random.Range(0, g.gridWidth),
                                            Random.Range(0, g.gridHeight)));
    }

    void TryLoadFromResources()
    {
        // Resources copy uses uniform 48x48 grid slice naming: _0 to _15
        // Row0(0-3)=down, Row1(4-7)=up, Row2(8-11)=left, Row3(12-15)=right
        var all = Resources.LoadAll<Sprite>("Basic Charakter Spritesheet");
        if (all == null || all.Length == 0) return;

        walkDownFrames  = GetRange(all, 0, 4);
        walkUpFrames    = GetRange(all, 4, 4);
        walkLeftFrames  = GetRange(all, 8, 4);
        walkRightFrames = GetRange(all, 12, 4);
    }

    static Sprite[] GetRange(Sprite[] all, int start, int count)
    {
        var result = new List<Sprite>();
        for (int i = start; i < start + count && i < all.Length; i++)
            result.Add(all[i]);
        return result.ToArray();
    }
}
