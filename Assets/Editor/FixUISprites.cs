using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class FixUISprites
{
    [MenuItem("Tools/Fix UI Sprites")]
    public static void Execute()
    {
        // Load all sprites from the basic pack spritesheet
        string sheetPath = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/Sprite sheet for Basic Pack.png";
        var allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .ToArray();

        Debug.Log($"[FixUISprites] Loaded {allSprites.Length} sprites from sheet");

        // The spritesheet is 896x240. Looking at the image:
        // - Row y=0..15 (bottom row in Unity coords) = progress bars / small items
        // - Row y=16..31 = small icons row
        // - Large panel sprites are at bottom of image visually → high y in Unity (Unity y=0 is bottom)
        // - The big panel backgrounds (light tan, 33x33) are around x:0-32, y:192-224 visually top = y:0-48 in Unity
        //
        // Strategy: find the largest sprite that looks like a panel background (square-ish, tan colored)
        // and the wide button sprite.

        // Print sprite positions to identify them
        var sortedByArea = allSprites
            .OrderByDescending(s => s.rect.width * s.rect.height)
            .Take(20)
            .ToArray();

        Debug.Log("=== Largest sprites ===");
        foreach (var s in sortedByArea)
            Debug.Log($"  {s.name}: rect={s.rect}, pivot={s.pivot}");

        // Find panel-like sprites: wide and tall (32+ pixels)
        var panelCandidates = allSprites.Where(s => s.rect.width >= 32 && s.rect.height >= 32).ToArray();
        Debug.Log($"=== Panel candidates ({panelCandidates.Length}) ===");
        foreach (var s in panelCandidates)
            Debug.Log($"  {s.name}: {s.rect}");

        // Find button-like sprites: wide, short (width>48, height 16-32)
        var buttonCandidates = allSprites.Where(s => s.rect.width >= 48 && s.rect.height >= 14 && s.rect.height <= 24).ToArray();
        Debug.Log($"=== Button candidates ({buttonCandidates.Length}) ===");
        foreach (var s in buttonCandidates)
            Debug.Log($"  {s.name}: {s.rect}");

        // Find the best panel sprite: largest area, roughly square
        Sprite panelSprite = panelCandidates
            .OrderByDescending(s => s.rect.width * s.rect.height)
            .FirstOrDefault();

        // Find the best button sprite: widest button-shaped
        Sprite buttonSprite = buttonCandidates
            .OrderByDescending(s => s.rect.width)
            .FirstOrDefault();

        if (panelSprite != null)
            Debug.Log($"[FixUISprites] Selected panel sprite: {panelSprite.name} {panelSprite.rect}");
        if (buttonSprite != null)
            Debug.Log($"[FixUISprites] Selected button sprite: {buttonSprite.name} {buttonSprite.rect}");

        // Apply to scene objects
        ApplyToScene(panelSprite, buttonSprite);
    }

    static void ApplyToScene(Sprite panelSprite, Sprite buttonSprite)
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("[FixUISprites] UICanvas not found"); return; }

        // --- BottomBar ---
        var bottomBar = canvas.transform.Find("BottomBar");
        if (bottomBar != null && panelSprite != null)
        {
            var img = bottomBar.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = panelSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                // Make border data work: set pixelsPerUnitMultiplier so the 9-slice looks right at 64px height
                img.pixelsPerUnitMultiplier = 1f;
                Debug.Log("[FixUISprites] Applied panel sprite to BottomBar");
            }
        }

        // --- ShopButton ---
        var shopBtn = canvas.transform.Find("BottomBar/ShopButton");
        if (shopBtn != null && buttonSprite != null)
        {
            var img = shopBtn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = buttonSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                img.pixelsPerUnitMultiplier = 1f;
                Debug.Log("[FixUISprites] Applied button sprite to ShopButton");
            }
        }

        // --- FPDisplay background ---
        var fpDisplay = canvas.transform.Find("BottomBar/FPDisplay");
        if (fpDisplay != null)
        {
            // Add a background image to FPDisplay if it doesn't have one
            var img = fpDisplay.GetComponent<Image>();
            if (img == null)
                img = fpDisplay.gameObject.AddComponent<Image>();
            if (img != null && buttonSprite != null)
            {
                img.sprite = buttonSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                img.pixelsPerUnitMultiplier = 1f;
                Debug.Log("[FixUISprites] Applied button sprite to FPDisplay bg");
            }
        }

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixUISprites] Done");
    }
}
