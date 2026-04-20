using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.InputSystem;

/// Allows dragging the borderless window by clicking on non-UI areas.
public class WindowDragger : MonoBehaviour
{
    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT p);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr i,
        int x, int y, int cx, int cy, uint f);
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);

    [StructLayout(LayoutKind.Sequential)] struct POINT { public int x, y; }
    [StructLayout(LayoutKind.Sequential)] struct RECT  {
        public int left, top, right, bottom; }

    const uint SWP_NOSIZE   = 0x0001;
    const uint SWP_NOZORDER = 0x0004;

    bool    isDragging;
    POINT   dragStart;
    RECT    winStart;
    IntPtr  hwnd;

    void Start()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        hwnd = GetActiveWindow();
#endif
    }

    void Update()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
                GetCursorPos(out dragStart);
                GetWindowRect(hwnd, out winStart);
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame) isDragging = false;

        if (isDragging)
        {
            GetCursorPos(out POINT cur);
            int dx = cur.x - dragStart.x;
            int dy = cur.y - dragStart.y;
            SetWindowPos(hwnd, IntPtr.Zero,
                winStart.left + dx, winStart.top + dy, 0, 0,
                SWP_NOSIZE | SWP_NOZORDER);
        }
#endif
    }
}
