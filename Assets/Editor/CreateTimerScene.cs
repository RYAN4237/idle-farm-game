using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.IO;

/// Creates a standalone TimerScene with minimal UI
/// Timer App = independent product, not a Farm sub-feature
/// 
/// 关键设计：
/// - 独立场景，只包含 Timer 相关组件
/// - 小窗口 (220x300px)，右下角浮动
/// - 通过 SharedState.json 与 Farm 通信
public class CreateTimerScene
{
    [MenuItem("FocusFarm/1. Create Timer Scene")]
    public static void Execute()
    {
        Debug.Log("=====================================");
        Debug.Log("🎬 创建独立 Timer 场景");
        Debug.Log("=====================================");

        // 检查是否已存在，提示用户
        string scenePath = "Assets/Scenes/TimerScene.unity";
        if (File.Exists(scenePath))
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "场景已存在",
                "TimerScene.unity 已存在，是否覆盖？\n\n" +
                "点击 Yes 会删除现有场景并重新创建。",
                "Yes, 覆盖",
                "No, 取消"
            );
            if (!overwrite)
            {
                Debug.Log("❌ 用户取消，保留现有场景");
                return;
            }
        }

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Debug.Log("[1/7] 创建相机（透明背景）...");
        
        // ── 1. Camera (transparent bg) ────────────────────────────────
        var cam = new GameObject("Main Camera");
        var c = cam.AddComponent<Camera>();
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(0, 0, 0, 0); // transparent - 关键！
        c.orthographic = true;
        c.orthographicSize = 5;
        cam.tag = "MainCamera";

        Debug.Log("[2/7] 创建 Canvas（220x300 固定尺寸）...");
        
        // ── 2. Canvas (small Timer UI) ────────────────────────────────
        var canvasGO = new GameObject("TimerCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.referencePixelsPerUnit = 100;
        
        canvasGO.AddComponent<GraphicRaycaster>();

        Debug.Log("[3/7] 创建背景面板（半透明卡片）...");
        
        // Background panel (220x300) - 固定尺寸
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        
        // 关键：固定尺寸，居中
        bgRT.anchorMin = new Vector2(0.5f, 0.5f);
        bgRT.anchorMax = new Vector2(0.5f, 0.5f);
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = new Vector2(220, 300); // 固定尺寸
        bgRT.anchoredPosition = Vector2.zero;
        
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.92f); // 深色半透明背景
        
        // 圆角效果（可选，需要自定义 Sprite）
        // bgImg.sprite = ...; 

        Debug.Log("[4/7] 创建计时器显示...");
        
        // ── 3. Timer Display ──────────────────────────────────────────
        var timerGO = new GameObject("TimerText");
        timerGO.transform.SetParent(bgGO.transform, false);
        var timerRT = timerGO.AddComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0.5f, 0.6f);
        timerRT.anchorMax = new Vector2(0.5f, 0.6f);
        timerRT.pivot = new Vector2(0.5f, 0.5f);
        timerRT.sizeDelta = new Vector2(200, 60);
        timerRT.anchoredPosition = Vector2.zero;
        
        var timerTMP = timerGO.AddComponent<TextMeshProUGUI>();
        timerTMP.text = "25:00";
        timerTMP.fontSize = 48;
        timerTMP.fontStyle = FontStyles.Bold;
        timerTMP.alignment = TextAlignmentOptions.Center;
        timerTMP.color = new Color(0.95f, 0.95f, 0.95f); // 柔和白色

        Debug.Log("[5/7] 创建阶段标签...");
        
        // ── 4. Phase Label ────────────────────────────────────────────
        var phaseGO = new GameObject("PhaseLabel");
        phaseGO.transform.SetParent(bgGO.transform, false);
        var phaseRT = phaseGO.AddComponent<RectTransform>();
        phaseRT.anchorMin = new Vector2(0.5f, 0.78f);
        phaseRT.anchorMax = new Vector2(0.5f, 0.78f);
        phaseRT.pivot = new Vector2(0.5f, 0.5f);
        phaseRT.sizeDelta = new Vector2(150, 30);
        phaseRT.anchoredPosition = Vector2.zero;
        
        var phaseTMP = phaseGO.AddComponent<TextMeshProUGUI>();
        phaseTMP.text = "● 专注模式";
        phaseTMP.fontSize = 16;
        phaseTMP.alignment = TextAlignmentOptions.Center;
        phaseTMP.color = new Color(0.1f, 0.62f, 0.46f); // 专注绿色

        Debug.Log("[6/7] 创建操作按钮...");
        
        // ── 5. Action Button ──────────────────────────────────────────
        var btnGO = new GameObject("StartButton");
        btnGO.transform.SetParent(bgGO.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.25f);
        btnRT.anchorMax = new Vector2(0.5f, 0.25f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.sizeDelta = new Vector2(160, 45);
        btnRT.anchoredPosition = Vector2.zero;
        
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.52f, 0.75f); // 柔和蓝色
        var btn = btnGO.AddComponent<Button>();
        
        // 按钮颜色变化
        var colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.52f, 0.75f);
        colors.highlightedColor = new Color(0.2f, 0.6f, 0.85f);
        colors.pressedColor = new Color(0.1f, 0.42f, 0.65f);
        btn.colors = colors;

        var btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;
        
        var btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "开始专注";
        btnTMP.fontSize = 18;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.white;

        Debug.Log("[7/7] 配置系统组件...");
        
        // ── 6. Systems ────────────────────────────────────────────────
        var sysGO = new GameObject("TimerSystems");

        // FocusSystem (核心计时逻辑)
        var focus = sysGO.AddComponent<FocusSystem>();
        focus.focusDurationMinutes = 25f;
        focus.restDurationMinutes = 5f;
        focus.focusCompletionReward = 10f;

        // TimerStateWriter (写入 SharedState.json，与 Farm 通信)
        var stateWriter = sysGO.AddComponent<TimerStateWriter>();

        // TimerWindowManager (窗口定位：右下角 220x300)
        var winMgr = sysGO.AddComponent<TimerWindowManager>();
        winMgr.windowWidth = 220;
        winMgr.windowHeight = 300;
        winMgr.marginRight = 20;
        winMgr.marginBottom = 140; // 避开 Farm 底部条
        winMgr.allowDrag = true;

        // PomodoroTimer UI 控制器
        var pomoUI = sysGO.AddComponent<PomodoroTimer>();
        pomoUI.timerText = timerTMP;
        pomoUI.phaseLabelText = phaseTMP;
        pomoUI.actionButton = btn;
        pomoUI.actionButtonText = btnTMP;
        pomoUI.workColor = new Color(0.1f, 0.62f, 0.46f); // 专注绿
        pomoUI.restColor = new Color(0.37f, 0.54f, 0.87f); // 休息蓝
        pomoUI.enablePulseEffect = true;

        // ── 7. Save Scene ─────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, scenePath);
        
        Debug.Log("\n=====================================");
        Debug.Log($"✅ Timer 场景创建成功！");
        Debug.Log($"   路径: {scenePath}");
        Debug.Log($"   尺寸: 220x300px");
        Debug.Log($"   位置: 右下角浮动窗口");
        Debug.Log("=====================================");
        Debug.Log("\n下一步:");
        Debug.Log("1. 菜单 → FocusFarm → 🔍 检查构建状态");
        Debug.Log("2. 菜单 → FocusFarm → 🔨 仅构建 Apps");
        Debug.Log("3. 菜单 → FocusFarm → ▶ 仅启动 Apps");
        Debug.Log("=====================================\n");
        
        // 自动打开场景查看
        EditorSceneManager.OpenScene(scenePath);
    }
}
