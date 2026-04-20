using UnityEngine;
using UnityEditor;
using TMPro;

public class FixPlotPositions
{
    public static void Execute()
    {
        // Camera ortho=2.8, aspect=16/9
        // World width total = 2 * 2.8 * (16/9) = 9.956 units
        // World x: -4.978 to +4.978

        // Farm area = 10% to 52% of screen width
        // Farm x start = -4.978 + 9.956 * 0.10 = -3.982
        // Farm x end   = -4.978 + 9.956 * 0.52 = +0.199
        // Farm width   = 4.181 units
        // Farm center x = (-3.982 + 0.199) / 2 = -1.892

        float camHalfW  = 2.8f * (16f / 9f); // = 4.978
        float farmStart = -camHalfW + 9.956f * 0.10f; // = -3.982
        float farmEnd   = -camHalfW + 9.956f * 0.52f; // = 0.199
        float farmCX    = (farmStart + farmEnd) * 0.5f; // = -1.892
        float farmW     = farmEnd - farmStart;           // = 4.181

        // 3 plots per row: spacing = farmW / 3 = 1.394
        float sp  = farmW / 3.5f;   // ~1.2 units between centers
        float ry0 =  0.85f;
        float ry1 = -0.85f;

        var pos = new Vector3[]
        {
            new Vector3(farmCX - sp, ry0, 0f),
            new Vector3(farmCX,      ry0, 0f),
            new Vector3(farmCX + sp, ry0, 0f),
            new Vector3(farmCX - sp, ry1, 0f),
            new Vector3(farmCX,      ry1, 0f),
            new Vector3(farmCX + sp, ry1, 0f),
        };

        // scale=8 → 8×0.16 = 1.28 world units. Fits in farmW=4.18 with 3 plots: 3.84 used, ok
        for (int i = 0; i < 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(8f, 8f, 1f);

            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localScale    = new Vector3(0.034f, 0.034f, 1f);
                label.localPosition = new Vector3(0f, 0.004f, -0.1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null) tmp.fontSize = 10f;
                EditorUtility.SetDirty(label.gameObject);
            }

            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.055f, -0.05f);
                barBg.localScale    = new Vector3(0.013f, 0.006f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            EditorUtility.SetDirty(go);
            Debug.Log($"FarmPlot_{i+1} → ({pos[i].x:F2}, {pos[i].y:F2})");
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"FixPlotPositions done. farmCX={farmCX:F2}, sp={sp:F2}, scale=8");
    }
}
