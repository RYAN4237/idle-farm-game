using UnityEngine;
using System.Runtime.InteropServices;
using System;

/// Renders Unity with transparent background using URP + Win32 DWM.
/// Attach to Main Camera.
public class WindowTransparency : MonoBehaviour
{
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern int   SetWindowLong(IntPtr h, int n, uint v);
    [DllImport("user32.dll")] static extern uint  GetWindowLong(IntPtr h, int n);
    [DllImport("user32.dll")] static extern bool  SetWindowPos(IntPtr h, IntPtr i, int x, int y, int cx, int cy, uint f);
    [DllImport("Dwmapi.dll")] static extern uint  DwmExtendFrameIntoClientArea(IntPtr h, ref MARGINS m);

    [StructLayout(LayoutKind.Sequential)]
    struct MARGINS { public int left,right,top,bottom; }

    const int  GWL_STYLE   = -16;
    const int  GWL_EXSTYLE = -20;
    const uint WS_POPUP      = 0x80000000;
    const uint WS_VISIBLE    = 0x10000000;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TOPMOST = 0x00000008;
    const uint WS_EX_TOOLWINDOW = 0x00000080;
    const uint SWP_FRAMECHANGED = 0x0020;
    const uint SWP_SHOWWINDOW   = 0x0040;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    [Header("Position & Size")]
    public int startX      = 0;
    public int startY      = 0;
    public int startWidth  = 1280;
    public int startHeight = 720;

    void Awake()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }
#endif
    }

    void Start()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        Apply();
#endif
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    void Apply()
    {
        var hwnd = GetActiveWindow();

        // Borderless popup
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // Layered + topmost + hide from taskbar
        SetWindowLong(hwnd, GWL_EXSTYLE,
            WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);

        // Extend DWM glass to whole window (needed for alpha transparency)
        var m = new MARGINS { left=-1, right=-1, top=-1, bottom=-1 };
        DwmExtendFrameIntoClientArea(hwnd, ref m);

        // Position window
        SetWindowPos(hwnd, HWND_TOPMOST,
            startX, startY, startWidth, startHeight,
            SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }
#endif
}
