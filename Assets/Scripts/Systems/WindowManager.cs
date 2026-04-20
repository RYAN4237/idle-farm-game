using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// Windows-native transparent, always-on-top, no-taskbar window manager.
/// Attach to a persistent GameObject in any scene.
/// Matches the Rusty's Retirement window behavior.
public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance { get; private set; }

    [Header("Window Settings")]
    public bool  alwaysOnTop      = true;
    public bool  transparent      = true;
    public bool  removeTaskbar    = true;
    public bool  removeTitle      = true;
    public bool  clickThrough     = false; // enable for pure overlay mode

    [Header("Farm Layout (bottom strip)")]
    public bool  isFarmWindow     = true;
    public int   farmHeight       = 120;   // px, adjust to taste

#if UNITY_STANDALONE_WIN
    // ── Win32 API ─────────────────────────────────────────────────────
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool  SetWindowPos(IntPtr h, IntPtr hwa,
        int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] static extern int   GetWindowLong(IntPtr h, int nIndex);
    [DllImport("user32.dll")] static extern int   SetWindowLong(IntPtr h, int nIndex, int dw);
    [DllImport("user32.dll")] static extern bool  SetLayeredWindowAttributes(IntPtr h,
        uint crKey, byte alpha, uint flags);
    [DllImport("user32.dll")] static extern bool  ShowWindow(IntPtr h, int nCmdShow);
    [DllImport("user32.dll")] static extern int   GetSystemMetrics(int nIndex);

    const int GWL_STYLE   = -16;
    const int GWL_EXSTYLE = -20;
    const int WS_CAPTION  = 0x00C00000;
    const int WS_THICKFRAME  = 0x00040000;
    const int WS_MINIMIZEBOX = 0x00020000;
    const int WS_MAXIMIZEBOX = 0x00010000;
    const int WS_SYSMENU  = 0x00080000;
    const int WS_EX_LAYERED    = 0x00080000;
    const int WS_EX_TRANSPARENT= 0x00000020;
    const int WS_EX_TOOLWINDOW = 0x00000080; // hides from taskbar
    const int WS_EX_TOPMOST    = 0x00000008;
    const int LWA_COLORKEY  = 0x00000001;
    const int LWA_ALPHA     = 0x00000002;
    const int SW_SHOW       = 5;
    const uint SWP_NOSIZE   = 0x0001;
    const uint SWP_NOMOVE   = 0x0002;
    const uint SWP_SHOWWINDOW = 0x0040;
    const uint SWP_FRAMECHANGED = 0x0020;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    IntPtr _hwnd;
#endif

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        _hwnd = GetActiveWindow();
        Apply();
#endif
    }

    public void Apply()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (_hwnd == IntPtr.Zero) return;

        int style   = GetWindowLong(_hwnd, GWL_STYLE);
        int exStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);

        // Remove title bar and resize frame
        if (removeTitle)
            style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);

        // Layered = required for transparency
        exStyle |= WS_EX_LAYERED;

        if (removeTaskbar) exStyle |= WS_EX_TOOLWINDOW;
        if (clickThrough)  exStyle |= WS_EX_TRANSPARENT;

        SetWindowLong(_hwnd, GWL_STYLE,   style);
        SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle);

        // Transparent background (key color = black, Unity cam bg = (0,0,0,0))
        if (transparent)
            SetLayeredWindowAttributes(_hwnd, 0x00000000, 255, LWA_COLORKEY);

        // Position: always on top, snap to bottom of screen
        if (isFarmWindow)
        {
            int screenW = GetSystemMetrics(0); // SM_CXSCREEN
            int screenH = GetSystemMetrics(1); // SM_CYSCREEN
            SetWindowPos(_hwnd, HWND_TOPMOST,
                0, screenH - farmHeight, screenW, farmHeight,
                SWP_FRAMECHANGED | SWP_SHOWWINDOW);
        }
        else if (alwaysOnTop)
        {
            SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
        }

        ShowWindow(_hwnd, SW_SHOW);
        Debug.Log($"[WindowManager] Applied: transparent={transparent}, topmost={alwaysOnTop}, taskbar={!removeTaskbar}");
#endif
    }

    // ── Runtime helpers ───────────────────────────────────────────────
    public void SetClickThrough(bool enable)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        clickThrough = enable;
        int ex = GetWindowLong(_hwnd, GWL_EXSTYLE);
        if (enable) ex |=  WS_EX_TRANSPARENT;
        else        ex &= ~WS_EX_TRANSPARENT;
        SetWindowLong(_hwnd, GWL_EXSTYLE, ex);
#endif
    }

    public void MoveWindow(int x, int y, int w, int h)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        SetWindowPos(_hwnd, HWND_TOPMOST, x, y, w, h, SWP_FRAMECHANGED | SWP_SHOWWINDOW);
#endif
    }
}
