using UnityEngine;
using UnityEditor;

public class FinalFixes
{
    public static void Execute()
    {
        // ── 1. FarmGrid: ensure originY puts farm in middle of camera ──
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            var grid = cam.GetComponent<FarmGrid>();
            if (grid != null)
            {
                grid.originX = -15f;
                grid.originY = -2f;
                EditorUtility.SetDirty(cam);
                Debug.Log($"FarmGrid: origin=({grid.originX},{grid.originY})");
            }
        }

        // ── 2. FarmMapScroller lives on Main Camera ────────────────────
        if (cam != null)
        {
            var scroller = cam.GetComponent<FarmMapScroller>();
            if (scroller != null)
            {
                scroller.mapMinX    = -13f;
                scroller.mapMaxX    =  13f;
                scroller.scrollSpeed = 12f;
                EditorUtility.SetDirty(cam);
                Debug.Log("FarmMapScroller updated on Camera");
            }
            else
            {
                // Try GameManager
                var gm = GameObject.Find("GameManager");
                var s2 = gm?.GetComponent<FarmMapScroller>();
                if (s2 != null)
                {
                    s2.mapMinX     = -13f;
                    s2.mapMaxX     =  13f;
                    s2.scrollSpeed =  12f;
                    EditorUtility.SetDirty(gm);
                    Debug.Log("FarmMapScroller updated on GameManager");
                }
            }
        }

        // ── 3. IdleSystem: confirm settings ───────────────────────────
        var gm2 = GameObject.Find("GameManager");
        if (gm2 != null)
        {
            var idle = gm2.GetComponent<IdleSystem>();
            if (idle != null)
            {
                idle.baseIncomePerSecond = 0.5f; // gentle idle
                idle.focusMultiplier     = 2f;
                EditorUtility.SetDirty(gm2);
                Debug.Log($"IdleSystem: base={idle.baseIncomePerSecond}/s, focusMult={idle.focusMultiplier}");
            }

            // ── 4. TransparentWindow: correct bar height ───────────────
            var tw = gm2.GetComponent<TransparentWindow>();
            if (tw != null)
            {
                tw.barHeight    = 160;
                tw.bottomOffset = 0;
                EditorUtility.SetDirty(gm2);
                Debug.Log($"TransparentWindow: barHeight={tw.barHeight}");
            }

            // ── 5. AmbientPerformance ──────────────────────────────────
            var ap = gm2.GetComponent<AmbientPerformance>();
            if (ap != null)
            {
                ap.idleFPS          = 15;
                ap.activeFPS        = 30;
                ap.idleAfterSeconds = 5f;
                EditorUtility.SetDirty(gm2);
                Debug.Log("AmbientPerformance: idle=15fps active=30fps");
            }
        }

        // ── 6. Build settings ──────────────────────────────────────────
        PlayerSettings.fullScreenMode      = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth  = 1920;
        PlayerSettings.defaultScreenHeight = 160;
        PlayerSettings.runInBackground     = true;
        PlayerSettings.SplashScreen.show   = false;
        Application.targetFrameRate        = 15;
        QualitySettings.vSyncCount         = 0;
        Debug.Log("Build settings: 1920x160 windowed, runInBackground, no splash");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FinalFixes] Done!");
    }
}
