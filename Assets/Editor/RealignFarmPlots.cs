using UnityEngine;
using UnityEditor;
using TMPro;

public class RealignFarmPlots
{
    public static void Execute()
    {
        // Camera x=-1.5, ortho=2.8, 16:9 → half-width=4.978
        // Visible x: -1.5-4.978=-6.478 to -1.5+4.978=+3.478
        // FP panel = left 10% of screen = world x up to: -6.478 + 9.956*0.10 = -5.482
        // First column starts at -4.8 (safely after FP panel)
        // 6 columns spaced 2.0 apart: -4.8, -2.8, -0.8, +1.2, +3.2, +5.2

        float startX = -4.8f;
        float step   =  2.0f;
        float scale  =  8f;
        float ry0    =  0.85f;
        float ry1    = -0.85f;

        for (int c = 0; c < 6; c++)
        {
            float x = startX + c * step;

            // Back row: plots 1-6
            int bi = c + 1;
            var bgGO = GameObject.Find("FarmPlot_" + bi);
            if (bgGO != null)
            {
                bgGO.transform.position   = new Vector3(x, ry0, 0f);
                bgGO.transform.localScale = new Vector3(scale, scale, 1f);
                FixCol(bgGO);
                EditorUtility.SetDirty(bgGO);
            }

            // Front row: plots 7-12
            int fi = c + 7;
            var fgGO = GameObject.Find("FarmPlot_" + fi);
            if (fgGO != null)
            {
                fgGO.transform.position   = new Vector3(x, ry1, 0f);
                fgGO.transform.localScale = new Vector3(scale, scale, 1f);
                FixCol(fgGO);
                EditorUtility.SetDirty(fgGO);
            }
        }

        // Camera start: x=-1.5 shows first 3 cols (-4.8,-2.8,-0.8)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(-1.5f, 0f, -10f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Scroller: can scroll from x=-1.5 (start) to x=+1.7 (shows last 3 cols)
        var gm = GameObject.Find("GameManager");
        var scroller = gm?.GetComponent<FarmMapScroller>();
        if (scroller != null)
        {
            scroller.mapMinX       = -1.5f;
            scroller.mapMaxX       =  1.8f;
            scroller.scrollSpeed   = 10f;
            scroller.snapSmoothing = 8f;
            EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("RealignFarmPlots done! Cols at -4.8,-2.8,-0.8,+1.2,+3.2,+5.2");
    }

    static void FixCol(GameObject go)
    {
        var col = go.GetComponent<BoxCollider2D>();
        if (col != null) col.size = new Vector2(0.16f, 0.16f);

        var label = go.transform.Find("Label");
        if (label == null) return;
        label.localScale    = new Vector3(0.034f, 0.034f, 1f);
        label.localPosition = new Vector3(0f, 0.004f, -0.1f);
        var tmp = label.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.fontSize           = 10f;
            tmp.enableWordWrapping = false;
            tmp.textWrappingMode   = TMPro.TextWrappingModes.NoWrap;
            tmp.overflowMode       = TMPro.TextOverflowModes.Overflow;
        }
        EditorUtility.SetDirty(label.gameObject);
    }
}
