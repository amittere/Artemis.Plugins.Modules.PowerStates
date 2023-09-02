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

namespace Artemis.Plugins.Modules.PowerStates
{
    [PluginFeature(Name = "Power States", AlwaysEnabled = true)]
    public class PowerStatesModule : Module<PowerStatesDataModel>
    {
        private ManagementEventWatcher _eventWatcher;

        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new List<IModuleActivationRequirement>();

        public override void Enable()
        {
            RegisterEvents();
        }

        public override void Disable()
        {
            UnregisterEvents();
        }

        public override void Update(double deltaTime)
        {
        }

        private void RegisterEvents()
        {
            var query = new WqlEventQuery("Win32_PowerManagementEvent");
            var scope = new ManagementScope("root\\CIMV2");

            _eventWatcher = new ManagementEventWatcher(scope, query);
            _eventWatcher.EventArrived += OnEventArrived;
            _eventWatcher.Start();
        }

        private void UnregisterEvents()
        {
            if (_eventWatcher != null)
            {
                _eventWatcher.Stop();
            }
        }

        private void OnEventArrived(object sender, EventArrivedEventArgs e)
        {
            foreach (PropertyData data in e.NewEvent.Properties)
            {
                if (data == null || data.Value == null) continue;

                // Respond to power state change based on value.
                // https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-powermanagementevent
                switch (data.Value.ToString())
                {
                    // Entering Suspend (4)
                    case "4":
                        DataModel.PowerState = PowerState.Suspended;
                        break;

                    // Resume from Suspend (7)
                    // Resume Automatic (18)
                    case "7":
                    case "18":
                        DataModel.PowerState = PowerState.Active;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
