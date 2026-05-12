using UnityEngine;
using UnityEngine.UI;

/// Controls the settings popup opened by the gear icon.
public class SettingsMenuController : MonoBehaviour
{
    public GameObject popup;
    public Button     closeBtn;
    public Button     quitBtn;

    void Start()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(HidePopup);
        if (quitBtn  != null) quitBtn.onClick.AddListener(OnQuit);
        if (popup    != null) popup.SetActive(false);
    }

    public void TogglePopup()
    {
        if (popup == null) return;
        popup.SetActive(!popup.activeSelf);
    }

    public void HidePopup()
    {
        if (popup != null) popup.SetActive(false);
    }

    void OnQuit()
    {
        var mgr = MacOSWindowManager.Instance;
        if (mgr != null) mgr.QuitGame();
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
