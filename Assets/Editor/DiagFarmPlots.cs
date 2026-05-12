using UnityEngine;
using UnityEditor;

public class DiagFarmPlots
{
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        int i = 0;
        foreach (Transform t in container.transform)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            Debug.Log($"[Plot{i}] pos={t.position} scale={t.localScale.x:F2} sprite={sr?.sprite?.name ?? "NULL"} enabled={sr?.enabled} color={sr?.color} sortOrder={sr?.sortingOrder} mat={sr?.sharedMaterial?.name}");
            i++;
            if (i >= 3) break;
        }
    }
}
