using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Auto-Farmer panel content: shows current level, upgrade button, description
public class AutoPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI intervalText;
    public Button          upgradeButton;
    public TextMeshProUGUI upgradeButtonText;
    public TextMeshProUGUI fpDisplay;

    void OnEnable() => Refresh();

    void Update()
    {
        // Refresh FP display every frame
        if (fpDisplay != null && ResourceSystem.Instance != null)
            fpDisplay.text = $"FP: {Mathf.FloorToInt(ResourceSystem.Instance.FocusPoints)}";
    }

    public void Refresh()
    {
        var af = AutoFarmer.Instance;
        if (af == null) return;

        int level = af.CurrentLevel;

        if (level == 0)
        {
            levelText?.SetText("Auto-Farmer: OFF");
            descText?.SetText("Automatically harvests ready crops\nand re-plants empty plots.");
            intervalText?.SetText("Buy to activate");
        }
        else
        {
            string[] stars = { "", "★", "★★", "★★★" };
            levelText?.SetText($"Auto-Farmer Lv{level} {stars[Mathf.Min(level,3)]}");
            float interval = af.intervals[Mathf.Min(level-1, af.intervals.Length-1)];
            descText?.SetText("Auto harvests & plants\nevery few seconds.");
            intervalText?.SetText($"Speed: every {interval:F0}s");
        }

        if (af.CanUpgrade())
        {
            float cost = af.UpgradeCost();
            upgradeButton?.gameObject.SetActive(true);
            upgradeButtonText?.SetText($"Upgrade Lv{level+1}\n{cost} FP");
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() =>
                {
                    if (af.TryUpgrade()) Refresh();
                });
            }
        }
        else
        {
            upgradeButton?.gameObject.SetActive(false);
            upgradeButtonText?.SetText("MAX LEVEL");
        }
    }
}
