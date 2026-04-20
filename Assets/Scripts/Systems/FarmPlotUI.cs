using UnityEngine;
using TMPro;

public class FarmPlotUI : MonoBehaviour
{
    public enum VisualState { Empty, Growing, Ready }

    public TextMeshPro label;
    private Transform  barFill;

    void Awake()
    {
        var bg = transform.Find("ProgressBarBG");
        if (bg != null) barFill = bg.Find("ProgressBarFill");
    }

    public void SetState(VisualState state)
    {
        if (state == VisualState.Empty)   SetFillScale(0f);
        if (state == VisualState.Ready)   SetFillScale(1f);
    }

    public void SetProgress(float t)
    {
        SetFillScale(Mathf.Clamp01(t));
    }

    void SetFillScale(float t)
    {
        if (barFill == null) return;
        var s = barFill.localScale;
        s.x = Mathf.Max(t, 0.001f);
        barFill.localScale = s;
        var p = barFill.localPosition;
        p.x = -0.5f + t * 0.5f;
        barFill.localPosition = p;
    }
}
