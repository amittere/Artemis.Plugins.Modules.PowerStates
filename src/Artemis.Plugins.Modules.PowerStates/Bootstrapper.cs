using Artemis.Core;
using System;

namespace Artemis.Plugins.Modules.PowerStates;

public class Bootstrapper : PluginBootstrapper
{
    public override void OnPluginEnabled(Plugin plugin)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new NotImplementedException("Plugin only supports Windows");
        }
    }
}
