using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// Positions the Unity window as a slim bottom bar (like Rusty's Retirement).
/// Window sticks to the bottom of the screen, full width, ~200px tall.
/// No titlebar, always on top.
public class TransparentWindow : MonoBehaviour
{
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern int   SetWindowLong(IntPtr h, int n, uint v);
    [DllImport("user32.dll")] static extern bool  SetWindowPos(IntPtr h, IntPtr ins, int x,int y,int cx,int cy, uint f);
    [DllImport("user32.dll")] static extern bool  GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] static extern int   GetSystemMetrics(int n);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int left, top, right, bottom; }

    const int  GWL_STYLE        = -16;
    const int  GWL_EXSTYLE      = -20;
    const uint WS_POPUP         = 0x80000000;
    const uint WS_VISIBLE       = 0x10000000;
    const uint WS_EX_TOPMOST    = 0x00000008;
    const uint WS_EX_TOOLWINDOW = 0x00000080; // hides from taskbar
    const uint WS_EX_APPWINDOW  = 0x00040000;
    const uint SWP_SHOWWINDOW   = 0x0040;
    const uint SWP_FRAMECHANGED = 0x0020;
    const uint SWP_NOACTIVATE   = 0x0010;
    const int  SM_CXSCREEN      = 0;  // screen width
    const int  SM_CYSCREEN      = 1;  // screen height

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    [Header("Bar Settings")]
    [Tooltip("Height of the game bar in pixels")]
    public int barHeight = 200;
    [Tooltip("Offset from bottom of screen (0 = flush to bottom)")]
    public int bottomOffset = 0;
    [Tooltip("Override width (0 = full screen width)")]
    public int overrideWidth = 0;

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

        int screenW = GetSystemMetrics(SM_CXSCREEN);
        int screenH = GetSystemMetrics(SM_CYSCREEN);

        int winW = overrideWidth > 0 ? overrideWidth : screenW;
        int winH = barHeight;
        int winX = 0;
        int winY = screenH - winH - bottomOffset;

        // Borderless popup, always on top
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_TOPMOST | WS_EX_TOOLWINDOW);

        SetWindowPos(hwnd, HWND_TOPMOST,
            winX, winY, winW, winH,
            SWP_SHOWWINDOW | SWP_FRAMECHANGED | SWP_NOACTIVATE);

        Debug.Log($"[Window] Positioned: {winW}x{winH} at ({winX},{winY})");
    }
#endif
}
