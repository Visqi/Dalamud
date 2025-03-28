using System.Collections.Generic;
using System.Reflection;

using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.IoC;
using Dalamud.IoC.Internal;
using Dalamud.Plugin.Services;

using CSJobGaugeManager = FFXIVClientStructs.FFXIV.Client.Game.JobGaugeManager;

namespace Dalamud.Game.ClientState.JobGauge;

/// <summary>
/// This class converts in-memory Job gauge data to structs.
/// </summary>
[PluginInterface]
[ServiceManager.EarlyLoadedService]
#pragma warning disable SA1015
[ResolveVia<IJobGauges>]
#pragma warning restore SA1015
internal class JobGauges : IServiceType, IJobGauges
{
    private Dictionary<Type, JobGaugeBase> cache = [];

    [ServiceManager.ServiceConstructor]
    private JobGauges()
    {
    }

    /// <inheritdoc/>
    public unsafe IntPtr Address => (nint)(&CSJobGaugeManager.Instance()->EmptyGauge);

    /// <inheritdoc/>
    public T Get<T>() where T : JobGaugeBase
    {
        // This is cached to mitigate the effects of using activator for instantiation.
        // Since the gauge itself reads from live memory, there isn't much downside to doing this.
        if (!this.cache.TryGetValue(typeof(T), out var gauge))
        {
            gauge = this.cache[typeof(T)] = (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { this.Address }, null);
        }

        return (T)gauge;
    }
}
