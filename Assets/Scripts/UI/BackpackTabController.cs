using UnityEngine;
using UnityEngine.UI;

/// Handles Seeds / Fishing Rods tab switching in the Backpack panel.
public class BackpackTabController : MonoBehaviour
{
    public Button seedsTabBtn;
    public Button fishTabBtn;
    public GameObject seedsPanel;
    public GameObject fishPanel;
    public Image seedsTabImg;
    public Image fishTabImg;

    static readonly UnityEngine.Color TabActive = new UnityEngine.Color(0.45f, 0.35f, 0.18f, 1f);
    static readonly UnityEngine.Color TabInact  = new UnityEngine.Color(0.30f, 0.22f, 0.10f, 1f);

    void Start()
    {
        seedsTabBtn?.onClick.AddListener(() => ShowTab(true));
        fishTabBtn?.onClick.AddListener(()  => ShowTab(false));
        ShowTab(true);
    }

    void ShowTab(bool seeds)
    {
        if (seedsPanel) seedsPanel.SetActive(seeds);
        if (fishPanel)  fishPanel.SetActive(!seeds);
        if (seedsTabImg) seedsTabImg.color = seeds ? TabActive : TabInact;
        if (fishTabImg)  fishTabImg.color  = seeds ? TabInact  : TabActive;
    }
}
