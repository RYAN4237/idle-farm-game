using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// 种子格子点击处理 - 挂在每个Cell上
public class SeedCellButton : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler,
    UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string seedName;
    public int    seedCost;
    public bool   isLocked;

    private Image         _bg;
    private Color         _normalColor;
    private static Color  _selectedColor = new Color(0.83f, 0.94f, 0.63f, 1f);
    private static Color  _hoverColor    = new Color(0.96f, 0.98f, 0.88f, 1f);
    private static SeedCellButton _current;

    void Awake()
    {
        _bg = GetComponent<Image>();
        if (_bg != null) _normalColor = _bg.color;
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e)
    {
        if (!isLocked && _bg != null && _current != this)
            _bg.color = _hoverColor;
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e)
    {
        if (!isLocked && _bg != null && _current != this)
            _bg.color = _normalColor;
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData e)
    {
        if (isLocked) return;

        // Deselect previous
        if (_current != null && _current != this && _current._bg != null)
            _current._bg.color = _current._normalColor;

        _current = this;
        if (_bg != null) _bg.color = _selectedColor;

        // Start placement mode
        if (PlacementManager.Instance != null)
            PlacementManager.Instance.StartPlacing();

        // Update info bar
        var infoBar = GameObject.Find("UICanvas/ExpandablePanel/Content/Middle/GridWrap/InfoBar/T");
        if (infoBar != null)
        {
            var tmp = infoBar.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = $"{seedName}  •  $ {seedCost}  •  Click grid to plant";
        }

        Debug.Log($"[SeedCellButton] Selected: {seedName} (${seedCost})");
    }

    public static void ClearSelection()
    {
        if (_current != null && _current._bg != null)
            _current._bg.color = _current._normalColor;
        _current = null;
    }
}
