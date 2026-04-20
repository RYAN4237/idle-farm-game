using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

public class BuildFarmingScene
{
    public static void Execute()
    {
        // ── 1. Refresh so Unity sees new Farming scripts ──
        AssetDatabase.Refresh();

        // ── 2. FarmingSystem on GameManager ──
        var gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            var fs = gm.GetComponent("FarmingSystem");
            if (fs == null) gm.AddComponent(System.Type.GetType("FarmingSystem"));
            Debug.Log("FarmingSystem ensured on GameManager.");
        }

        // ── 3. Farm root ──
        var existingFarm = GameObject.Find("FarmContainer");
        if (existingFarm != null) Object.DestroyImmediate(existingFarm);
        var farmRoot = new GameObject("FarmContainer");

        Vector3[] positions = new Vector3[]
        {
            new Vector3(-2.2f, -3.2f, 0f),
            new Vector3( 0.0f, -3.2f, 0f),
            new Vector3( 2.2f, -3.2f, 0f),
        };

        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        for (int i = 0; i < 3; i++)
        {
            var plotGO = new GameObject("FarmPlot_" + (i + 1));
            plotGO.transform.SetParent(farmRoot.transform);
            plotGO.transform.position = positions[i];
            plotGO.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            // SpriteRenderer
            var sr = plotGO.AddComponent<SpriteRenderer>();
            sr.sprite = uiSprite;
            sr.color  = new Color(0.35f, 0.28f, 0.20f, 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sortingOrder = 0;

            // BoxCollider2D
            plotGO.AddComponent<BoxCollider2D>();

            // FarmPlot script (added by type name after Refresh)
            var fpType = System.Type.GetType("FarmPlot");
            if (fpType != null) plotGO.AddComponent(fpType);
            else Debug.LogWarning("FarmPlot type not found — run again after compile.");

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(plotGO.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, 0.72f, -0.1f);
            labelGO.transform.localScale    = new Vector3(0.22f, 0.22f, 1f);
            var tmp = labelGO.AddComponent<TextMeshPro>();
            tmp.text      = "Plant\n(10 FP)";
            tmp.fontSize  = 12f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.enableWordWrapping = true;
            tmp.sortingOrder = 3;

            // Progress bar BG
            var barBgGO = new GameObject("ProgressBarBG");
            barBgGO.transform.SetParent(plotGO.transform, false);
            barBgGO.transform.localPosition = new Vector3(0f, -0.55f, -0.1f);
            barBgGO.transform.localScale    = new Vector3(0.85f, 0.10f, 1f);
            var barBgSR = barBgGO.AddComponent<SpriteRenderer>();
            barBgSR.sprite = uiSprite;
            barBgSR.color  = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            barBgSR.sortingOrder = 1;

            // Progress bar Fill
            var barFillGO = new GameObject("ProgressBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            barFillGO.transform.localPosition = new Vector3(-0.5f, 0f, -0.05f);
            barFillGO.transform.localScale    = new Vector3(0.001f, 1f, 1f);
            var barFillSR = barFillGO.AddComponent<SpriteRenderer>();
            barFillSR.sprite = uiSprite;
            barFillSR.color  = new Color(0.25f, 0.85f, 0.35f, 1f);
            barFillSR.sortingOrder = 2;
        }

        // ── 4. Physics2DRaycaster on camera ──
        var cam = Camera.main;
        if (cam != null)
        {
            var raycasterType = System.Type.GetType("UnityEngine.EventSystems.Physics2DRaycaster, UnityEngine.PhysicsModule");
            if (raycasterType == null)
                raycasterType = typeof(Physics2DRaycaster);
            if (cam.GetComponent(raycasterType) == null)
            {
                cam.gameObject.AddComponent(raycasterType);
                Debug.Log("Physics2DRaycaster added to Main Camera.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("BuildFarmingScene complete!");
    }
}
