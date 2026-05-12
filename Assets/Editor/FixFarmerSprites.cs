using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixFarmerSprites
{
    [MenuItem("Tools/Fix Farmer Walk Sprites")]
    public static void Execute()
    {
        var farmer = GameObject.Find("Farmer");
        if (farmer == null) { Debug.LogError("Farmer not found"); return; }
        var fc = farmer.GetComponent<FarmerCharacter>();
        if (fc == null) { Debug.LogError("FarmerCharacter not found"); return; }

        string sheetPath = "Assets/Sprout Lands - Sprites - Basic pack/Characters/Basic Charakter Spritesheet.png";
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);

        // Build sprite dictionary by index
        var sprites = new System.Collections.Generic.Dictionary<int, Sprite>();
        foreach (var a in allAssets)
        {
            if (a is Sprite s)
            {
                var parts = s.name.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int idx))
                    sprites[idx] = s;
            }
        }
        Debug.Log($"Total char sprites: {sprites.Count}");

        // Sprout Lands Basic Character:
        // The spritesheet is 4 columns x 4 rows of 48x48 sprites
        // Row 0 (y=144 in Unity coords, top in pixel): walk down frames  0,1,2,3
        // Row 1 (y=96):  walk left/right frames                          4,5,6,7
        // Row 2 (y=48):  walk up frames                                  8,9,10,11
        // Row 3 (y=0):   idle/misc                                       12,13,14,15
        // But from logs: indices 12-15 at y=0, 8-11 at y=48, etc.
        // => index order is bottom-to-top in Unity sprite coords
        // Down = row at highest pixel y = index 0-3
        // Let's assign: down=0-3, up=4-7, left=8-11, right=8-11(flipped)

        Sprite[] GetFrames(int start, int count)
        {
            var arr = new Sprite[count];
            for (int i = 0; i < count; i++)
                sprites.TryGetValue(start + i, out arr[i]);
            return arr;
        }

        // Based on Sprout Lands sheet layout (4x4, 48px):
        // Row top→bottom in pixels: down-walk, left-walk, right-walk, up-walk
        // Unity loads bottom→top: idx 0=bottom-left → top-right
        // From logs: idx 12-15 at y=0 (bottom row), idx 0-3 at y=144 (top row)
        // Top row in pixels = walk down animation = indices 0-3 in Unity numbering? 
        // Actually sprite index goes left-to-right, bottom-to-top in Unity
        // So idx 0 = bottom-left (y=0), idx 12 = top-left (y=144)
        // Typical Sprout Lands layout (top→bottom): down, left, right, up
        // In Unity indices (bottom→top): up=0-3, right=4-7, left=8-11, down=12-15
        
        var so = new SerializedObject(fc);
        
        void SetArray(string propName, Sprite[] frames)
        {
            var prop = so.FindProperty(propName);
            prop.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }

        SetArray("walkDownFrames",  GetFrames(12, 4)); // top pixel row = down
        SetArray("walkUpFrames",    GetFrames(0, 4));  // bottom pixel row = up
        SetArray("walkLeftFrames",  GetFrames(8, 4));  
        SetArray("walkRightFrames", GetFrames(4, 4));
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(fc);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Farmer walk frames assigned. Down=12-15, Up=0-3, Left=8-11, Right=4-7");
    }
}
