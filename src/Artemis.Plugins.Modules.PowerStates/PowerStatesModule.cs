using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.ColorScience;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.PowerStates.DataModels;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using Serilog;
using Microsoft.Win32;

namespace Artemis.Plugins.Modules.PowerStates
{
    [PluginFeature(Name = "Power States", AlwaysEnabled = true)]
    [SupportedOSPlatform("windows")]
    public class PowerStatesModule : Module<PowerStatesDataModel>
    {
        private ILogger _logger;

        private string _className = "Artemis.Plugins.Modules.PowerStates";
        private NativeMethods.WndProc _wndProc;
        private IntPtr _windowHandle;
        private IntPtr _suspendResumeNotificationHandle;
        private IntPtr _powerSettingNotificationHandle;

        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new List<IModuleActivationRequirement>();

        public PowerStatesModule(ILogger logger)
        {
            _logger = logger;
            _wndProc = WindowProc;
        }

        public override void Enable()
        {
            RegisterPowerSettingNotification();
        }

        public override void Disable()
        {
            UnregisterPowerSettingNotification();
        }

        public override void Update(double deltaTime)
        {
        }

        #region Power Setting Notifications

        private void RegisterPowerSettingNotification()
        {
            SystemEvents.InvokeOnEventsThread(() =>
            {
                // Create simple invisible window to process WM_POWERBROADCAST messages
                var wndClass = new NativeMethods.WNDCLASS();
                wndClass.lpszClassName = _className;
                wndClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc);

                var atom = NativeMethods.RegisterClassW(ref wndClass);
                if (atom == 0 && Marshal.GetLastWin32Error() != NativeMethods.ERROR_CLASS_ALREADY_EXISTS)
                {
                    _logger.Error("Failed RegisterClassW: {0}", Marshal.GetLastWin32Error());
                    return;
                }

                _windowHandle = NativeMethods.CreateWindowExW(0, _className, String.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (_windowHandle == IntPtr.Zero)
                {
                    _logger.Error("Failed CreateWindowExW: {0}", Marshal.GetLastWin32Error());
                    return;
                }

                _powerSettingNotificationHandle = NativeMethods.RegisterPowerSettingNotification(
                    _windowHandle, NativeMethods.GUID_CONSOLE_DISPLAY_STATE, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                if (_powerSettingNotificationHandle ==  IntPtr.Zero)
                {
                    _logger.Error("Failed RegisterPowerSettingNotification: {0}", Marshal.GetLastWin32Error());
                }

                _suspendResumeNotificationHandle = NativeMethods.RegisterSuspendResumeNotification(
                    _windowHandle, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                if (_suspendResumeNotificationHandle == IntPtr.Zero)
                {
                    _logger.Error("Failed RegisterSuspendResumeNotification: {0}", Marshal.GetLastWin32Error());
                }
            });
        }

        private void UnregisterPowerSettingNotification()
        {
            SystemEvents.InvokeOnEventsThread(() =>
            {
                if (_suspendResumeNotificationHandle != IntPtr.Zero)
                {
                    NativeMethods.UnregisterSuspendResumeNotification(_suspendResumeNotificationHandle);
                    _suspendResumeNotificationHandle = IntPtr.Zero;
                }

                if (_powerSettingNotificationHandle != IntPtr.Zero)
                {
                    NativeMethods.UnregisterPowerSettingNotification(_powerSettingNotificationHandle);
                    _powerSettingNotificationHandle = IntPtr.Zero;
                }

                if (_windowHandle != IntPtr.Zero)
                {
                    NativeMethods.DestroyWindow(_windowHandle);
                    _windowHandle = IntPtr.Zero;
                }

                NativeMethods.UnregisterClassW(_className, IntPtr.Zero);
            });
        }

        private IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            _logger.Debug("Received message: msg = {0}, wParam = {1}, lParam = {2}", msg, wParam, lParam);
            switch (msg)
            {
                case NativeMethods.WM_CREATE:
                    return 0;

                case NativeMethods.WM_POWERBROADCAST:
                    if (wParam == NativeMethods.PBT_POWERSETTINGCHANGE)
                    {
                        var setting = Marshal.PtrToStructure<NativeMethods.POWERBROADCAST_SETTING>(lParam);
                        switch (setting.Data)
                        {
                            case 0:
                                DataModel.DisplayState = DisplayState.MonitorOff;
                                break;

                            case 1:
                                DataModel.DisplayState = DisplayState.MonitorOn;
                                break;

                            case 2:
                                DataModel.DisplayState = DisplayState.MonitorDimmed;
                                break;
                        }
                    }
                    else if (wParam == NativeMethods.PBT_APMSUSPEND)
                    {
                        DataModel.PowerState = PowerState.Suspended;
                    }
                    else if (wParam == NativeMethods.PBT_APMRESUMEAUTOMATIC)
                    {
                        DataModel.PowerState = PowerState.Active;
                    }
                    return 1; // TRUE

                default:
                    return NativeMethods.DefWindowProcW(hwnd, msg, wParam, lParam);
            }
        }

        #endregion
    }
}
