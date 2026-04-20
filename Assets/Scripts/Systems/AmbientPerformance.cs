using UnityEngine;

/// Keeps the game at low FPS when not interacted with.
/// Essential for a desktop ambient app — should never spike CPU.
[DefaultExecutionOrder(-200)]
public class AmbientPerformance : MonoBehaviour
{
    [Header("FPS Targets")]
    public int idleFPS      = 15;  // background, no interaction
    public int activeFPS    = 30;  // user hovering/clicking
    public int boostFPS     = 30;  // during focus session (farm animating)

    [Header("Idle Timeout")]
    public float idleAfterSeconds = 3f;

    float _lastInteractionTime;
    bool  _isActive;
    int   _currentTarget;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = idleFPS;
        QualitySettings.vSyncCount  = 0;
        _currentTarget = idleFPS;
    }

    void OnEnable()
    {
        GameEventBus.OnFocusStart  += OnFocusStart;
        GameEventBus.OnFocusPause  += OnFocusPause;
    }

    void OnDisable()
    {
        GameEventBus.OnFocusStart  -= OnFocusStart;
        GameEventBus.OnFocusPause  -= OnFocusPause;
    }

    void OnFocusStart() => SetTarget(boostFPS);
    void OnFocusPause() => SetTarget(idleFPS);

    void Update()
    {
        // Check for any mouse/keyboard input
        if (Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            _lastInteractionTime = Time.realtimeSinceStartup;
            if (!_isActive) { _isActive = true; SetTarget(activeFPS); }
        }
        else if (_isActive && Time.realtimeSinceStartup - _lastInteractionTime > idleAfterSeconds)
        {
            _isActive = false;
            SetTarget(idleFPS);
        }
    }

    void SetTarget(int fps)
    {
        if (_currentTarget == fps) return;
        _currentTarget = fps;
        Application.targetFrameRate = fps;
    }
}
