using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform   expandablePanel;
    public float           panelAnimSpeed = 14f;

    [Header("Buttons")]
    public Button          seedButton;
    public Button          buildButton;
    public Button          upgradeButton;
    public Button          closeButton;

    [Header("Panel Title")]
    public TextMeshProUGUI panelTitle;

    // Legacy fields
    [HideInInspector] public TextMeshProUGUI timerText;
    [HideInInspector] public TextMeshProUGUI statusText;
    [HideInInspector] public TextMeshProUGUI focusPointsText;
    [HideInInspector] public TextMeshProUGUI incomeRateText;
    [HideInInspector] public TextMeshProUGUI sessionCountText;
    [HideInInspector] public TextMeshProUGUI startPauseButtonText;
    [HideInInspector] public TextMeshProUGUI durationLabelText;
    [HideInInspector] public Button          startPauseButton;
    [HideInInspector] public Button          resetButton;
    [HideInInspector] public Button          decreaseDurationBtn;
    [HideInInspector] public Button          increaseDurationBtn;
    [HideInInspector] public Image           progressRing;
    [HideInInspector] public Image           backgroundPanel;
    [HideInInspector] public GameObject[]    cycleDots;
    [HideInInspector] public Color           idleColor      = Color.grey;
    [HideInInspector] public Color           workingColor   = Color.green;
    [HideInInspector] public Color           restColor      = Color.cyan;
    [HideInInspector] public Color           completedColor = Color.yellow;

    // Panel: pivot=(1,0.5), anchor right
    // shown  → anchoredPosition.x = 0   (panel flush with right edge)
    // hidden → anchoredPosition.x = 210 (panel 210px off-screen right)
    const float SHOWN_X  =   0f;
    const float HIDDEN_X = 210f;

    bool   _open;
    string _tab;

    void Start()
    {
        // start hidden
        if (expandablePanel != null)
        {
            var p = expandablePanel.anchoredPosition;
            expandablePanel.anchoredPosition = new Vector2(HIDDEN_X, p.y);
        }

        if (seedButton    != null) seedButton.onClick.AddListener(   () => TogglePanel("SEEDS"));
        if (buildButton   != null) buildButton.onClick.AddListener(  () => TogglePanel("BUILD"));
        if (upgradeButton != null) upgradeButton.onClick.AddListener(() => TogglePanel("UPGRADES"));
        if (closeButton   != null) closeButton.onClick.AddListener(ClosePanel);
    }

    void Update()
    {
        if (expandablePanel == null) return;
        float tx  = _open ? SHOWN_X : HIDDEN_X;
        var   pos = expandablePanel.anchoredPosition;
        expandablePanel.anchoredPosition = new Vector2(
            Mathf.Lerp(pos.x, tx, Time.deltaTime * panelAnimSpeed), pos.y);
    }

    public void TogglePanel(string tab)
    {
        if (_open && _tab == tab) { ClosePanel(); return; }
        _open = true;
        _tab  = tab;
        if (panelTitle != null) panelTitle.text = tab;
    }

    public void ClosePanel()
    {
        _open = false;
        _tab  = "";
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
            PlacementManager.Instance.StopPlacing();
        SeedCellButton.ClearSelection();
    }
}
