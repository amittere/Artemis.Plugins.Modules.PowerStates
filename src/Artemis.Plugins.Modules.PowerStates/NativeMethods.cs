using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Artemis.Plugins.Modules.PowerStates
{
    [SupportedOSPlatform("windows")]
    internal class NativeMethods
    {
        #region Power Setting Notification

        internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x0;

        internal const int PBT_APMSUSPEND = 0x4;
        internal const int PBT_APMRESUMEAUTOMATIC = 0x12;
        internal const int PBT_POWERSETTINGCHANGE = 0x8013;

        internal static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern nint RegisterPowerSettingNotification(nint hWnd, [In] Guid PowerSettingGuid, uint Flags);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern bool UnregisterPowerSettingNotification(nint handle);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern nint RegisterSuspendResumeNotification(nint hWnd, uint Flags);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern bool UnregisterSuspendResumeNotification(nint handle);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        #endregion

        #region WndProc

        internal const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        internal const int WM_CREATE = 0x0001;
        internal const int WM_POWERBROADCAST = 0x0218;

        internal delegate nint WndProc(nint hWnd, uint Msg, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WNDCLASS
        {
            public uint style;
            public nint lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public nint hInstance;
            public nint hIcon;
            public nint hCursor;
            public nint hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern ushort RegisterClassW([In] ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterClassW(
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpClassName,
            nint hInstance);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern nint CreateWindowExW(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            nint hWndParent,
            nint hMenu,
            nint hInstance,
            nint lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyWindow(nint hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern nint DefWindowProcW(nint hWnd, uint msg, nint wParam, nint lParam);

        #endregion
    }
}
