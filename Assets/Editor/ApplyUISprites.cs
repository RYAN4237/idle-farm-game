using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ApplyUISprites
{
    [MenuItem("Tools/Apply UI Sprites")]
    public static void Execute()
    {
        // --- BottomBar: use dialog box big (176x48, 9-slice border=12) ---
        string panelPath = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/Dialouge UI/dialog box big.png";
        var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(panelPath);

        // --- Buttons: use Square Buttons 26x26 sprite _3 (4th sprite = warm brown, normal state) ---
        string btnSheetPath = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/buttons/Square Buttons 26x26.png";
        var allBtnSprites = AssetDatabase.LoadAllAssetsAtPath(btnSheetPath).OfType<Sprite>().ToArray();
        // Sheet is 96x192 = 4 cols x ~8 rows of 26x26 sprites. Warm tan button is row 1 col 0 = index 4
        // Order: Unity loads bottom-to-top (y=0 is bottom). Row 0 (bottom) = grey light, Row 1 = tan, etc.
        foreach (var s in allBtnSprites.Take(12))
            Debug.Log($"  btn sprite: {s.name} rect={s.rect}");

        // Pick the warm brown button (rows 2+ from bottom = the tan/brown ones)
        // The sheet 4-wide: col0=normal, col1=hover, col2=pressed, col3=disabled
        // Row from bottom: 0=green tint, 1=grey-green, 2=light tan, 3=tan, 4=brown, etc.
        // We want "tan pressed" style for BottomBar display feel
        // Use _4 (index 4 = col0,row1 from bottom = second row, first column = light tan normal)
        Sprite btnNormal = allBtnSprites.FirstOrDefault(s => s.name == "Square Buttons 26x26_4");
        if (btnNormal == null && allBtnSprites.Length > 4) btnNormal = allBtnSprites[4];

        Debug.Log($"[ApplyUISprites] Panel sprite: {(panelSprite != null ? panelSprite.name : "NULL")}");
        Debug.Log($"[ApplyUISprites] Button sprite: {(btnNormal != null ? btnNormal.name + " " + btnNormal.rect : "NULL")}");

        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Apply panel sprite to BottomBar
        var bottomBar = canvas.transform.Find("BottomBar");
        if (bottomBar != null && panelSprite != null)
        {
            var img = bottomBar.GetComponent<Image>();
            img.sprite = panelSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.pixelsPerUnitMultiplier = 1f;
            // Make the bar taller so 9-slice looks better: 48px source → 64px bar is fine
            var rt = bottomBar.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 64);
            EditorUtility.SetDirty(img);
            Debug.Log("[ApplyUISprites] BottomBar: applied dialog box big sprite (9-slice)");
        }

        // Apply button sprite to ShopButton (9-slice the 26x26 square button)
        var shopBtn = canvas.transform.Find("BottomBar/ShopButton");
        if (shopBtn != null && btnNormal != null)
        {
            var img = shopBtn.GetComponent<Image>();
            // For the 26x26 sprite with no border set, use Simple mode scaled up
            img.sprite = btnNormal;
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.pixelsPerUnitMultiplier = 1f;
            img.preserveAspect = false;
            // Resize button to be more reasonable: 110x44
            var rt = shopBtn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(110, -12);
            EditorUtility.SetDirty(img);
            Debug.Log("[ApplyUISprites] ShopButton: applied square button sprite");
        }

        // Apply coin background to FPDisplay area using same button sprite
        var fpDisplay = canvas.transform.Find("BottomBar/FPDisplay");
        if (fpDisplay != null && btnNormal != null)
        {
            // Ensure FPDisplay has an Image component for background
            var img = fpDisplay.GetComponent<Image>();
            if (img == null) img = fpDisplay.gameObject.AddComponent<Image>();
            img.sprite = btnNormal;
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.raycastTarget = false;
            // Resize FPDisplay to fit nicely
            var rt = fpDisplay.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, -12);
            EditorUtility.SetDirty(img);
            Debug.Log("[ApplyUISprites] FPDisplay: applied button sprite background");
        }

        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[ApplyUISprites] Done");
    }
}
