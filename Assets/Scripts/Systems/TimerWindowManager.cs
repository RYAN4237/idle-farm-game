using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// Timer App window: small draggable card anchored to bottom-right corner.
/// Frameless, always-on-top, transparent background, hidden from taskbar.
/// Attach to GameManager in TimerScene.
public class TimerWindowManager : MonoBehaviour
{
    [Header("Window Size (px)")]
    public int windowWidth  = 220;
    public int windowHeight = 300;

    [Header("Margin from screen edge (px)")]
    public int marginRight  = 20;
    public int marginBottom = 140; // sit above the Farm strip

    [Header("Allow drag to reposition")]
    public bool allowDrag = true;

    bool    _dragging;
    Vector2 _dragOffset;

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool   SetWindowPos(IntPtr h, IntPtr ins, int x, int y, int cx, int cy, uint f);
    [DllImport("user32.dll")] static extern int    SetWindowLong(IntPtr h, int n, int v);
    [DllImport("Dwmapi.dll")] static extern uint   DwmExtendFrameIntoClientArea(IntPtr h, ref MARGINS m);
    [DllImport("user32.dll")] static extern int    GetSystemMetrics(int n);
    [DllImport("user32.dll")] static extern bool   GetWindowRect(IntPtr h, out RECT r);

    [StructLayout(LayoutKind.Sequential)] struct MARGINS { public int left, right, top, bottom; }
    [StructLayout(LayoutKind.Sequential)] struct RECT    { public int left, top, right, bottom; }

    const int  GWL_STYLE        = -16;
    const int  GWL_EXSTYLE      = -20;
    const int  WS_POPUP         = unchecked((int)0x80000000);
    const int  WS_VISIBLE       = 0x10000000;
    const int  WS_EX_LAYERED    = 0x00080000;
    const int  WS_EX_TOPMOST    = 0x00000008;
    const int  WS_EX_TOOLWINDOW = 0x00000080;
    const uint SWP_NOSIZE       = 0x0001;
    const uint SWP_FRAMECHANGED = 0x0020;
    const uint SWP_SHOWWINDOW   = 0x0040;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    IntPtr _hwnd;

    void Start()
    {
#if !UNITY_EDITOR
        _hwnd = GetActiveWindow();

        // Remove title bar / border → borderless popup
        SetWindowLong(_hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        // Layered (alpha) + always on top + hide from taskbar
        SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);

        // DWM glass → allows true per-pixel transparency
        var m = new MARGINS { left = -1, right = -1, top = -1, bottom = -1 };
        DwmExtendFrameIntoClientArea(_hwnd, ref m);

        // Position: bottom-right corner, above Farm strip
        int sw = GetSystemMetrics(0); // screen width
        int sh = GetSystemMetrics(1); // screen height
        int x  = sw - windowWidth  - marginRight;
        int y  = sh - windowHeight - marginBottom;
        SetWindowPos(_hwnd, HWND_TOPMOST, x, y, windowWidth, windowHeight,
            SWP_FRAMECHANGED | SWP_SHOWWINDOW);

        Debug.Log($"[TimerWindow] Positioned ({x},{y}) size {windowWidth}x{windowHeight}");
#endif
    }

    void Update()
    {
        if (!allowDrag) return;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) { _dragging = true;  _dragOffset = Input.mousePosition; }
        if (Input.GetMouseButtonUp(0))   { _dragging = false; }

        if (_dragging && Input.GetMouseButton(0) && _hwnd != IntPtr.Zero)
        {
            Vector2 delta = (Vector2)Input.mousePosition - _dragOffset;
            if (GetWindowRect(_hwnd, out RECT r))
            {
                SetWindowPos(_hwnd, HWND_TOPMOST,
                    r.left + (int)delta.x,
                    r.top  - (int)delta.y, // screen Y is inverted vs Unity Y
                    0, 0,
                    SWP_NOSIZE | SWP_FRAMECHANGED);
            }
            _dragOffset = Input.mousePosition;
        }
#endif
    }
#else
    void Start()  { }
    void Update() { }
#endif
}
