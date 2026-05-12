using UnityEngine;
using UnityEditor;

public class FixSkyBG
{
    [MenuItem("Farm/Fix Sky")]
    public static void Execute()
    {
        var go = GameObject.Find("SkyBG");
        if (go == null) { Debug.LogWarning("SkyBG not found"); return; }

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Switch to Simple mode so scale controls size
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = -100;

        // Camera top = 2.7 + 5 = 7.7, need sky to cover y=5.7~8.0
        // Width: cover full camera width = 17.92w, centered at x=10.5
        // Scale to fill: sprite is 28.4w x 2.17h, target 20w x 2.5h
        go.transform.position = new Vector3(10.5f, 7.0f, 1f);
        go.transform.localScale = new Vector3(0.7f, 1.15f, 1f);

        Debug.Log("SkyBG fixed.");
    }
}
