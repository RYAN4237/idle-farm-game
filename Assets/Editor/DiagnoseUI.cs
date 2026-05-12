using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DiagnoseUI
{
    [MenuItem("Farm/Diagnose UI")]
    public static void Execute()
    {
        // Game View size
        var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        var gameView = EditorWindow.GetWindow(gameViewType);
        var sizeInfo = gameViewType?.GetProperty("currentGameViewSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Debug.Log($"[Diagnose] Screen: {Screen.width}x{Screen.height}");
        Debug.Log($"[Diagnose] Display: {Display.main.renderingWidth}x{Display.main.renderingHeight}");

        var canvas = GameObject.Find("UICanvas");
        if (canvas)
        {
            var c = canvas.GetComponent<Canvas>();
            var cs = canvas.GetComponent<CanvasScaler>();
            var rt = canvas.GetComponent<RectTransform>();
            Debug.Log($"[Diagnose] Canvas scaleFactor={c.scaleFactor}");
            Debug.Log($"[Diagnose] Canvas renderingDisplaySize={c.renderingDisplaySize}");
            Debug.Log($"[Diagnose] CanvasScaler ref={cs.referenceResolution} match={cs.matchWidthOrHeight}");
            Debug.Log($"[Diagnose] Canvas RectTransform size={rt.sizeDelta}");

            var bar = canvas.transform.Find("BottomBar");
            if (bar)
            {
                var brt = bar.GetComponent<RectTransform>();
                Debug.Log($"[Diagnose] BottomBar anchorMin={brt.anchorMin} anchorMax={brt.anchorMax}");
                Debug.Log($"[Diagnose] BottomBar pivot={brt.pivot}");
                Debug.Log($"[Diagnose] BottomBar anchoredPos={brt.anchoredPosition}");
                Debug.Log($"[Diagnose] BottomBar sizeDelta={brt.sizeDelta}");
                Debug.Log($"[Diagnose] BottomBar localPos={brt.localPosition}");
                Debug.Log($"[Diagnose] BottomBar rect={brt.rect}");

                // World corners
                Vector3[] corners = new Vector3[4];
                brt.GetWorldCorners(corners);
                Debug.Log($"[Diagnose] BottomBar world corners: BL={corners[0]} TL={corners[1]} TR={corners[2]} BR={corners[3]}");
            }

            var panel = canvas.transform.Find("ExpandablePanel");
            if (panel)
            {
                var prt = panel.GetComponent<RectTransform>();
                Debug.Log($"[Diagnose] ExpandablePanel anchoredPos={prt.anchoredPosition} sizeDelta={prt.sizeDelta}");
            }
        }

        // Camera
        var cam = Camera.main;
        if (cam)
        {
            Debug.Log($"[Diagnose] Camera orthoSize={cam.orthographicSize} pos={cam.transform.position} bgColor={cam.backgroundColor}");
            Debug.Log($"[Diagnose] Camera pixelRect={cam.pixelRect}");
        }
    }
}
