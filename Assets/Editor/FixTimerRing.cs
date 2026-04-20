using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixTimerRing
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");

        // OuterRing and ProgressRing: make them a fixed square centered in the timer area
        // Timer area = left 20% of screen, bottom 26%
        // We want a circle: centered, square, with some padding

        foreach (var name in new[]{"OuterRing","ProgressRing"})
        {
            var t = canvas?.transform.Find(name);
            if (t == null) continue;

            var r = t.GetComponent<RectTransform>();
            // Fixed square centered in left section: 
            // pivot center, anchored to center of left bar area
            r.anchorMin        = new Vector2(0.5f * 0.20f, 0.5f * 0.26f);  // center of left+bar
            r.anchorMax        = new Vector2(0.5f * 0.20f, 0.5f * 0.26f);
            r.pivot            = new Vector2(0.5f, 0.5f);
            // Size: fit in left section — screen is ~1920x1080
            // Left 20% ≈ 384px, bar 26% ≈ 281px → ring max = min(384,281) - padding
            r.sizeDelta        = new Vector2(210f, 210f);
            r.anchoredPosition = new Vector2(0f, 10f); // slightly up

            var img = t.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;

            EditorUtility.SetDirty(t.gameObject);
        }

        // Timer text: position inside the ring (centered in left bar area)
        foreach (var name in new[]{"TimerText","StatusText","DurationLabel"})
        {
            var t = canvas?.transform.Find(name);
            if (t == null) continue;

            var r = t.GetComponent<RectTransform>();
            // Re-anchor to left bar area with specific vertical position
            float lx = 0.20f, by = 0.26f;
            switch (name)
            {
                case "StatusText":
                    r.anchorMin = new Vector2(0.01f, by * 0.65f);
                    r.anchorMax = new Vector2(lx - 0.01f, by * 0.82f);
                    break;
                case "TimerText":
                    r.anchorMin = new Vector2(0.01f, by * 0.36f);
                    r.anchorMax = new Vector2(lx - 0.01f, by * 0.65f);
                    break;
                case "DurationLabel":
                    r.anchorMin = new Vector2(0.01f, by * 0.12f);
                    r.anchorMax = new Vector2(lx - 0.01f, by * 0.30f);
                    break;
            }
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
            EditorUtility.SetDirty(t.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixTimerRing complete + saved!");
    }
}
