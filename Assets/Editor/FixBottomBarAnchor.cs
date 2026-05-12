using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// Diagnoses and fixes the BottomBar so it fills the bottom of the screen properly.
public class FixBottomBarAnchor
{
    [MenuItem("Farm/Fix Bottom Bar Anchor")]
    public static void Execute()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) { Debug.LogError("UICanvas not found"); return; }

        var canvasRT = canvasGO.GetComponent<RectTransform>();
        Debug.Log($"Canvas rect: {canvasRT.rect}");
        Debug.Log($"Canvas sizeDelta: {canvasRT.sizeDelta}");

        var barT = canvasGO.transform.Find("BottomBar");
        if (barT == null) { Debug.LogError("BottomBar not found"); return; }

        var brt = barT.GetComponent<RectTransform>();

        // Log world corners before fix
        Vector3[] corners = new Vector3[4];
        brt.GetWorldCorners(corners);
        Debug.Log($"BottomBar BEFORE - BL={corners[0]} TL={corners[1]} TR={corners[2]} BR={corners[3]}");
        Debug.Log($"BottomBar BEFORE - rect={brt.rect} localPos={brt.localPosition}");

        // The canvas is 1631x909, pivot (0.5,0.5), so:
        // Bottom-left of canvas = (-815.5, -454.5) in local space
        // Bottom-right = (815.5, -454.5)
        // We want BottomBar to span full width at bottom, height=100px
        // anchorMin=(0,0) anchorMax=(1,0) means: left=0%, right=100%, bottom=0%
        // pivot=(0.5,0) means: pivot at bottom-center
        // anchoredPosition=(0,0) means: bottom edge at y=0% of canvas = bottom of screen
        // sizeDelta=(0,100) means: height=100px (width follows anchors)

        brt.anchorMin        = new Vector2(0f, 0f);
        brt.anchorMax        = new Vector2(1f, 0f);
        brt.pivot            = new Vector2(0.5f, 0f);
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta        = new Vector2(0f, 100f);

        brt.GetWorldCorners(corners);
        Debug.Log($"BottomBar AFTER - BL={corners[0]} TL={corners[1]} TR={corners[2]} BR={corners[3]}");
        Debug.Log($"BottomBar AFTER - rect={brt.rect} localPos={brt.localPosition}");

        EditorUtility.SetDirty(brt);

        // Also fix ExpandablePanel to sit above BottomBar
        var panelT = canvasGO.transform.Find("ExpandablePanel");
        if (panelT != null)
        {
            var prt = panelT.GetComponent<RectTransform>();
            // Anchor: right edge, from bottom of BottomBar to top of screen
            prt.anchorMin        = new Vector2(1f, 0f);
            prt.anchorMax        = new Vector2(1f, 1f);
            prt.pivot            = new Vector2(1f, 0f);
            // sizeDelta.y = -(BottomBar height) to leave room at bottom
            prt.sizeDelta        = new Vector2(260f, -100f);
            // Hidden: 260px off right edge, y offset = 100 (above BottomBar)
            prt.anchoredPosition = new Vector2(260f, 100f);
            EditorUtility.SetDirty(prt);

            prt.GetWorldCorners(corners);
            Debug.Log($"ExpandablePanel AFTER - BL={corners[0]} TL={corners[1]} TR={corners[2]} BR={corners[3]}");
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixBottomBarAnchor] Done!");
    }
}
