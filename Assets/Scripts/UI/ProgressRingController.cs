using UnityEngine;
using UnityEngine.UI;

public class ProgressRingController : MonoBehaviour
{
    public Image progressRing;

    [Header("Colors")]
    public Color idleColor      = new Color(0.20f, 0.85f, 0.70f, 0.95f); // teal
    public Color focusingColor  = new Color(0.20f, 0.85f, 0.70f, 0.95f); // teal
    public Color restColor      = new Color(0.22f, 0.54f, 0.87f, 1.00f); // blue
    public Color completedColor = new Color(0.40f, 1.00f, 0.50f, 1.00f); // green

    void OnEnable()
    {
        if (FocusSystem.Instance != null)
        {
            FocusSystem.Instance.OnTimerTick      += OnTick;
            FocusSystem.Instance.OnRunningChanged += OnRunningChanged;
            FocusSystem.Instance.OnFocusCompleted += OnFocusCompleted;
            FocusSystem.Instance.OnPhaseChanged   += OnPhaseChanged;
            FocusSystem.Instance.OnRestCompleted  += OnRestCompleted;
        }
        if (progressRing != null)
        {
            progressRing.fillAmount = 1f;
            progressRing.color = idleColor;
        }
    }

    void OnTick(float remaining, float total)
    {
        if (progressRing == null || total <= 0f) return;
        progressRing.fillAmount = Mathf.Max(remaining / total, 0f);
    }

    void OnRunningChanged(bool running)
    {
        if (progressRing == null) return;
        bool isRest = FocusSystem.Instance != null && FocusSystem.Instance.IsResting;
        progressRing.color = isRest ? restColor : (running ? focusingColor : idleColor);
    }

    void OnPhaseChanged(bool isRest)
    {
        if (progressRing == null) return;
        progressRing.color = isRest ? restColor : idleColor;
        progressRing.fillAmount = 1f;
    }

    void OnFocusCompleted()
    {
        // 短暂完成色，切换到休息后 OnPhaseChanged 会接管
        if (progressRing != null)
            progressRing.color = completedColor;
    }

    void OnRestCompleted()
    {
        if (progressRing != null)
        {
            progressRing.color = idleColor;
            progressRing.fillAmount = 1f;
        }
    }

    void OnDisable()
    {
        if (FocusSystem.Instance != null)
        {
            FocusSystem.Instance.OnTimerTick      -= OnTick;
            FocusSystem.Instance.OnRunningChanged -= OnRunningChanged;
            FocusSystem.Instance.OnFocusCompleted -= OnFocusCompleted;
            FocusSystem.Instance.OnPhaseChanged   -= OnPhaseChanged;
            FocusSystem.Instance.OnRestCompleted  -= OnRestCompleted;
        }
    }
}
