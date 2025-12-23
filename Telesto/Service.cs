using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Diagnostics;

namespace Telesto
{

    internal class Service
    {

        [PluginService] public IDalamudPluginInterface pi { get; private set; }
        [PluginService] public ICommandManager cm { get; private set; }
        [PluginService] public IChatGui cg { get; private set; }
        [PluginService] public IGameGui gg { get; private set; }
        [PluginService] public IClientState cs { get; private set; }
        [PluginService] public IObjectTable ot { get; private set; }
        [PluginService] public IPartyList pl { get; private set; }
        [PluginService] public ITextureProvider tp { get; private set; }
        [PluginService] public ISigScanner ss { get; private set; }
        public int _ffxivPid = 0;

        public Service()
        {
            _ffxivPid = Process.GetCurrentProcess().Id;
        }

    }

}
