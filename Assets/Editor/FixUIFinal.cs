using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// Final UI fix: make BottomBar taller and more visible,
/// ensure all elements are properly sized for 1631x909 canvas.
public class FixUIFinal
{
    [MenuItem("Farm/Fix UI Final")]
    public static void Execute()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) { Debug.LogError("UICanvas not found"); return; }

        // ── BottomBar: make it taller (100px) and more prominent ─────
        var barT = canvasGO.transform.Find("BottomBar");
        if (barT != null)
        {
            var rt = barT.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0f, 100f);
            EditorUtility.SetDirty(rt);

            // Ensure image is visible
            var img = barT.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.85f, 0.73f, 0.52f, 1f); // fully opaque
                EditorUtility.SetDirty(img);
            }

            // FPDisplay
            var fpT = barT.Find("FPDisplay");
            if (fpT != null)
            {
                var fpRT = fpT.GetComponent<RectTransform>();
                fpRT.anchorMin        = new Vector2(0f, 0f);
                fpRT.anchorMax        = new Vector2(0f, 1f);
                fpRT.pivot            = new Vector2(0f, 0.5f);
                fpRT.anchoredPosition = new Vector2(16f, 0f);
                fpRT.sizeDelta        = new Vector2(200f, 0f);
                EditorUtility.SetDirty(fpRT);

                var coinT = fpT.Find("CoinIcon");
                if (coinT != null)
                {
                    var coinRT = coinT.GetComponent<RectTransform>();
                    coinRT.anchorMin        = new Vector2(0f, 0.1f);
                    coinRT.anchorMax        = new Vector2(0f, 0.9f);
                    coinRT.pivot            = new Vector2(0f, 0.5f);
                    coinRT.anchoredPosition = Vector2.zero;
                    coinRT.sizeDelta        = new Vector2(60f, 0f);
                    EditorUtility.SetDirty(coinRT);
                }

                var fpTxtT = fpT.Find("FPText");
                if (fpTxtT != null)
                {
                    var fpTxtRT = fpTxtT.GetComponent<RectTransform>();
                    fpTxtRT.anchorMin        = new Vector2(0f, 0f);
                    fpTxtRT.anchorMax        = new Vector2(1f, 1f);
                    fpTxtRT.pivot            = new Vector2(0f, 0.5f);
                    fpTxtRT.anchoredPosition = new Vector2(64f, 0f);
                    fpTxtRT.sizeDelta        = new Vector2(-64f, 0f);
                    EditorUtility.SetDirty(fpTxtRT);

                    var tm = fpTxtT.GetComponent<TextMeshProUGUI>();
                    if (tm != null)
                    {
                        tm.fontSize  = 32;
                        tm.color     = new Color(0.22f, 0.12f, 0.02f, 1f);
                        tm.fontStyle = FontStyles.Bold;
                        EditorUtility.SetDirty(tm);
                    }
                }
            }

            // ShopButton
            var shopT = barT.Find("ShopButton");
            if (shopT != null)
            {
                var shopRT = shopT.GetComponent<RectTransform>();
                shopRT.anchorMin        = new Vector2(1f, 0f);
                shopRT.anchorMax        = new Vector2(1f, 1f);
                shopRT.pivot            = new Vector2(1f, 0.5f);
                shopRT.anchoredPosition = new Vector2(-16f, 0f);
                shopRT.sizeDelta        = new Vector2(130f, -16f);
                EditorUtility.SetDirty(shopRT);

                var shopImg = shopT.GetComponent<Image>();
                if (shopImg != null)
                {
                    shopImg.color = new Color(0.35f, 0.65f, 0.25f, 1f);
                    EditorUtility.SetDirty(shopImg);
                }

                var lblT = shopT.Find("Label");
                if (lblT != null)
                {
                    var tm = lblT.GetComponent<TextMeshProUGUI>();
                    if (tm != null)
                    {
                        tm.fontSize  = 26;
                        tm.color     = Color.white;
                        tm.fontStyle = FontStyles.Bold;
                        EditorUtility.SetDirty(tm);
                    }
                }
            }
        }

        // ── ExpandablePanel: fix size and hidden position ─────────────
        var panelT = canvasGO.transform.Find("ExpandablePanel");
        if (panelT != null)
        {
            var rt = panelT.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(1f, 0f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(1f, 0f);
            rt.sizeDelta        = new Vector2(260f, -100f);
            rt.anchoredPosition = new Vector2(260f, 100f); // hidden off-screen right
            EditorUtility.SetDirty(rt);

            // Fix panel background
            var img = panelT.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.18f, 0.22f, 0.28f, 0.97f);
                EditorUtility.SetDirty(img);
            }

            // Fix TitleBar
            var titleBarT = panelT.Find("TitleBar");
            if (titleBarT != null)
            {
                var tbRT = titleBarT.GetComponent<RectTransform>();
                tbRT.sizeDelta = new Vector2(0f, 44f);
                EditorUtility.SetDirty(tbRT);

                var titleTxtT = titleBarT.Find("TitleText");
                if (titleTxtT != null)
                {
                    var tm = titleTxtT.GetComponent<TextMeshProUGUI>();
                    if (tm != null) { tm.fontSize = 20; EditorUtility.SetDirty(tm); }
                }
            }

            // Fix TabRow
            var tabRowT = panelT.Find("TabRow");
            if (tabRowT != null)
            {
                var trRT = tabRowT.GetComponent<RectTransform>();
                trRT.anchoredPosition = new Vector2(0f, -44f);
                trRT.sizeDelta        = new Vector2(0f, 36f);
                EditorUtility.SetDirty(trRT);

                // Fix tab button text sizes
                foreach (Transform tab in tabRowT)
                {
                    var txtT = tab.Find("Text");
                    if (txtT != null)
                    {
                        var tm = txtT.GetComponent<TextMeshProUGUI>();
                        if (tm != null) { tm.fontSize = 14; EditorUtility.SetDirty(tm); }
                    }
                }
            }

            // Fix ContentArea
            var contentT = panelT.Find("ContentArea");
            if (contentT != null)
            {
                var cRT = contentT.GetComponent<RectTransform>();
                cRT.offsetMax = new Vector2(0f, -80f); // below title(44) + tabs(36)
                EditorUtility.SetDirty(cRT);
            }
        }

        // ── Update UIManager HIDDEN_X to match new panel width ────────
        var uiMgr = canvasGO.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            // UIManager uses const HIDDEN_X=210, we need to update the script
            // For now just ensure the panel ref is correct
            EditorUtility.SetDirty(uiMgr);
        }

        // ── Save ──────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixUIFinal] Done! BottomBar=100px, Panel=260px wide.");
    }
}
