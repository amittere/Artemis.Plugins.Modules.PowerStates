using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.PowerStates.DataModels
{
    public class PowerStatesDataModel : DataModel
    {
        public PowerState PowerState { get; set; }
        public DisplayState DisplayState { get; set; }
    }

    public enum PowerState
    {
        Active,
        Suspended
    }

    public enum DisplayState
    {
        MonitorOn,
        MonitorOff,
        MonitorDimmed
    }
}
