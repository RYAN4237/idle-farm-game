using UnityEngine;
using UnityEditor;

public class TestScroll
{
    public static void ScrollRight()
    {
        // Simulate scrolling right: move camera to x=+1.8 to see rightmost 3 cols
        var scroller = FarmMapScroller.Instance;
        if (scroller == null) { Debug.LogError("FarmMapScroller not found — game running?"); return; }
        scroller.ScrollTo(1.8f);
        Debug.Log($"Scrolled to x=1.8. CanLeft={scroller.CanScrollLeft}, CanRight={scroller.CanScrollRight}");
    }

    public static void ScrollLeft()
    {
        var scroller = FarmMapScroller.Instance;
        if (scroller == null) return;
        scroller.ScrollTo(-1.5f);
        Debug.Log("Scrolled back to x=-1.5");
    }
}
