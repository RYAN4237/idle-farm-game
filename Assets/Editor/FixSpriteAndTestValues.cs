using UnityEngine;
using UnityEditor;

public class FixSpriteAndTestValues
{
    public static void Execute()
    {
        for (int i = 1; i <= 3; i++)
        {
            var plotGO = GameObject.Find("FarmPlot_" + i);
            if (plotGO == null) continue;

            // Fix SpriteRenderer: Simple mode instead of Sliced
            var sr = plotGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.drawMode = SpriteDrawMode.Simple;
                plotGO.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
                EditorUtility.SetDirty(plotGO);
            }

            // Fix BoxCollider size to 1x1 (scale handles world size)
            var col = plotGO.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(1f, 1f);
                EditorUtility.SetDirty(plotGO);
            }

            // Fix ProgressBarBG
            var barBg = plotGO.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                var barBgSR = barBg.GetComponent<SpriteRenderer>();
                if (barBgSR != null) barBgSR.drawMode = SpriteDrawMode.Simple;
                barBg.localScale = new Vector3(0.85f, 0.08f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);

                var barFill = barBg.Find("ProgressBarFill");
                if (barFill != null)
                {
                    var fillSR = barFill.GetComponent<SpriteRenderer>();
                    if (fillSR != null) fillSR.drawMode = SpriteDrawMode.Simple;
                    EditorUtility.SetDirty(barFill.gameObject);
                }
            }

            // Set test values: cheap + fast for quick loop testing
            var plot = plotGO.GetComponent<FarmPlot>();
            if (plot != null)
            {
                plot.plantCost      = 1f;
                plot.growthDuration = 3f;
                plot.harvestReward  = 5f;
                EditorUtility.SetDirty(plot);
            }

            Debug.Log($"FarmPlot_{i}: drawMode=Simple, testValues(cost=1, grow=3s, reward=5)");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixSpriteAndTestValues complete!");
    }
}
