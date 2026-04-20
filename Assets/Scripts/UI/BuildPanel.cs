using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Building/decoration panel — placeholder for now, will expand later
public class BuildPanel : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    void OnEnable()
    {
        if (titleText) titleText.text = "Buildings";
        if (descText)  descText.text  =
            "Place decorations and\nupgrades on your farm.\n\nComing soon!";
    }
}
