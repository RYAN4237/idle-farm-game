using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CropShopUIController : MonoBehaviour
{
    private List<Button>     cropButtons   = new List<Button>();
    private List<GameObject> lockOverlays  = new List<GameObject>();
    private TextMeshProUGUI  autoFarmerBtnText;

    void Start()
    {
        BuildButtons();
        // Find AutoFarmer button text
        var afBtn = transform.parent?.Find("AutoFarmerBtn");
        if (afBtn != null)
            autoFarmerBtnText = afBtn.GetComponentInChildren<TextMeshProUGUI>();
    }

    void BuildButtons()
    {
        var shop = CropShop.Instance;
        if (shop == null) return;

        int count = shop.allCrops.Count;
        for (int i = 0; i < count; i++)
        {
            var crop = shop.allCrops[i];
            int col  = i % 2;
            int row  = i / 2;

            float xMin = 0.02f + col * 0.50f;
            float xMax = xMin  + 0.46f;
            float yMax = 0.85f - row * 0.44f;
            float yMin = yMax  - 0.40f;

            var btnGO = new GameObject("Crop_" + crop.cropName);
            btnGO.transform.SetParent(transform, false);

            var rect   = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = new Vector2(3f, 3f);
            rect.offsetMax = new Vector2(-3f, -3f);

            var img  = btnGO.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 1f);
            var btn  = btnGO.AddComponent<Button>();
            var cols = btn.colors;
            cols.highlightedColor = new Color(0.28f, 0.34f, 0.42f, 1f);
            cols.pressedColor     = new Color(0.14f, 0.18f, 0.22f, 1f);
            btn.colors = cols;

            int idx = i;
            btn.onClick.AddListener(() => OnCropClicked(idx));

            // Crop name
            MakeText(btnGO.transform, "Name",
                new Vector2(0f, 0.55f), Vector2.one,
                crop.cropName, 11f, crop.readyColor);

            // Stats
            MakeText(btnGO.transform, "Stats",
                Vector2.zero, new Vector2(1f, 0.58f),
                $"{crop.plantCost}FP/{crop.growthTime}s\n+{crop.harvestReward}FP",
                8f, new Color(0.75f, 0.75f, 0.75f, 1f));

            // Lock overlay
            var lockGO = new GameObject("Lock");
            lockGO.transform.SetParent(btnGO.transform, false);
            var lr = lockGO.AddComponent<RectTransform>();
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
            lockGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            MakeText(lockGO.transform, "LockText",
                Vector2.zero, Vector2.one,
                $"Lock\n{crop.unlockCost} FP", 10f, new Color(0.8f, 0.8f, 0.8f, 1f));

            cropButtons.Add(btn);
            lockOverlays.Add(lockGO);
        }

        RefreshUI();
    }

    void OnCropClicked(int index)
    {
        var shop = CropShop.Instance;
        if (shop == null) return;
        if (!shop.IsCropUnlocked(index))
        {
            if (shop.TryUnlockCrop(index)) RefreshUI();
        }
        else
        {
            shop.SelectCrop(index);
            RefreshUI();
        }
    }

    void RefreshUI()
    {
        var shop = CropShop.Instance;
        if (shop == null) return;

        for (int i = 0; i < cropButtons.Count; i++)
        {
            bool unlocked = shop.IsCropUnlocked(i);
            bool selected = shop.SelectedCrop == shop.allCrops[i];

            if (i < lockOverlays.Count) lockOverlays[i].SetActive(!unlocked);

            var img = cropButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = selected  ? new Color(0.15f, 0.38f, 0.22f, 1f) :
                            unlocked  ? new Color(0.18f, 0.22f, 0.28f, 1f) :
                                        new Color(0.12f, 0.14f, 0.18f, 1f);
        }

        // Refresh AutoFarmer button text
        RefreshAutoFarmerBtn();
    }

    void RefreshAutoFarmerBtn()
    {
        if (autoFarmerBtnText == null) return;
        var af = AutoFarmer.Instance;
        if (af == null) return;

        if (!af.CanUpgrade())
            autoFarmerBtnText.text = "Auto-Farmer MAX";
        else
            autoFarmerBtnText.text =
                $"Auto-Farmer Lv{af.CurrentLevel + 1}  {af.UpgradeCost()} FP";
    }

    void Update()
    {
        // Refresh every 60 frames
        if (Time.frameCount % 60 == 0) RefreshUI();
    }

    static void MakeText(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax, string text, float size, Color color)
    {
        var go   = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r    = go.AddComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = new Vector2(2f, 2f); r.offsetMax = new Vector2(-2f, -2f);
        var tmp  = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }
}
