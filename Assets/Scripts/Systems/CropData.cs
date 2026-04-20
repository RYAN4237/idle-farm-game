using UnityEngine;

/// Data definition for one crop type
[CreateAssetMenu(fileName = "CropData", menuName = "Farm/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Identity")]
    public string cropName    = "Wheat";
    public string icon        = "*";        // ASCII fallback icon

    [Header("Economy")]
    public float plantCost     = 10f;
    public float harvestReward = 20f;
    public float growthTime    = 10f;       // seconds

    [Header("Colors")]
    public Color emptyColor   = new Color(0.45f, 0.32f, 0.16f, 1f);
    public Color growingColor = new Color(0.20f, 0.58f, 0.20f, 1f);
    public Color readyColor   = new Color(0.30f, 1.00f, 0.30f, 1f);

    [Header("Unlock")]
    public float unlockCost   = 0f;   // 0 = available from start
    public int   unlockOrder  = 0;    // display order in shop
}
