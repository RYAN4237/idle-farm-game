using UnityEngine;
using TMPro;

/// Keeps the TopBar FP value in sync with ResourceSystem
public class TopBarFP : MonoBehaviour
{
    public TextMeshProUGUI valueText;

    void Update()
    {
        if (valueText == null) return;
        float fp = ResourceSystem.Instance != null ? ResourceSystem.Instance.FocusPoints : 0f;
        valueText.text = ((int)fp).ToString();
    }
}
