using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// macOS transparent bottom-bar window — Rusty Retirement style.
/// Attach to any persistent GameObject. Only active on macOS standalone builds.
/// Window: fixed at screen bottom, full width, no border, always on top, not movable.
public class MacOSWindowManager : MonoBehaviour
{
    public static MacOSWindowManager Instance { get; private set; }

    [Header("Bar Settings")]
    [Tooltip("Height of the game bar in screen pixels")]
    public int barHeight = 200;
    [Tooltip("Pixels from bottom of screen (0 = flush)")]
    public int bottomOffset = 0;

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
    // ── Objective-C runtime ──────────────────────────────────────────
    [DllImport("libobjc.dylib", EntryPoint = "objc_getClass")]
    static extern IntPtr GetClass(string name);

    [DllImport("libobjc.dylib", EntryPoint = "sel_registerName")]
    static extern IntPtr GetSel(string name);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr MsgSend(IntPtr obj, IntPtr sel);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr MsgSend_Bool(IntPtr obj, IntPtr sel, bool val);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr MsgSend_Int(IntPtr obj, IntPtr sel, int val);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr MsgSend_IntPtr(IntPtr obj, IntPtr sel, IntPtr val);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern void MsgSend_Rect(IntPtr obj, IntPtr sel, CGRect rect, bool display, bool animate);

    [DllImport("libobjc.dylib", EntryPoint = "objc_msgSend_stret")]
    static extern void MsgSend_GetRect(out CGRect r, IntPtr obj, IntPtr sel);

    [StructLayout(LayoutKind.Sequential)]
    struct CGRect
    {
        public double x, y, w, h;
        public CGRect(double x, double y, double w, double h)
        { this.x = x; this.y = y; this.w = w; this.h = h; }
    }

    // NSWindowStyleMask
    const int StyleBorderless          = 0;
    const int StyleFullSizeContentView = 1 << 15;

    // NSWindowLevel — floats above normal windows
    const int LevelFloating = 3;

    // NSWindowCollectionBehavior: CanJoinAllSpaces(1) | Stationary(16) | IgnoresCycle(64)
    const int CollectionBehavior = 1 | 16 | 64;

    IntPtr _win;

    IntPtr GetNSWindow()
    {
        var app = MsgSend(GetClass("NSApplication"), GetSel("sharedApplication"));
        var win = MsgSend(app, GetSel("mainWindow"));
        if (win == IntPtr.Zero)
        {
            var arr = MsgSend(app, GetSel("orderedWindows"));
            if (arr != IntPtr.Zero)
                win = MsgSend_Int(arr, GetSel("objectAtIndex:"), 0);
        }
        return win;
    }

    void ApplyMacWindow()
    {
        _win = GetNSWindow();
        if (_win == IntPtr.Zero) { Debug.LogWarning("[MacOS] NSWindow not found"); return; }

        // Screen dimensions (macOS Y is bottom-up)
        var screen = MsgSend(_win, GetSel("screen"));
        if (screen == IntPtr.Zero)
            screen = MsgSend(GetClass("NSScreen"), GetSel("mainScreen"));
        CGRect sf;
        MsgSend_GetRect(out sf, screen, GetSel("frame"));

        double winW = sf.w;
        double winH = barHeight;
        double winX = sf.x;
        double winY = sf.y + bottomOffset; // macOS origin = bottom-left

        // Borderless
        MsgSend_Int(_win, GetSel("setStyleMask:"), StyleBorderless | StyleFullSizeContentView);

        // Transparent
        var clear = MsgSend(GetClass("NSColor"), GetSel("clearColor"));
        MsgSend_IntPtr(_win, GetSel("setBackgroundColor:"), clear);
        MsgSend_Bool(_win, GetSel("setOpaque:"), false);
        MsgSend_Bool(_win, GetSel("setHasShadow:"), false);

        // Always on top, not movable
        MsgSend_Int(_win, GetSel("setLevel:"), LevelFloating);
        MsgSend_Bool(_win, GetSel("setMovable:"), false);

        // Hide from Mission Control / Exposé / Dock
        MsgSend_Int(_win, GetSel("setCollectionBehavior:"), CollectionBehavior);

        // Snap to bottom
        var frame = new CGRect(winX, winY, winW, winH);
        MsgSend_Rect(_win, GetSel("setFrame:display:animate:"), frame, true, false);

        Debug.Log($"[MacOS] Window set: {winW}x{winH} at ({winX},{winY})");
    }
#endif

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Camera: transparent black background (required for see-through)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        Invoke(nameof(ApplyMacWindow), 0.15f);
#endif
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
