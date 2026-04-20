using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixButtons
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel");
        var buttonBar  = rightPanel?.Find("ButtonBar");

        // ── 1. ButtonBar: give it a proper size ──
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(240f, 52f);
            r.anchoredPosition = new Vector2(0f, -278f);
            EditorUtility.SetDirty(buttonBar.gameObject);

            // ── StartPauseButton ──
            var startBtn = buttonBar.Find("StartPauseButton");
            if (startBtn != null)
            {
                var r2 = startBtn.GetComponent<RectTransform>();
                r2.anchorMin        = new Vector2(0f, 0f);
                r2.anchorMax        = new Vector2(0.56f, 1f);
                r2.offsetMin        = new Vector2(2f, 2f);
                r2.offsetMax        = new Vector2(-2f, -2f);
                r2.anchoredPosition = Vector2.zero;
                r2.sizeDelta        = Vector2.zero;

                // Make sure it has an Image (needed for Button interaction)
                var img = startBtn.GetComponent<Image>() ?? startBtn.gameObject.AddComponent<Image>();
                img.color = new Color(0.20f, 0.85f, 0.70f, 1f);
                var btn = startBtn.GetComponent<Button>();
                if (btn != null) btn.targetGraphic = img;

                // Fix text
                var txt = startBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.text = "Start Focus"; txt.fontSize = 12f; txt.color = Color.white; }

                EditorUtility.SetDirty(startBtn.gameObject);
            }

            // ── ResetButton ──
            var resetBtn = buttonBar.Find("ResetButton");
            if (resetBtn != null)
            {
                var r2 = resetBtn.GetComponent<RectTransform>();
                r2.anchorMin        = new Vector2(0.60f, 0f);
                r2.anchorMax        = new Vector2(1f, 1f);
                r2.offsetMin        = new Vector2(2f, 2f);
                r2.offsetMax        = new Vector2(-2f, -2f);
                r2.anchoredPosition = Vector2.zero;
                r2.sizeDelta        = Vector2.zero;

                var img = resetBtn.GetComponent<Image>() ?? resetBtn.gameObject.AddComponent<Image>();
                img.color = new Color(0.30f, 0.30f, 0.35f, 1f);
                var btn = resetBtn.GetComponent<Button>();
                if (btn != null) btn.targetGraphic = img;

                var txt = resetBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.text = "Reset"; txt.fontSize = 12f; txt.color = Color.white; }

                EditorUtility.SetDirty(resetBtn.gameObject);
            }
        }

        // ── 2. AutoFarmer button ──
        var shopPanel = rightPanel?.Find("CropShopPanel");
        var afBtn     = shopPanel?.Find("AutoFarmerBtn");
        if (afBtn != null)
        {
            var r = afBtn.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.02f, 0.88f);
            r.anchorMax        = new Vector2(0.98f, 1.00f);
            r.offsetMin        = new Vector2(3f, 2f);
            r.offsetMax        = new Vector2(-3f, -2f);

            // Ensure Image exists for Button click area
            var img = afBtn.GetComponent<Image>() ?? afBtn.gameObject.AddComponent<Image>();
            img.color = new Color(0.20f, 0.30f, 0.42f, 1f);
            var btn = afBtn.GetComponent<Button>();
            if (btn != null) btn.targetGraphic = img;

            EditorUtility.SetDirty(afBtn.gameObject);
            Debug.Log("AutoFarmer button fixed.");
        }

        // ── 3. Also fix all crop buttons in shop ──
        if (shopPanel != null)
        {
            foreach (Transform child in shopPanel)
            {
                if (!child.name.StartsWith("Crop_")) continue;
                var img = child.GetComponent<Image>();
                var btn = child.GetComponent<Button>();
                if (img != null && btn != null)
                {
                    btn.targetGraphic = img;
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixButtons complete + saved!");
    }
}
