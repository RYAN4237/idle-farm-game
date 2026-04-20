using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Listens to GameEventBus and applies growth boosts to the farm.
/// Also handles visual feedback (sparkles, speed indicator).
public class FarmBoostReceiver : MonoBehaviour
{
    public static FarmBoostReceiver Instance { get; private set; }

    [Header("Current State (read-only)")]
    [SerializeField] float _currentMultiplier = 1f;
    [SerializeField] float _boostTimeRemaining;

    public float GrowthMultiplier => _currentMultiplier;

    Coroutine _boostCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void OnEnable()  => GameEventBus.OnBoostRequested += ApplyBoost;
    void OnDisable() => GameEventBus.OnBoostRequested -= ApplyBoost;

    void ApplyBoost(float multiplier, float duration)
    {
        // Stack or refresh: take the higher multiplier, add durations
        float newMult = Mathf.Max(_currentMultiplier, multiplier);
        float newDur  = _boostTimeRemaining + duration;

        if (_boostCoroutine != null) StopCoroutine(_boostCoroutine);
        _boostCoroutine = StartCoroutine(BoostRoutine(newMult, newDur));

        // Visual: flash topbar or spawn particles
        ShowBoostEffect(multiplier);
        Debug.Log($"[FarmBoost] x{newMult} for {newDur}s");
    }

    IEnumerator BoostRoutine(float mult, float dur)
    {
        _currentMultiplier   = mult;
        _boostTimeRemaining  = dur;

        while (_boostTimeRemaining > 0)
        {
            _boostTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        _currentMultiplier  = 1f;
        _boostTimeRemaining = 0f;
        _boostCoroutine     = null;
        Debug.Log("[FarmBoost] Boost expired, back to x1");
    }

    void ShowBoostEffect(float mult)
    {
        // Find TopBar and flash it gold briefly
        var topBar = GameObject.Find("UICanvas/TopBar");
        if (topBar != null) StartCoroutine(FlashTopBar(topBar, mult));
    }

    IEnumerator FlashTopBar(GameObject topBar, float mult)
    {
        var img = topBar.GetComponent<Image>();
        if (img == null) yield break;
        Color original = img.color;
        Color flash    = new Color(0.9f, 0.75f, 0.1f, 1f); // gold

        for (int i = 0; i < 3; i++)
        {
            img.color = flash;
            yield return new WaitForSeconds(0.15f);
            img.color = original;
            yield return new WaitForSeconds(0.15f);
        }
    }
}
