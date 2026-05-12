using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Builds the complete FocusFarm HUD under UICanvas in Farm.unity
public class BuildFarmHUD
{
    const string KUI   = "Assets/KenneyUI/";
    const string KFUI  = "Assets/KenneyFantasyUI/Panel/";
    const string KICO  = "Assets/KenneyIcons/";
    const string SL_UI = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/";
    const string SL_SP = "Assets/Sprout Lands - Sprites - Basic pack/";

    [MenuItem("Tools/Build Farm HUD")]
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Remove old HUD root if rebuilding
        var oldHUD = canvas.transform.Find("HUD");
        if (oldHUD != null) Object.DestroyImmediate(oldHUD.gameObject);

        var hud = MakeGO("HUD", canvas.transform);
        StretchFull(hud);

        BuildTopHUD(hud.transform);
        BuildQuestPanel(hud.transform);
        BuildInventoryPanel(hud.transform);
        BuildResourceBadges(hud.transform);
        BuildMinimap(hud.transform);
        BuildDialogueBox(hud.transform);
        BuildHotbar(hud.transform);

        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[BuildFarmHUD] Done");
    }

    // ── TOP HUD ──────────────────────────────────────────────────────────────
    static void BuildTopHUD(Transform hud)
    {
        var bar = MakeGO("TopHUD", hud);
        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 64);

        var bg = MakeImage("BG", bar.transform, KUI + "panel_brown.png");
        if (bg != null) { StretchFull(bg.gameObject); Slice9(bg); bg.color = new Color(0.22f, 0.14f, 0.08f, 0.92f); }

        // Avatar — use person icon as placeholder
        var avatar = MakeImage("Avatar", bar.transform, KICO + "singleplayer.png");
        if (avatar != null)
        {
            var art = avatar.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0, 0.5f);
            art.anchorMax = new Vector2(0, 0.5f);
            art.pivot     = new Vector2(0.5f, 0.5f);
            art.anchoredPosition = new Vector2(38, 0);
            art.sizeDelta = new Vector2(40, 40);
            avatar.color = new Color(0.95f, 0.85f, 0.65f, 1);
        }

        // 3 stat bars removed

        // Day / season label — center
        var dayLabel = MakeLabel("DayLabel", bar.transform, "Day 12  *  Spring");
        if (dayLabel != null)
        {
            var lrt = dayLabel.GetComponent<RectTransform>();
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = Vector2.zero;
            lrt.sizeDelta = new Vector2(280, 44);
            dayLabel.fontSize = 20;
            dayLabel.color = new Color(0.98f, 0.92f, 0.72f, 1);
            dayLabel.alignment = TextAlignmentOptions.Center;
        }

        // Gear icon — right side
        var gear = MakeImage("GearIcon", bar.transform, KICO + "gear.png");
        if (gear != null)
        {
            var grt = gear.GetComponent<RectTransform>();
            grt.anchorMin = new Vector2(1, 0.5f);
            grt.anchorMax = new Vector2(1, 0.5f);
            grt.pivot     = new Vector2(0.5f, 0.5f);
            grt.anchoredPosition = new Vector2(-36, 0);
            grt.sizeDelta = new Vector2(34, 34);
            gear.color = new Color(0.98f, 0.92f, 0.72f, 1);
        }
    }

    static void BuildStatBar(Transform parent, string id, string leftFile, string midFile, string rightFile, Vector2 anchoredPos, Color fillColor, string labelText, float fillPct)
    {
        var root = MakeGO(id + "Bar", parent);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(180, 14);

        var bgL = MakeImage("BgL", root.transform, KUI + leftFile  + ".png");
        var bgM = MakeImage("BgM", root.transform, KUI + midFile   + ".png");
        var bgR = MakeImage("BgR", root.transform, KUI + rightFile + ".png");
        LayoutBarTriple(bgL, bgM, bgR, 180, 14);

        var fill = MakeGO(id + "Fill", root.transform);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(fillPct / 100f, 1);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;
        fillImg.raycastTarget = false;

        var lbl = MakeLabel(id + "Label", root.transform, labelText);
        if (lbl != null)
        {
            lbl.fontSize = 10; lbl.color = Color.white;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            lbl.alignment = TextAlignmentOptions.Center;
        }
    }

    static void LayoutBarTriple(Image l, Image m, Image r, float totalW, float h)
    {
        if (l == null || m == null || r == null) return;
        float ew = h;
        SetRTExact(l.gameObject, 0, -h * 0.5f, ew, h);
        SetRTExact(m.gameObject, ew, -h * 0.5f, totalW - ew * 2, h);
        SetRTExact(r.gameObject, totalW - ew, -h * 0.5f, ew, h);
    }

    // ── QUEST PANEL ──────────────────────────────────────────────────────────
    static void BuildQuestPanel(Transform hud)
    {
        var panel = MakeGO("QuestPanel", hud);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(12, -78);
        rt.sizeDelta = new Vector2(220, 210);

        var bg = MakeImage("BG", panel.transform, KFUI + "panel-000.png");
        if (bg != null) { StretchFull(bg.gameObject); Slice9(bg); bg.color = new Color(0.18f, 0.12f, 0.08f, 0.95f); }

        var title = MakeLabel("Title", panel.transform, "Daily Quests");
        if (title != null)
        {
            var trt = title.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -10);
            trt.sizeDelta = new Vector2(-24, 28);
            title.fontSize = 15; title.fontStyle = FontStyles.Bold;
            title.color = new Color(0.98f, 0.88f, 0.55f, 1);
            title.alignment = TextAlignmentOptions.Center;
        }

        var div = MakeImage("Divider", panel.transform, null);
        if (div != null)
        {
            div.color = new Color(0.6f, 0.42f, 0.18f, 0.7f);
            div.sprite = null;
            var drt = div.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0, 1); drt.anchorMax = new Vector2(1, 1);
            drt.pivot = new Vector2(0.5f, 1);
            drt.anchoredPosition = new Vector2(0, -40);
            drt.sizeDelta = new Vector2(-20, 2);
        }

        string[] quests = { "Water 5 crops", "Harvest wheat x3", "Feed the animals", "Sell 10 items" };
        bool[] done = { true, true, false, false };
        for (int i = 0; i < quests.Length; i++)
            BuildQuestRow(panel.transform, quests[i], done[i], i);
    }

    static void BuildQuestRow(Transform parent, string text, bool completed, int idx)
    {
        var row = MakeGO("Quest_" + idx, parent);
        var rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(12, -48 - idx * 38);
        rt.sizeDelta = new Vector2(-24, 32);

        string iconPath = completed ? KUI + "iconCheck_bronze.png" : KUI + "iconCross_brown.png";
        var ico = MakeImage("Icon", row.transform, iconPath);
        if (ico != null)
        {
            ico.color = completed ? new Color(0.4f, 0.8f, 0.3f, 1) : new Color(0.85f, 0.35f, 0.25f, 1);
            SetAnchored(ico.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(11, 0), new Vector2(22, 22));
        }

        var lbl = MakeLabel("Text", row.transform, text);
        if (lbl != null)
        {
            lbl.fontSize = 12;
            lbl.color = completed ? new Color(0.55f, 0.75f, 0.45f, 1) : new Color(0.9f, 0.85f, 0.72f, 1);
            if (completed) lbl.fontStyle = FontStyles.Strikethrough;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(30, 0); lrt.offsetMax = Vector2.zero;
            lbl.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    // ── INVENTORY ────────────────────────────────────────────────────────────
    static void BuildInventoryPanel(Transform hud)
    {
        var panel = MakeGO("InventoryPanel", hud);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(12, -298);
        rt.sizeDelta = new Vector2(220, 200);

        var bg = MakeImage("BG", panel.transform, KUI + "panel_brown.png");
        if (bg != null) { StretchFull(bg.gameObject); Slice9(bg); bg.color = new Color(0.18f, 0.11f, 0.06f, 0.95f); }

        var title = MakeLabel("Title", panel.transform, "Inventory");
        if (title != null)
        {
            var trt = title.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -10);
            trt.sizeDelta = new Vector2(-16, 24);
            title.fontSize = 14; title.fontStyle = FontStyles.Bold;
            title.color = new Color(0.98f, 0.88f, 0.55f, 1);
            title.alignment = TextAlignmentOptions.Center;
        }

        Color[] gemColors = {
            new Color(0.9f, 0.3f, 0.3f, 1),   new Color(0.3f, 0.7f, 0.3f, 1),
            new Color(0.3f, 0.5f, 0.9f, 1),   new Color(0.9f, 0.75f, 0.2f, 1),
            new Color(0.75f, 0.3f, 0.85f, 1), new Color(0.3f, 0.85f, 0.75f, 1),
            new Color(0.9f, 0.55f, 0.2f, 1),  new Color(0, 0, 0, 0),
            new Color(0, 0, 0, 0),             new Color(0, 0, 0, 0),
            new Color(0, 0, 0, 0),             new Color(0, 0, 0, 0),
        };

        for (int i = 0; i < 12; i++)
        {
            int col = i % 4;
            int row = i / 4;

            var slot = MakeGO("Slot_" + i, panel.transform);
            var srt = slot.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(0, 1);
            srt.pivot = new Vector2(0, 1);
            srt.anchoredPosition = new Vector2(10 + col * 50, -40 - row * 50);
            srt.sizeDelta = new Vector2(46, 46);

            var slotBg = MakeImage("SlotBG", slot.transform, KUI + "panelInset_brown.png");
            if (slotBg != null) { StretchFull(slotBg.gameObject); Slice9(slotBg); slotBg.color = new Color(0.12f, 0.08f, 0.04f, 1); }

            if (i < gemColors.Length && gemColors[i].a > 0.1f)
            {
                var gem = MakeGO("Gem", slot.transform);
                var grt = gem.GetComponent<RectTransform>();
                grt.anchorMin = new Vector2(0.15f, 0.15f);
                grt.anchorMax = new Vector2(0.85f, 0.85f);
                grt.offsetMin = grt.offsetMax = Vector2.zero;
                var gImg = gem.AddComponent<Image>();
                gImg.color = gemColors[i];
                gImg.raycastTarget = false;
            }
        }
    }

    // ── RESOURCE BADGES ──────────────────────────────────────────────────────
    static void BuildResourceBadges(Transform hud)
    {
        var root = MakeGO("ResourceBadges", hud);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-12, -78);
        rt.sizeDelta = new Vector2(160, 160);

        (string icon, string label, string value, Color color)[] res = {
            (KICO + "star.png",           "Coins", "1,240", new Color(0.95f, 0.78f, 0.1f, 1)),
            (KICO + "menuList.png",       "Wood",  "85",    new Color(0.6f, 0.4f, 0.2f, 1)),
            (KICO + "information.png",    "Stone", "42",    new Color(0.7f, 0.7f, 0.7f, 1)),
            (KICO + "shoppingBasket.png", "Seeds", "17",    new Color(0.4f, 0.75f, 0.3f, 1)),
        };

        for (int i = 0; i < res.Length; i++)
        {
            var badge = MakeGO("Badge_" + res[i].label, root.transform);
            var brt = badge.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(1, 1);
            brt.anchoredPosition = new Vector2(0, -i * 38);
            brt.sizeDelta = new Vector2(155, 34);

            var bbg = MakeImage("BG", badge.transform, KUI + "buttonLong_brown.png");
            if (bbg != null) { StretchFull(bbg.gameObject); Slice9(bbg); bbg.color = new Color(0.18f, 0.11f, 0.06f, 0.92f); }

            var ico = MakeImage("Icon", badge.transform, res[i].icon);
            if (ico != null)
            {
                ico.color = res[i].color;
                SetAnchored(ico.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(18, 0), new Vector2(22, 22));
            }

            var val = MakeLabel("Value", badge.transform, res[i].value);
            if (val != null)
            {
                val.fontSize = 14; val.fontStyle = FontStyles.Bold;
                val.color = res[i].color;
                var lrt = val.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(44, 0); lrt.offsetMax = new Vector2(-8, 0);
                val.alignment = TextAlignmentOptions.MidlineRight;
            }
        }
    }

    // ── MINIMAP ──────────────────────────────────────────────────────────────
    static void BuildMinimap(Transform hud)
    {
        var root = MakeGO("Minimap", hud);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-12, -248);
        rt.sizeDelta = new Vector2(160, 155);

        var border = MakeImage("Border", root.transform, KFUI + "panel-000.png");
        if (border != null) { StretchFull(border.gameObject); Slice9(border); border.color = new Color(0.2f, 0.13f, 0.07f, 0.97f); }

        var mapBg = MakeGO("MapBG", root.transform);
        var mbrt = mapBg.GetComponent<RectTransform>();
        mbrt.anchorMin = new Vector2(0.1f, 0.12f);
        mbrt.anchorMax = new Vector2(0.9f, 0.92f);
        mbrt.offsetMin = mbrt.offsetMax = Vector2.zero;
        var mbImg = mapBg.AddComponent<Image>();
        mbImg.color = new Color(0.25f, 0.52f, 0.22f, 1);
        mbImg.raycastTarget = false;

        var river = MakeGO("River", mapBg.transform);
        var rrt = river.GetComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0, 0.44f); rrt.anchorMax = new Vector2(1, 0.56f);
        rrt.offsetMin = rrt.offsetMax = Vector2.zero;
        var rImg = river.AddComponent<Image>();
        rImg.color = new Color(0.28f, 0.55f, 0.88f, 0.85f);
        rImg.raycastTarget = false;

        float[,] blds = { { 0.18f, 0.68f }, { 0.38f, 0.78f }, { 0.58f, 0.22f }, { 0.72f, 0.72f } };
        for (int i = 0; i < 4; i++)
        {
            var b = MakeGO("Bld_" + i, mapBg.transform);
            var brt = b.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(blds[i, 0], blds[i, 1]);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(10, 8);
            var bImg = b.AddComponent<Image>();
            bImg.color = new Color(0.55f, 0.38f, 0.22f, 1);
            bImg.raycastTarget = false;
        }

        var marker = MakeGO("PlayerMarker", mapBg.transform);
        var mrt = marker.GetComponent<RectTransform>();
        mrt.anchorMin = mrt.anchorMax = new Vector2(0.42f, 0.6f);
        mrt.pivot = new Vector2(0.5f, 0.5f);
        mrt.sizeDelta = new Vector2(8, 8);
        var mImg = marker.AddComponent<Image>();
        mImg.color = new Color(1f, 0.95f, 0.2f, 1);
        mImg.raycastTarget = false;

        var lbl = MakeLabel("MapLabel", root.transform, "Map");
        if (lbl != null)
        {
            lbl.fontSize = 11; lbl.color = new Color(0.8f, 0.7f, 0.5f, 0.8f);
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 0);
            lrt.pivot = new Vector2(0.5f, 1);
            lrt.anchoredPosition = new Vector2(0, -2);
            lrt.sizeDelta = new Vector2(0, 16);
            lbl.alignment = TextAlignmentOptions.Center;
        }
    }

    // ── DIALOGUE BOX ─────────────────────────────────────────────────────────
    static void BuildDialogueBox(Transform hud)
    {
        var root = MakeGO("DialogueBox", hud);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0);
        rt.anchorMax = new Vector2(0.9f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 10);
        rt.sizeDelta = new Vector2(0, 130);

        var bg = MakeImage("BG", root.transform,
            SL_UI + "Dialouge UI/Premade dialog box medium.png");
        if (bg != null)
        {
            StretchFull(bg.gameObject);
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 1f;
        }

        // Portrait, name label, and dialogue text removed
        var arrow = MakeImage("ContinueArrow", root.transform, KICO + "arrowDown.png");
        if (arrow != null)
        {
            arrow.color = new Color(0.4f, 0.25f, 0.1f, 0.9f);
            SetAnchored(arrow.gameObject, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 16), new Vector2(18, 18));
        }
    }

    // ── HOTBAR ───────────────────────────────────────────────────────────────
    static void BuildHotbar(Transform hud)
    {
        var root = MakeGO("Hotbar", hud);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 150);
        rt.sizeDelta = new Vector2(448, 58);

        var bg = MakeImage("BG", root.transform, KUI + "panel_brown.png");
        if (bg != null) { StretchFull(bg.gameObject); Slice9(bg); bg.color = new Color(0.18f, 0.11f, 0.06f, 0.92f); }

        Color[] itemColors = {
            new Color(0.9f, 0.3f, 0.3f, 1),
            new Color(0.3f, 0.7f, 0.3f, 1),
            new Color(0.98f, 0.88f, 0.55f, 1),
            new Color(0.3f, 0.5f, 0.9f, 1),
            new Color(0.7f, 0.45f, 0.2f, 1),
            new Color(0.65f, 0.3f, 0.75f, 1),
            new Color(0.3f, 0.75f, 0.65f, 1),
            new Color(0, 0, 0, 0),
        };

        int activeSlot = 2;

        for (int i = 0; i < 8; i++)
        {
            bool isActive = i == activeSlot;
            float slotSize = isActive ? 52 : 46;
            float slotY    = isActive ? 4 : 0;
            float startX   = -216 + i * 54f;

            var slot = MakeGO("Slot_" + i, root.transform);
            var srt = slot.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(startX + 27, slotY);
            srt.sizeDelta = new Vector2(slotSize, slotSize);

            var slotBg = MakeImage("SlotBG", slot.transform,
                isActive ? KUI + "buttonSquare_beige.png" : KUI + "buttonSquare_brown.png");
            if (slotBg != null)
            {
                StretchFull(slotBg.gameObject); Slice9(slotBg);
                if (isActive) slotBg.color = new Color(0.98f, 0.88f, 0.55f, 1);
            }

            if (i < itemColors.Length && itemColors[i].a > 0.1f)
            {
                var item = MakeGO("Item", slot.transform);
                var irt = item.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.2f, 0.2f);
                irt.anchorMax = new Vector2(0.8f, 0.8f);
                irt.offsetMin = irt.offsetMax = Vector2.zero;
                var iImg = item.AddComponent<Image>();
                iImg.color = itemColors[i];
                iImg.raycastTarget = false;
            }

            var numLbl = MakeLabel("Num", slot.transform, (i + 1).ToString());
            if (numLbl != null)
            {
                numLbl.fontSize = 9;
                numLbl.color = new Color(0.7f, 0.6f, 0.45f, 0.8f);
                var lrt = numLbl.GetComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 0);
                lrt.pivot = new Vector2(0.5f, 0);
                lrt.anchoredPosition = new Vector2(0, 2);
                lrt.sizeDelta = new Vector2(0, 12);
                numLbl.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    // ── HELPERS ──────────────────────────────────────────────────────────────
    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static Image MakeImage(string name, Transform parent, string spritePath)
    {
        var go = MakeGO(name, parent);
        var img = go.AddComponent<Image>();
        img.raycastTarget = false;

        if (!string.IsNullOrEmpty(spritePath))
        {
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (spr != null)
            {
                img.sprite = spr;
            }
            else
            {
                // Try first sprite in the asset (spritesheet)
                var all = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                foreach (var a in all)
                    if (a is Sprite s) { img.sprite = s; break; }
                if (img.sprite == null)
                    Debug.LogWarning("[BuildFarmHUD] Sprite not found: " + spritePath);
            }
        }
        return img;
    }

    static TextMeshProUGUI MakeLabel(string name, Transform parent, string text)
    {
        var go = MakeGO(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 14;
        tmp.color = new Color(0.95f, 0.9f, 0.78f, 1);
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }

    static void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Slice9(Image img)
    {
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;
    }

    static void SetAnchored(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void SetRTExact(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
}
