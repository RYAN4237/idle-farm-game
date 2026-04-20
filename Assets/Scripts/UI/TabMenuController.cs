using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// Manages the right-side tab menu: Seeds | Auto | Build
public class TabMenuController : MonoBehaviour
{
    public static TabMenuController Instance { get; private set; }

    [Header("Tab Buttons")]
    public Button seedsTab;
    public Button autoTab;
    public Button buildTab;

    [Header("Panels")]
    public GameObject seedsPanel;
    public GameObject autoPanel;
    public GameObject buildPanel;

    [Header("Tab Colors")]
    public Color activeTabColor   = new Color(0.15f, 0.38f, 0.25f, 1f);
    public Color inactiveTabColor = new Color(0.12f, 0.15f, 0.20f, 1f);
    public Color activeTextColor  = Color.white;
    public Color inactiveTextColor= new Color(0.55f, 0.55f, 0.55f, 1f);

    public enum Tab { Seeds, Auto, Build }
    public Tab CurrentTab { get; private set; } = Tab.Seeds;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        seedsTab?.onClick.AddListener(() => SwitchTab(Tab.Seeds));
        autoTab?.onClick.AddListener(()  => SwitchTab(Tab.Auto));
        buildTab?.onClick.AddListener(() => SwitchTab(Tab.Build));
        SwitchTab(Tab.Seeds);
    }

    public void SwitchTab(Tab tab)
    {
        CurrentTab = tab;

        // Show/hide panels
        if (seedsPanel) seedsPanel.SetActive(tab == Tab.Seeds);
        if (autoPanel)  autoPanel.SetActive(tab == Tab.Auto);
        if (buildPanel) buildPanel.SetActive(tab == Tab.Build);

        // Update tab button colors
        SetTabStyle(seedsTab, tab == Tab.Seeds);
        SetTabStyle(autoTab,  tab == Tab.Auto);
        SetTabStyle(buildTab, tab == Tab.Build);
    }

    void SetTabStyle(Button btn, bool isActive)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img) img.color = isActive ? activeTabColor : inactiveTabColor;
        var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (txt) txt.color = isActive ? activeTextColor : inactiveTextColor;
    }
}
