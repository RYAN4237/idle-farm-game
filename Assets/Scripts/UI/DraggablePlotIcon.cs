using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// 侧边栏里的 Farm Plot 图标
/// 点击后进入放置模式，然后在地图上点击格子放置
public class DraggablePlotIcon : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual")]
    public Image iconImage;
    public Color normalColor    = new Color(0.25f, 0.55f, 0.25f, 1f);
    public Color highlightColor = new Color(0.35f, 0.80f, 0.35f, 1f);
    public Color activeColor    = new Color(0.95f, 0.75f, 0.15f, 1f); // 选中时变黄

    private bool _isSelected;

    void Start()
    {
        if (iconImage == null) iconImage = GetComponent<Image>();
        SetColor(normalColor);
    }

    void Update()
    {
        // 如果放置管理器退出了放置模式（右键取消），同步状态
        if (_isSelected && PlacementManager.Instance != null && !PlacementManager.Instance.IsPlacing)
        {
            _isSelected = false;
            SetColor(normalColor);
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!_isSelected) SetColor(highlightColor);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!_isSelected) SetColor(normalColor);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (PlacementManager.Instance == null) return;

        if (_isSelected)
        {
            // 再次点击取消
            _isSelected = false;
            SetColor(normalColor);
            PlacementManager.Instance.StopPlacing();
        }
        else
        {
            _isSelected = true;
            SetColor(activeColor);
            PlacementManager.Instance.StartPlacing();
        }
    }

    void SetColor(Color c)
    {
        if (iconImage != null) iconImage.color = c;
    }
}
