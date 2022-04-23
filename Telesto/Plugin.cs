using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using FFXIVClientStructs.FFXIV.Client.System.String;

namespace Telesto
{

    // Telegram Service for Triggernometry Operations (TELESTO)
    public sealed class Plugin : IDalamudPlugin
    {

        private class Response
        {

            public int version { get; set; } = 1;
            public int id { get; set; } = 0;
            public object response { get; set; } = null;

        }

        private class Combatant
        {

            public string displayname { get; set; }
            public string fullname { get; set; }
            public int order { get; set; }

        }

        internal class PendingRequest : IDisposable
        {

            public AutoResetEvent ReadyEvent = new AutoResetEvent(false);

            public int Id { get; set; }
            public string Request { get; set; }
            public object Response { get; set; } = null;

            public void Dispose()
            {
                ReadyEvent.Dispose();
            }

        }

        private Parser _pr = null;
        private Endpoint _ep = null;
        internal Config _cfg = new Config();

        public string Name => "Telesto";

        private DalamudPluginInterface _pi { get; init; }
        private CommandManager _cm { get; init; }
        private ChatGui _cg { get; init; }
        private GameGui _gg { get; init; }
        private ClientState _cs { get; init; }
        private PartyList _pl { get; init; }

        private string _debugTeleTest = "";
        private bool _configOpen = false;
        private bool _loggedIn = false;
        private bool _funcPtrFound = false;

        private bool _cfgAutostartEndpoint;
        private string _cfgHttpEndpoint;

        private int _reqServed = 0;

        private delegate void PostCommandDelegate(IntPtr ui, IntPtr cmd, IntPtr unk1, byte unk2);
        private IntPtr _chatBoxModPtr = IntPtr.Zero;
        private PostCommandDelegate postCmdFuncptr = null;

        private Queue<PendingRequest> Requests = new Queue<PendingRequest>();

        [PluginService]
        public static SigScanner TargetModuleScanner { get; private set; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] Framework framework,
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] PartyList partylist
        )
        {
            _pi = pluginInterface;
            _cm = commandManager;
            _cs = clientState;
            _gg = gameGui;
            _cg = chatGui;
            _pl = partylist;
            _cfg = _pi.GetPluginConfig() as Config ?? new Config();
            _pi.UiBuilder.Draw += DrawUI;
            _pi.UiBuilder.OpenConfigUi += OpenConfigUI;
            _cm.AddHandler("/telesto", new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Telesto configuration"
            });
            _cs.Login += _cs_Login;
            _cs.Logout += _cs_Logout;
            if (_cs.IsLoggedIn == true)
            {
                _loggedIn = true;
                FindSigs();
            }
            _pr = new Parser();
            _ep = new Endpoint() { plug = this };
            if (_cfg.AutostartEndpoint == true)
            {
                _ep.Start();
            }
        }

        private void ResetSigs()
        {
            _funcPtrFound = false;
        }

        private void FindSigs()
        {
            // sig from saltycog/ffxiv-startup-commands
            _chatBoxModPtr = SearchForSig("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
            if (_chatBoxModPtr != IntPtr.Zero)
            {
                postCmdFuncptr = Marshal.GetDelegateForFunctionPointer<PostCommandDelegate>(_chatBoxModPtr);
                _funcPtrFound = (postCmdFuncptr != null);
            }
        }

        private IntPtr SearchForSig(string sig)
        {
            return TargetModuleScanner.ScanText(sig);
        }

        private void _cs_Logout(object sender, EventArgs e)
        {
            _loggedIn = false;
            ResetSigs();
        }

        private void _cs_Login(object sender, EventArgs e)
        {
            _loggedIn = true;
            ResetSigs();
            FindSigs();
        }

        internal string QueueTelegram(string tele)
        {
            using (PendingRequest pr = new PendingRequest() { Request = tele })
            {
                lock (Requests)
                {
                    Requests.Enqueue(pr);
                }
                pr.ReadyEvent.WaitOne();
                return JsonSerializer.Serialize<Response>(new Response() { id = pr.Id, response = pr.Response });
            }
        }

        public void Dispose()
        {
            if (_ep != null)
            {
                _ep.Dispose();
            }
            _cm.RemoveHandler("/telesto");
            _cs.Logout -= _cs_Logout;
            _cs.Login -= _cs_Login;
            _pi.UiBuilder.Draw -= DrawUI;
            _pi.UiBuilder.OpenConfigUi -= OpenConfigUI;
        }

        private void OnCommand(string command, string args)
        {
            _configOpen = true;
        }

        private void DrawUI()
        {
            PendingRequest pr = null;
            int queueSize = 0;
            try
            {
                lock (Requests)
                {
                    queueSize = Requests.Count;
                    if (queueSize > 0)
                    {
                        pr = Requests.Dequeue();
                    }
                }
                if (pr != null)
                {
                    pr.Response = ProcessRequest(pr);
                    pr.ReadyEvent.Set();
                    _reqServed++;
                }
            }
            catch (Exception ex)
            {
                _cg.PrintError(String.Format("Exception in telegram processing: {0}", ex.Message));
            }
            if (_configOpen == false)
            {
                return;
            }
            ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);
            ImGui.Begin("Telesto", ref _configOpen);
            ImGui.Separator();
            ImGui.BeginTabBar("Telesto_Main", ImGuiTabBarFlags.None);

            if (ImGui.BeginTabItem("Endpoint"))
            {
                ImGui.Checkbox("Start endpoint on launch", ref _cfgAutostartEndpoint);
                ImGui.InputText("HTTP POST endpoint", ref _cfgHttpEndpoint, 2048);
                ImGui.Separator();
                ImGui.Text(String.Format("Endpoint status: {0}", _ep.Status));
                ImGui.Text(String.Format("Status description: {0}", _ep.StatusDescription));
                ImGui.Separator();
                if (ImGui.Button("Save and start endpoint"))
                {
                    SaveConfig();
                    _ep.Start();
                }
                if (ImGui.Button("Stop endpoint"))
                {
                    _ep.Stop();
                }
                ImGui.Separator();
                if (ImGui.Button("Revert changes"))
                {
                    RevertConfig(_cfg);
                }
                if (ImGui.Button("Restore defaults"))
                {
                    RestoreConfig();
                }
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Debug"))
            {
                ImGui.Text(String.Format("Logged in: {0}", _loggedIn));
                ImGui.Text(String.Format("Function pointer found: {0}", _funcPtrFound));
                ImGui.Text(String.Format("Function pointer: {0:X}", _chatBoxModPtr));
                ImGui.Text(String.Format("Request queue size: {0}", queueSize));
                ImGui.Text(String.Format("Requests served: {0}", _reqServed));
                ImGui.Separator();
                ImGui.InputText("Telegram test input", ref _debugTeleTest, 2048);
                if (ImGui.Button("Process test input"))
                {
                    try
                    {
                        using (pr = new PendingRequest())
                        {
                            pr.Request = _debugTeleTest;
                            ProcessRequest(pr);
                        }
                        _debugTeleTest = "";
                    }
                    catch (Exception ex)
                    {
                        _cg.PrintError(String.Format("Exception in telegram test: {0}", ex.Message));
                    }
                }
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.Separator();
            if (ImGui.Button("Close"))
            {
                _configOpen = false;
            }
            ImGui.End();
        }

        private void OpenConfigUI()
        {
            _cfgAutostartEndpoint = _cfg.AutostartEndpoint;
            _cfgHttpEndpoint = _cfg.HttpEndpoint;
            _configOpen = true;
        }

        private void SaveConfig()
        {
            _cfg.AutostartEndpoint = _cfgAutostartEndpoint;
            _cfg.HttpEndpoint = _cfgHttpEndpoint;
            _pi.SavePluginConfig(_cfg);
        }

        private void RevertConfig(Config cfg)
        {
            _cfgAutostartEndpoint = _cfg.AutostartEndpoint;
            _cfgHttpEndpoint = _cfg.HttpEndpoint;
        }

        private void RestoreConfig()
        {
            RevertConfig(new Config());
        }

        public unsafe void SubmitCommand(string cmd)
        {
            if (_loggedIn == false || _funcPtrFound == false)
            {
                return;
            }
            AtkUnitBase* ptr = (AtkUnitBase*)_gg.GetAddonByName("ChatLog", 1);
            if (ptr != null && ptr->IsVisible == true)
            {
                IntPtr uiModule = _gg.GetUIModule();
                if (uiModule != IntPtr.Zero)
                {
                    using (Command payload = new Command(cmd))
                    {
                        IntPtr p = Marshal.AllocHGlobal(32);
                        try
                        {
                            Marshal.StructureToPtr(payload, p, false);
                            this.postCmdFuncptr(uiModule, p, IntPtr.Zero, 0);
                        }
                        catch (Exception)
                        {
                        }
                        Marshal.FreeHGlobal(p);
                    }
                }
            }
        }

        private void OpenMapRaw(ushort territoryId, uint mapId, int x, int y)
        {
            _gg.OpenMapWithMapLink(
                new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(territoryId, mapId, x, y)
            );
        }

        private void OpenMapWorld(ushort territoryId, uint mapId, float x, float y)
        {
            _gg.OpenMapWithMapLink(
                new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(territoryId, mapId, x, y)
            );
        }

        internal object ProcessRequest(PendingRequest pr)
        {
            string json = pr.Request;
            Dictionary<string, object> d = _pr.Parse(json);
            pr.Id = Convert.ToInt32(d["id"]);
            return ProcessTelegramDictionary(d);
        }

        private object ProcessTelegramDictionary(Dictionary<string, object> d)
        {
            switch (d["type"].ToString().ToLower())
            {
                case "printmessage":
                    return HandlePrintMessage(d["payload"]);
                case "printerror":
                    return HandlePrintError(d["payload"]);
                case "executecommand":
                    return HandleExecuteCommand(d["payload"]);
                case "openmap":
                    return HandleOpenMap(d["payload"]);
                case "bundle":
                    return HandleBundle(d["payload"]);
                case "getpartymembers":
                    return HandleGetPartyMembers();
                case "ping":
                    return HandlePing();
            }
            _cg.PrintError(String.Format("Unhandled Telesto telegram type '{0}'", d["type"].ToString()));
            return null;
        }

        private object HandlePrintMessage(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            _cg.Print(d["message"].ToString());
            return null;
        }

        private object HandlePrintError(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            _cg.PrintError(d["message"].ToString());
            return null;
        }

        private object HandleExecuteCommand(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            SubmitCommand(d["command"].ToString());
            return null;
        }

        private object HandleOpenMap(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            switch (d["coords"].ToString().ToLower())
            {
                case "raw":
                    OpenMapRaw(Convert.ToUInt16(d["territory"]), Convert.ToUInt32(d["map"]), Convert.ToInt32(d["x"]), Convert.ToInt32(d["y"]));
                    break;
                case "world":
                    OpenMapWorld(Convert.ToUInt16(d["territory"]), Convert.ToUInt32(d["map"]), Convert.ToSingle(d["x"]), Convert.ToSingle(d["y"]));
                    break;
                default:
                    throw new ArgumentException("'coords' should be either 'raw' or 'world'");
            }
            return null;
        }

        private object HandleBundle(object o)
        {
            List<object> obs = (List<object>)o;
            foreach (object ob in obs)
            {
                ProcessTelegramDictionary((Dictionary<string, object>)ob);
            }
            return null;
        }

        [StructLayout(LayoutKind.Explicit, Size = 24)]
        public unsafe struct PartyListCharacter
        {

            [FieldOffset(0x00)]
            public byte* ptr;

        }

        private unsafe string UTF8StringToString(Utf8String s)
        {
            return System.Text.Encoding.UTF8.GetString(s.IsUsingInlineBuffer != 0 ? s.InlineBuffer : s.StringPtr, (int)s.BufUsed);
        }

        private unsafe object HandleGetPartyMembers()
        {
            AddonPartyList *pl = (AddonPartyList *)_gg.GetAddonByName("_PartyList", 1);
            IntPtr pla = _gg.FindAgentInterface(pl);
            List<Combatant> cbs = new List<Combatant>();
            for (int i = 0; i < pl->MemberCount; i++)
            {
                IntPtr p = (pla + (0x101a + 0xd8 * i));
                Utf8String s = pl->PartyMember[i].Name->NodeText;
                cbs.Add(new Combatant() { displayname = UTF8StringToString(s), fullname = Marshal.PtrToStringUTF8(p), order = i + 1 });
            }
            return cbs;
        }

        private object HandlePing()
        {
            return "pong";
        }

    }

}
