using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradePanelController : MonoBehaviour
{
    public static UpgradePanelController Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TextMeshProUGUI _multiplierLabel;
    [SerializeField] private Transform _nodeContainer;

    private List<NodeSlotUI> _slots = new();

    struct NodeSlotUI
    {
        public UnlockNodeData Data;
        public Button Btn;
        public Image BG;
        public TextMeshProUGUI NameLabel;
        public TextMeshProUGUI CostLabel;
        public TextMeshProUGUI StatusLabel;
    }

    static readonly Color Locked    = new(0.25f, 0.22f, 0.18f, 0.95f);
    static readonly Color Available = new(0.18f, 0.45f, 0.22f, 0.95f);
    static readonly Color Unlocked  = new(0.12f, 0.30f, 0.55f, 0.95f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (_panel != null) _panel.SetActive(false);
        if (_openButton != null) _openButton.onClick.AddListener(OpenPanel);
        if (_closeButton != null) _closeButton.onClick.AddListener(ClosePanel);

        if (UnlockTree.Instance != null)
            UnlockTree.Instance.OnUnlockTreeStateChanged += RefreshAll;
        if (ResourceSystem.Instance != null)
            ResourceSystem.Instance.OnFocusPointsChanged += _ => RefreshAll();
    }

    public void OpenPanel()
    {
        if (_panel == null) return;
        _panel.SetActive(true);
        BuildNodeSlots();
        RefreshAll();
    }

    public void ClosePanel()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    void BuildNodeSlots()
    {
        if (UnlockTree.Instance == null) return;

        foreach (var s in _slots)
            if (s.Btn != null) Destroy(s.Btn.transform.parent.gameObject);
        _slots.Clear();

        var nodes = UnlockTree.Instance.GetAllNodes();
        foreach (var node in nodes)
        {
            if (node == null) continue;
            var slot = CreateSlot(node);
            _slots.Add(slot);
        }
    }

    NodeSlotUI CreateSlot(UnlockNodeData node)
    {
        var go = new GameObject(node.NodeId);
        go.transform.SetParent(_nodeContainer, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 60);

        var bg = go.AddComponent<Image>();
        bg.color = Locked;

        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(6, 6, 4, 4);
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var nameGO = new GameObject("Name");
        nameGO.transform.SetParent(go.transform, false);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = node.DisplayName;
        nameTMP.fontSize = 11;
        nameTMP.color = Color.white;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.raycastTarget = false;
        var nameLE = nameGO.AddComponent<LayoutElement>();
        nameLE.preferredHeight = 16;

        var costGO = new GameObject("Cost");
        costGO.transform.SetParent(go.transform, false);
        var costTMP = costGO.AddComponent<TextMeshProUGUI>();
        costTMP.text = FormatCost(node.PointCost) + $"  (+{node.MultiplierGranted:F2}x)";
        costTMP.fontSize = 9;
        costTMP.color = new Color(1f, 0.85f, 0.2f, 1f);
        costTMP.alignment = TextAlignmentOptions.Center;
        costTMP.raycastTarget = false;
        var costLE = costGO.AddComponent<LayoutElement>();
        costLE.preferredHeight = 14;

        var statusGO = new GameObject("Status");
        statusGO.transform.SetParent(go.transform, false);
        var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
        statusTMP.fontSize = 9;
        statusTMP.alignment = TextAlignmentOptions.Center;
        statusTMP.raycastTarget = false;
        var statusLE = statusGO.AddComponent<LayoutElement>();
        statusLE.preferredHeight = 14;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        string capturedId = node.NodeId;
        btn.onClick.AddListener(() => OnNodeClicked(capturedId));

        return new NodeSlotUI
        {
            Data = node, Btn = btn, BG = bg,
            NameLabel = nameTMP, CostLabel = costTMP, StatusLabel = statusTMP
        };
    }

    void OnNodeClicked(string nodeId)
    {
        if (UnlockTree.Instance == null) return;
        UnlockTree.Instance.TryUnlockNode(nodeId);
    }

    void RefreshAll()
    {
        if (UnlockTree.Instance == null) return;

        foreach (var slot in _slots)
        {
            var state = UnlockTree.Instance.ComputeNodeState(slot.Data.NodeId);
            switch (state)
            {
                case UnlockTree.NodeState.Unlocked:
                    slot.BG.color = Unlocked;
                    slot.StatusLabel.text = "UNLOCKED";
                    slot.StatusLabel.color = new Color(0.6f, 0.9f, 1f);
                    slot.Btn.interactable = false;
                    break;
                case UnlockTree.NodeState.Available:
                    slot.BG.color = Available;
                    slot.StatusLabel.text = "AVAILABLE";
                    slot.StatusLabel.color = new Color(0.5f, 1f, 0.5f);
                    slot.Btn.interactable = true;
                    break;
                default:
                    slot.BG.color = Locked;
                    slot.StatusLabel.text = "LOCKED";
                    slot.StatusLabel.color = new Color(0.6f, 0.5f, 0.4f);
                    slot.Btn.interactable = false;
                    break;
            }
        }

        if (_multiplierLabel != null)
        {
            float mult = ResourceSystem.Instance?.GlobalMultiplier ?? 1f;
            _multiplierLabel.text = $"x{mult:F2}";
        }
    }

    static string FormatCost(float cost)
    {
        if (cost >= 1000000) return $"{cost / 1000000f:F1}M";
        if (cost >= 10000) return $"{cost / 1000f:F1}K";
        return $"{(int)cost}";
    }

    void OnDestroy()
    {
        if (UnlockTree.Instance != null)
            UnlockTree.Instance.OnUnlockTreeStateChanged -= RefreshAll;
        if (Instance == this) Instance = null;
    }
}
