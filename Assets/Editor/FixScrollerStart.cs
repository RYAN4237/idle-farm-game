using UnityEngine;
using UnityEditor;

public class FixScrollerStart
{
    public static void Execute()
    {
        var gm = GameObject.Find("GameManager");
        var scroller = gm?.GetComponent<FarmMapScroller>();
        if (scroller != null)
        {
            // Camera starts at x=-1.5, can scroll from -2.5 to +3.5
            // Left: sees col at x=-5,-3,-1
            // Right: sees col at x=-1,+1,+3,+5
            scroller.mapMinX       = -2.5f;
            scroller.mapMaxX       =  3.5f;
            scroller.scrollSpeed   = 10f;
            scroller.snapSmoothing = 8f;
            EditorUtility.SetDirty(gm);
        }

        // Also update scroll arrows to use larger step
        // (arrows already wired in scene, just update bounds)

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixScrollerStart done + saved!");
    }
}
