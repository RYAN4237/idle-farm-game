using UnityEngine;
using UnityEditor;
using TMPro;

public class FixFarmLayout
{
    public static void Execute()
    {
        // Scale=8 → size=1.28. Step=2.0 gives gap=0.72 (nice spacing)
        float scale = 8f;
        float step  = 2.0f;
        float ry0   =  0.85f;
        float ry1   = -0.85f;
        int   cols  = 6;

        // Start from x=-5.0 so first 3 cols visible at cam x=0 (ortho=2.8, half-width=4.98)
        // Cols: -5.0, -3.0, -1.0, +1.0, +3.0, +5.0
        float startX = -5.0f;

        for (int c = 0; c < cols; c++)
        {
            float x = startX + c * step;

            // Back row (plots 1-6)
            int backIdx  = c + 1;
            var backGO   = GameObject.Find("FarmPlot_" + backIdx);
            if (backGO != null)
            {
                backGO.transform.position   = new Vector3(x, ry0, 0f);
                backGO.transform.localScale = new Vector3(scale, scale, 1f);
                FixLabel(backGO);
                EditorUtility.SetDirty(backGO);
            }

            // Front row (plots 7-12)
            int frontIdx = c + 7;
            var frontGO  = GameObject.Find("FarmPlot_" + frontIdx);
            if (frontGO != null)
            {
                frontGO.transform.position   = new Vector3(x, ry1, 0f);
                frontGO.transform.localScale = new Vector3(scale, scale, 1f);
                FixLabel(frontGO);
                EditorUtility.SetDirty(frontGO);
            }
        }

        // Fix unlock costs
        var gm = GameObject.Find("GameManager");
        var shop = gm?.GetComponent<CropShop>();
        if (shop != null)
        {
            shop.plotUnlockCosts = new float[]
            {
                0f,   0f,   0f,          // 1-3: free
                80f,  150f, 300f,        // 4-6
                500f, 800f, 1200f,       // 7-9
                1800f,2500f,3500f        // 10-12
            };
            EditorUtility.SetDirty(gm);
            Debug.Log("CropShop unlock costs updated.");
        }

        // FarmMapScroller bounds: cam can scroll from x=-2 to x=+3
        // At x=-2: can see cols at -5,-3,-1 (leftmost 3)
        // At x=+3: can see cols at +1,+3,+5 (rightmost 3)
        var scroller = gm?.GetComponent<FarmMapScroller>();
        if (scroller != null)
        {
            scroller.mapMinX = -2.5f;
            scroller.mapMaxX =  3.0f;
            EditorUtility.SetDirty(gm);
            Debug.Log("FarmMapScroller bounds set: -2.5 to +3.0");
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixFarmLayout complete + saved!");
    }

    static void FixLabel(GameObject go)
    {
        var label = go.transform.Find("Label");
        if (label == null) return;
        label.localScale    = new Vector3(0.034f, 0.034f, 1f);
        label.localPosition = new Vector3(0f, 0.004f, -0.1f);
        var tmp = label.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.fontSize            = 10f;
            tmp.enableWordWrapping  = false;
            tmp.textWrappingMode    = TMPro.TextWrappingModes.NoWrap;
            tmp.overflowMode        = TMPro.TextOverflowModes.Overflow;
        }
        EditorUtility.SetDirty(label.gameObject);
    }
}
