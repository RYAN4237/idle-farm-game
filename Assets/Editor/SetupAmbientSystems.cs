using UnityEngine;
using UnityEditor;

public class SetupAmbientSystems
{
    public static void Execute()
    {
        // ── 1. Camera: black background = transparent key ──────────────
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            var camera = cam.GetComponent<Camera>();
            camera.clearFlags       = CameraClearFlags.SolidColor;
            camera.backgroundColor  = Color.black; // key color for transparency
            EditorUtility.SetDirty(cam);
            Debug.Log("Camera: black background (transparent key)");
        }

        // ── 2. GameManager: add persistent systems ─────────────────────
        var gm = GameObject.Find("GameManager");
        if (gm == null)
        {
            gm = new GameObject("GameManager");
            Debug.Log("Created GameManager");
        }

        // WindowManager
        if (gm.GetComponent<WindowManager>() == null)
        {
            var wm = gm.AddComponent<WindowManager>();
            wm.isFarmWindow   = true;
            wm.farmHeight     = 160; // px tall for the farm strip
            wm.transparent    = true;
            wm.alwaysOnTop    = true;
            wm.removeTaskbar  = true;
            wm.removeTitle    = true;
            Debug.Log("Added WindowManager");
        }

        // AmbientPerformance
        if (gm.GetComponent<AmbientPerformance>() == null)
        {
            var ap = gm.AddComponent<AmbientPerformance>();
            ap.idleFPS   = 15;
            ap.activeFPS = 30;
            ap.boostFPS  = 30;
            Debug.Log("Added AmbientPerformance");
        }

        // FocusEventBridge
        if (gm.GetComponent<FocusEventBridge>() == null)
        {
            var feb = gm.AddComponent<FocusEventBridge>();
            feb.boostMultiplier = 2f;
            feb.boostDuration   = 300f;
            Debug.Log("Added FocusEventBridge");
        }

        // FarmBoostReceiver
        if (gm.GetComponent<FarmBoostReceiver>() == null)
        {
            gm.AddComponent<FarmBoostReceiver>();
            Debug.Log("Added FarmBoostReceiver");
        }

        EditorUtility.SetDirty(gm);

        // ── 3. UICanvas: make background transparent ───────────────────
        var canvas = GameObject.Find("UICanvas");
        if (canvas != null)
        {
            // Remove any solid background image on the canvas root
            var img = canvas.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0,0,0,0);
            EditorUtility.SetDirty(canvas);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SetupAmbientSystems] Done! Farm is now an ambient desktop app.");
    }
}
