using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixCanvasAndGameView
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");

        // ── CanvasScaler: Scale With Screen Size, 1920x220 ──
        var scaler = canvas?.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 220f);
            scaler.matchWidthOrHeight  = 1f;  // match HEIGHT — bar height is fixed at 220px
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            EditorUtility.SetDirty(canvas);
            Debug.Log("CanvasScaler: referenceResolution=1920x220, matchHeight=1");
        }

        // ── Add 1920x220 Game View resolution ──
        AddGameViewSize(1920, 220, "Bar 1920x220");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixCanvasAndGameView complete + saved!");
    }

    static void AddGameViewSize(int w, int h, string label)
    {
        try
        {
            var T      = System.Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
            var gvType = System.Type.GetType("UnityEditor.GameViewSize,UnityEditor");
            var inst   = T?.GetProperty("instance",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.GetValue(null);
            var group  = inst?.GetType().GetMethod("GetGroup")
                ?.Invoke(inst, new object[]{(int)GameViewSizeGroupType.Standalone});

            if (group == null) { Debug.LogWarning("Could not get GameViewSizeGroup"); return; }

            // GameViewSize(GameViewSizeType.FixedResolution, w, h, label)
            var ctor  = gvType?.GetConstructors()[0];
            var entry = ctor?.Invoke(new object[]{ 1, w, h, label }); // 1 = FixedResolution
            group.GetType().GetMethod("AddCustomSize")?.Invoke(group, new object[]{entry});
            Debug.Log($"Game View size {w}x{h} added.");
        }
        catch(System.Exception e)
        {
            Debug.LogWarning("AddGameViewSize: " + e.Message);
        }
    }
}
