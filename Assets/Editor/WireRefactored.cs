using UnityEngine;
using UnityEditor;

public class WireRefactored
{
    public static void Execute()
    {
        var gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager not found!"); return; }

        // ── 1. FocusSystem 音频连线 ──
        var focus = gm.GetComponent<FocusSystem>();
        if (focus != null)
        {
            var alarmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_Thruster.ogg");
            if (alarmClip != null) focus.alarmClip = alarmClip;

            var audioSrc = gm.GetComponent<AudioSource>();
            if (audioSrc != null) focus.audioSource = audioSrc;

            EditorUtility.SetDirty(focus);
            Debug.Log("FocusSystem audio wired.");
        }

        // ── 2. UIManager progressRing + cycleDots 连线 ──
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found!"); return; }

        var uiManagerGO = canvas.GetComponent<UIManager>() ?? canvas.GetComponentInChildren<UIManager>();

        // UIManager 可能挂在其他地方，全局找
        var uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager == null) { Debug.LogWarning("UIManager not found!"); return; }

        // progressRing
        var ringTransform = canvas.transform.Find("CenterContainer/ProgressRing");
        if (ringTransform != null)
        {
            uiManager.progressRing = ringTransform.GetComponent<UnityEngine.UI.Image>();
            Debug.Log("UIManager.progressRing wired.");
        }

        // cycleDots
        var dotsParent = canvas.transform.Find("CycleDots");
        if (dotsParent != null)
        {
            var dots = new GameObject[dotsParent.childCount];
            for (int i = 0; i < dotsParent.childCount; i++)
                dots[i] = dotsParent.GetChild(i).gameObject;
            uiManager.cycleDots = dots;
            Debug.Log($"UIManager.cycleDots wired: {dots.Length} dots.");
        }

        EditorUtility.SetDirty(uiManager);

        // ── 3. 禁用 PomodoroTimer（架构已合并进 FocusSystem，不再需要） ──
        var pomodoroTimer = gm.GetComponent<PomodoroTimer>();
        if (pomodoroTimer != null)
        {
            pomodoroTimer.enabled = false;
            EditorUtility.SetDirty(pomodoroTimer);
            Debug.Log("PomodoroTimer disabled (logic merged into FocusSystem).");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("WireRefactored complete!");
    }
}
