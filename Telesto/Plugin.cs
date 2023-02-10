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
using static Lumina.Data.Parsing.Uld.NodeData;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Interface;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using BattleChara = Dalamud.Game.ClientState.Objects.Types.BattleChara;
using Dalamud.Game.Text.SeStringHandling;
using System.Reflection;
using Telesto.Interop;
using Character = Dalamud.Game.ClientState.Objects.Types.Character;
using StructCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using Dalamud.Game.ClientState.JobGauge.Enums;
using static Telesto.Endpoint;
using System.IO;
using System.Net;
using static System.Net.WebRequestMethods;
using System.Threading.Tasks;
using Dalamud.Data;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;

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

        private class Notification
        {

            public int version { get; set; } = 1;
            public int id { get; set; } = 0;
            public string notificationid { get; set; } = "undefined";
            public string notificationtype { get; set; } = "undefined";
            public object payload { get; set; } = null;

        }

        private class PropertyChangeNotification
        {

            public string objectid { get; set; } = "";
            public string name { get; set; } = "";
            public string oldvalue { get; set; } = "";
            public string newvalue { get; set; } = "";

        }

        private class Combatant
        {

            public string displayname { get; set; }
            public string fullname { get; set; }
            public int order { get; set; }
            public uint jobid { get; set; }
            public byte level { get; set; }
            public string actor { get; set; }

            public string job
            {
                get
                {
                    switch (jobid)
                    {
                        // JOBS
                        case 33: return "AST";
                        case 23: return "BRD";
                        case 25: return "BLM";
                        case 36: return "BLU";
                        case 38: return "DNC";
                        case 32: return "DRK";
                        case 22: return "DRG";
                        case 37: return "GNB";
                        case 31: return "MCH";
                        case 20: return "MNK";
                        case 30: return "NIN";
                        case 19: return "PLD";
                        case 35: return "RDM";
                        case 39: return "RPR";
                        case 34: return "SAM";
                        case 28: return "SCH";
                        case 40: return "SGE";
                        case 27: return "SMN";
                        case 21: return "WAR";
                        case 24: return "WHM";
                        // CRAFTERS
                        case 14: return "ALC";
                        case 10: return "ARM";
                        case 9: return "BSM";
                        case 8: return "CRP";
                        case 15: return "CUL";
                        case 11: return "GSM";
                        case 12: return "LTW";
                        case 13: return "WVR";
                        // GATHERERS
                        case 17: return "BTN";
                        case 18: return "FSH";
                        case 16: return "MIN";
                        // CLASSES
                        case 26: return "ACN";
                        case 5: return "ARC";
                        case 6: return "CNJ";
                        case 1: return "GLA";
                        case 4: return "LNC";
                        case 3: return "MRD";
                        case 2: return "PUG";
                        case 29: return "ROG";
                        case 7: return "THM";
                    }
                    return "";
                }
            }

            public string role
            {
                get
                {
                    switch (jobid)
                    {
                        // JOBS
                        case 19: return "Tank";
                        case 21: return "Tank";
                        case 32: return "Tank";
                        case 37: return "Tank";
                        case 24: return "Healer";
                        case 28: return "Healer";
                        case 33: return "Healer";
                        case 40: return "Healer";
                        case 20: return "DPS";
                        case 22: return "DPS";
                        case 23: return "DPS";
                        case 25: return "DPS";
                        case 27: return "DPS";
                        case 30: return "DPS";
                        case 31: return "DPS";
                        case 34: return "DPS";
                        case 35: return "DPS";
                        case 36: return "DPS";
                        case 38: return "DPS";
                        case 39: return "DPS";
                        // CRAFTERS
                        case 8: return "Crafter";
                        case 9: return "Crafter";
                        case 10: return "Crafter";
                        case 11: return "Crafter";
                        case 12: return "Crafter";
                        case 13: return "Crafter";
                        case 14: return "Crafter";
                        case 15: return "Crafter";
                        // GATHERERS
                        case 16: return "Gatherer";
                        case 17: return "Gatherer";
                        case 18: return "Gatherer";
                        // CLASSES
                        case 1: return "Tank";
                        case 3: return "Tank";
                        case 6: return "Healer";
                        case 26: return "Healer";
                        case 2: return "DPS";
                        case 4: return "DPS";
                        case 5: return "DPS";
                        case 7: return "DPS";
                        case 29: return "DPS";
                    }
                    return "";
                }
            }

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
        internal ChatGui _cg { get; init; }
        private GameGui _gg { get; init; }
        private ClientState _cs { get; init; }
        private ObjectTable _ot { get; init; }
        private PartyList _pl { get; init; }
        private DataManager _dm { get; init; }
        private Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        private ManualResetEvent SendPendingEvent = new ManualResetEvent(false);
        private ManualResetEvent StopEvent = new ManualResetEvent(false);
        private Thread SendThread = null;
        private bool _pollForMemory = false;
        private bool _sendThreadRunning = false;
        private DateTime _sendLastTimestamp = DateTime.Now;

        private string _debugTeleTest = "";
        private bool _configOpen = false;
        private bool _loggedIn = false;
        private bool _funcPtrChatboxFound = false;
        private bool _waymarksObjFound = false;
        private bool _destroyDoodles = false;
        private Waymarks _waymarks = new Waymarks();

        private bool _cfgAutostartEndpoint;
        private bool _territoryChanged = true;
        private string _cfgHttpEndpoint = "";

        private int _reqServed = 0;
        internal int _sentResponses = 0;
        internal int _sentTelegrams = 0;
        private int _numDoodles = 0;
        internal static Regex rex = new Regex(@"\$\{(?<id>[^\}\{\$]*)\}");
        internal static Regex rexnum = new Regex(@"\$(?<id>[0-9]+)");
        internal static Regex rexlidx = new Regex(@"(?<name>[^\[]+)\[(?<index>.+?)\]");
        internal static Regex rexnump = new Regex(@"\[(?<index>.+?)\]\.(?<prop>[a-zA-Z]+)");
        private static MathParser mp = new MathParser();

        private delegate void PostCommandDelegate(IntPtr ui, IntPtr cmd, IntPtr unk1, byte unk2);
        private delegate void GetWaymarkDelegate(IntPtr pObj, IntPtr pData);
        private IntPtr _chatBoxModPtr = IntPtr.Zero;
        private IntPtr _waymarksObj = IntPtr.Zero;
        private PostCommandDelegate postCmdFuncptr = null;

        private Dictionary<string, Doodle> Doodles = new Dictionary<string, Doodle>();
        private Queue<PendingRequest> Requests = new Queue<PendingRequest>();
        private Queue<Tuple<string, string>> Sends = new Queue<Tuple<string, string>>();

        [PluginService]
        public static SigScanner TargetModuleScanner { get; private set; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] ObjectTable objectTable,
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] PartyList partylist,
            [RequiredVersion("1.0")] DataManager dataManager
        )
        {
            _pi = pluginInterface;
            _cm = commandManager;
            _cs = clientState;
            _ot = objectTable;
            _gg = gameGui;
            _cg = chatGui;
            _pl = partylist;
            _dm = dataManager;
            _cfg = _pi.GetPluginConfig() as Config ?? new Config();
            _pi.UiBuilder.Draw += DrawUI;
            _pi.UiBuilder.OpenConfigUi += OpenConfigUI;
            _cm.AddHandler("/telesto", new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Telesto configuration"
            });
            _cs.Login += _cs_Login;
            _cs.Logout += _cs_Logout;
            _cs.TerritoryChanged += _cs_TerritoryChanged;
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
            SendThread = new Thread(new ParameterizedThreadStart(SendThreadProc));
            SendThread.Name = "Telesto send thread";
            SendThread.Start(this);
        }

        private void _cs_TerritoryChanged(object sender, ushort e)
        {
            _territoryChanged = true;
        }

        private void ResetSigs()
        {
            _funcPtrChatboxFound = false;
            _waymarksObjFound = false;
        }

        private void FindSigs()
        {
            // sig from saltycog/ffxiv-startup-commands
            _chatBoxModPtr = SearchForSig("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
            if (_chatBoxModPtr != IntPtr.Zero)
            {
                postCmdFuncptr = Marshal.GetDelegateForFunctionPointer<PostCommandDelegate>(_chatBoxModPtr);
                _funcPtrChatboxFound = (postCmdFuncptr != null);
            }
            // sig from PunishedPineapple/WaymarkPresetPlugin
            _waymarksObj = SearchForStaticAddress("41 80 F9 08 7C BB 48 8D ?? ?? ?? 48 8D ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 94 C0 EB 19", 11);
            if (_waymarksObj != IntPtr.Zero)
            {
                _waymarksObjFound = (_waymarksObj != IntPtr.Zero);
            }
        }

        internal ImGuiScene.TextureWrap? GetTexture(uint id)
        {
            return _dm.GetImGuiTextureIcon(id);
        }

        private IntPtr SearchForSig(string sig)
        {
            return TargetModuleScanner.ScanText(sig);
        }

        private IntPtr SearchForStaticAddress(string sig, int offset)
        {
            return TargetModuleScanner.GetStaticAddressFromSig(sig, 11);
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

        internal void QueueSendTelegram(string url, string tele)
        {
            lock (Sends)
            {
                Sends.Enqueue(new Tuple<string, string>(url, tele));
                SendPendingEvent.Set();
            }
        }

        public void Dispose()
        {
            if (StopEvent != null)
            {
                StopEvent.Set();
            }
            try
            {
                if (_ep != null)
                {
                    _ep.Dispose();
                    _ep = null;
                }
            }
            catch (Exception)
            {
            }
            foreach (KeyValuePair<string, Doodle> kp in Doodles)
            {
                try
                {
                    kp.Value.Dispose();
                }
                catch (Exception)
                {
                }
            }
            _cm.RemoveHandler("/telesto");
            _cs.Logout -= _cs_Logout;
            _cs.Login -= _cs_Login;
            _pi.UiBuilder.Draw -= DrawUI;
            _pi.UiBuilder.OpenConfigUi -= OpenConfigUI;
            try
            {
                if (StopEvent != null)
                {
                    StopEvent.Dispose();
                    StopEvent = null;
                }
            }
            catch (Exception)
            {
            }
            try
            { 
                if (SendPendingEvent != null)
                {
                    SendPendingEvent.Dispose();
                    SendPendingEvent = null;
                }
            }
            catch (Exception)
            {
            }
        }

        private void OnCommand(string command, string args)
        {
            OpenConfigUI();
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
            if (_pollForMemory == true)
            {
                PollForMemoryChanges();
            }
            if (_configOpen == false)
            {
                DrawDoodles();
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
                ImGui.Text(String.Format("Pointers found: cmd={0} wmo={1}", _funcPtrChatboxFound, _waymarksObjFound));
                ImGui.Text(String.Format("Pointer: cmd={0:X} wmo={1:x}", _chatBoxModPtr, _waymarksObj));
                ImGui.Text(String.Format("Request queue size: {0}", queueSize));
                ImGui.Text(String.Format("Requests served: {0}", _reqServed));
                ImGui.Separator();
                lock (Sends)
                {
                    ImGui.Text(String.Format("Sent: resp={0} tele={1} ({2} queued, running={3}, last={4})", _sentResponses, _sentTelegrams, Sends.Count, _sendThreadRunning, _sendLastTimestamp));
                }
                lock (Subscriptions)
                {
                    int activesubs = Subscriptions.Count;
                    var types = (from ix in Subscriptions.Values select ix.type).Distinct();
                    string typesdesc = String.Join(", ", types);
                    ImGui.Text(String.Format("Active subs: {0} ({1})", activesubs, typesdesc));
                }
                ImGui.Text(String.Format("Active polls: mem={0}", _pollForMemory));
                if (ImGui.Button("Remove all subs"))
                {
                    lock (Subscriptions)
                    {
                        Subscriptions.Clear();
                    }
                }
                ImGui.Separator();
                ImGui.Text(String.Format("Doodles active: {0}", _numDoodles));
                if (ImGui.Button("Destroy all doodles"))
                {
                    _destroyDoodles = true;
                }
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
            DrawDoodles();
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
            if (_loggedIn == false || _funcPtrChatboxFound == false)
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

        public unsafe void RefreshWaymarks()
        {
            if (_loggedIn == false || _waymarksObjFound == false)
            {
                return;
            }
            try
            {
                lock (_waymarks)
                {
                    Marshal.PtrToStructure<Waymarks>(_waymarksObj + 0x1b0, _waymarks);
                }
            }
            catch (Exception)
            {
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
                case "enabledoodle":
                    return HandleEnableDoodle(d["payload"]);
                case "disabledoodle":
                    return HandleDisableDoodle(d["payload"]);
                case "disabledoodleregex":
                    return HandleDisableDoodleRegex(d["payload"]);
                case "subscribe":
                    return HandleSubscribe(d["payload"]);
                case "unsubscribe":
                    return HandleUnsubscribe(d["payload"]);
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
            AddonPartyList* pl = (AddonPartyList*)_gg.GetAddonByName("_PartyList", 1);
            IntPtr pla = _gg.FindAgentInterface(pl);
            List<Combatant> cbs = new List<Combatant>();
            Dictionary<string, PartyMember> pls = new Dictionary<string, PartyMember>();
            for (int i = 0; i < _pl.Length; i++)
            {
                pls[_pl[i].Name.TextValue] = _pl[i];
            }
            for (int i = 0; i < pl->MemberCount; i++)
            {
                IntPtr p = (pla + (0x14ca + 0xd8 * i));
                Utf8String s = pl->PartyMember[i].Name->NodeText;
                string dispname = UTF8StringToString(s);
                string fullname = Marshal.PtrToStringUTF8(p);
                if (dispname[0] == '\u0000')
                {
                    dispname = "";
                }
                uint jobid = 0;
                byte level = 0;
                string actor = "";
                if (pls.ContainsKey(fullname) == true)
                {
                    PartyMember pm = pls[fullname];
                    jobid = pm.ClassJob.Id;
                    level = pm.Level;
                    actor = String.Format("{0:x8}", (pm.GameObject != null ? pm.GameObject.ObjectId : pm.ObjectId));
                }
                else if (_cs.LocalPlayer != null && fullname == _cs.LocalPlayer.Name.TextValue)
                {
                    jobid = _cs.LocalPlayer.ClassJob.Id;
                    level = _cs.LocalPlayer.Level;
                    actor = String.Format("{0:x8}", _cs.LocalPlayer.ObjectId);
                }
                cbs.Add(new Combatant() { displayname = dispname, fullname = fullname, order = i + 1, jobid = jobid, level = level, actor = actor });
            }
            return cbs;
        }

        private object HandlePing()
        {
            return "pong";
        }

        internal GameObject GetEntityById(ulong id)
        {
            return _ot.SearchById(id);
        }

        internal GameObject GetEntityByName(string id)
        {
            foreach (GameObject go in _ot)
            {
                if (String.Compare(go.Name.TextValue, id, true) == 0)
                {
                    return go;
                }
            }
            return null;
        }

        internal Doodle GetDoodleByName(string id)
        {
            lock (Doodles)
            {
                if (Doodles.ContainsKey(id) == true)
                {
                    return Doodles[id];
                }
            }
            return null;
        }

        internal Waymark GetWaymarkByName(string id)
        {
            Waymark wm = null;
            lock (_waymarks)
            {
                RefreshWaymarks();
                switch (id != null ? id.ToLower() : "")
                {
                    case "a": wm = _waymarks.A; break;
                    case "b": wm = _waymarks.B; break;
                    case "c": wm = _waymarks.C; break;
                    case "d": wm = _waymarks.D; break;
                    case "1": wm = _waymarks.One; break;
                    case "2": wm = _waymarks.Two; break;
                    case "3": wm = _waymarks.Three; break;
                    case "4": wm = _waymarks.Four; break;
                }
                if (wm != null)
                {
                    return wm.Duplicate();
                }
            }
            return null;
        }

        public static string TranslateJob(string id)
        {
            switch (id)
            {
                // JOBS
                case "33": return "AST";
                case "23": return "BRD";
                case "25": return "BLM";
                case "36": return "BLU";
                case "38": return "DNC";
                case "32": return "DRK";
                case "22": return "DRG";
                case "37": return "GNB";
                case "31": return "MCH";
                case "20": return "MNK";
                case "30": return "NIN";
                case "19": return "PLD";
                case "35": return "RDM";
                case "39": return "RPR";
                case "34": return "SAM";
                case "28": return "SCH";
                case "40": return "SGE";
                case "27": return "SMN";
                case "21": return "WAR";
                case "24": return "WHM";
                // CRAFTERS
                case "14": return "ALC";
                case "10": return "ARM";
                case "9": return "BSM";
                case "8": return "CRP";
                case "15": return "CUL";
                case "11": return "GSM";
                case "12": return "LTW";
                case "13": return "WVR";
                // GATHERERS
                case "17": return "BTN";
                case "18": return "FSH";
                case "16": return "MIN";
                // CLASSES
                case "26": return "ACN";
                case "5": return "ARC";
                case "6": return "CNJ";
                case "1": return "GLA";
                case "4": return "LNC";
                case "3": return "MRD";
                case "2": return "PUG";
                case "29": return "ROG";
                case "7": return "THM";
            }
            return "";
        }

        public static string TranslateRole(string id)
        {
            switch (id)
            {
                // JOBS
                case "19": return "Tank";
                case "21": return "Tank";
                case "32": return "Tank";
                case "37": return "Tank";
                case "24": return "Healer";
                case "28": return "Healer";
                case "33": return "Healer";
                case "40": return "Healer";
                case "20": return "DPS";
                case "22": return "DPS";
                case "23": return "DPS";
                case "25": return "DPS";
                case "27": return "DPS";
                case "30": return "DPS";
                case "31": return "DPS";
                case "34": return "DPS";
                case "35": return "DPS";
                case "36": return "DPS";
                case "38": return "DPS";
                case "39": return "DPS";
                // CRAFTERS
                case "8": return "Crafter";
                case "9": return "Crafter";
                case "10": return "Crafter";
                case "11": return "Crafter";
                case "12": return "Crafter";
                case "13": return "Crafter";
                case "14": return "Crafter";
                case "15": return "Crafter";
                // GATHERERS
                case "16": return "Gatherer";
                case "17": return "Gatherer";
                case "18": return "Gatherer";
                // CLASSES
                case "1": return "Tank";
                case "3": return "Tank";
                case "6": return "Healer";
                case "26": return "Healer";
                case "2": return "DPS";
                case "4": return "DPS";
                case "5": return "DPS";
                case "7": return "DPS";
                case "29": return "DPS";
            }
            return "";
        }

        private string GetEntityProperty(GameObject go, string prop)
        {
            switch (prop.ToLower())
            {
                case "address":
                    return go.Address.ToString();
                case "name":
                    return go.Name.TextValue;
                case "job":
                    return TranslateJob(go is BattleChara ? ((BattleChara)go).ClassJob.Id.ToString() : "0");
                case "jobid":
                    return go is BattleChara ? ((BattleChara)go).ClassJob.Id.ToString() : "0";
                case "currenthp":
                    return go is BattleChara ? ((BattleChara)go).CurrentHp.ToString() : "0";
                case "currentmp":
                    return go is BattleChara ? ((BattleChara)go).CurrentMp.ToString() : "0";
                case "currentcp":
                    return go is BattleChara ? ((BattleChara)go).CurrentCp.ToString() : "0";
                case "currentgp":
                    return go is BattleChara ? ((BattleChara)go).CurrentGp.ToString() : "0";
                case "maxhp":
                    return go is BattleChara ? ((BattleChara)go).MaxHp.ToString() : "0";
                case "maxmp":
                    return go is BattleChara ? ((BattleChara)go).MaxMp.ToString() : "0";
                case "maxcp":
                    return go is BattleChara ? ((BattleChara)go).MaxCp.ToString() : "0";
                case "maxgp":
                    return go is BattleChara ? ((BattleChara)go).MaxGp.ToString() : "0";
                case "level":
                    return go is BattleChara ? ((BattleChara)go).Level.ToString() : "0";
                case "x":
                    return go.Position.X.ToString();
                case "y":
                    return go.Position.Y.ToString();
                case "z":
                    return go.Position.Z.ToString();
                case "id":
                    return go.ObjectId.ToString("X8");
                case "heading":
                    return go.Rotation.ToString();
                case "targetid":
                    return go.TargetObjectId.ToString("X8");
                case "casttargetid":
                    return go is BattleChara ? ((BattleChara)go).CastTargetObjectId.ToString("X8") : "00000000";
                case "distance":
                    {
                        if (_cs.LocalPlayer != null)
                        {
                            float xdev = _cs.LocalPlayer.Position.X - go.Position.X;
                            float zdev = _cs.LocalPlayer.Position.Z - go.Position.Z;
                            return Math.Sqrt((xdev * xdev) + (zdev * zdev)).ToString();
                        }
                    }
                    break;
                case "role":
                    return TranslateRole(go is BattleChara ? ((BattleChara)go).ClassJob.Id.ToString() : "0");
            }
            return "";
        }

        public string ExpandVariables(Context ctx, string expr)
        {
            Match m, mx;
            string newexpr = expr;
            int i = 1;
            while (true)
            {
                m = rex.Match(newexpr);
                if (m.Success == false)
                {
                    m = rexnum.Match(newexpr);
                    if (m.Success == false)
                    {
                        break;
                    }
                }
                string x = m.Groups["id"].Value;
                string val = "";
                bool found = false;
                if (found == false)
                {
                    if (x == "_systemtime")
                    {
                        val = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
                        found = true;
                    }
                    else if (x == "_systemtimems")
                    {
                        val = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString();
                        found = true;
                    }
                    else if (x.IndexOf("_addr") == 0)
                    {
                        mx = rexlidx.Match(x);
                        if (mx.Success == true)
                        {
                            string idx = mx.Groups["index"].Value;
                            switch (idx.ToLower())
                            {
                                case "gameobject":
                                    val = ctx.go != null ? ctx.go.Address.ToString() : "0";
                                    break;
                            }
                            found = true;
                        }
                    }
                    else if (x.IndexOf("_sin") == 0)
                    {
                        mx = rexlidx.Match(x);
                        if (mx.Success == true)
                        {
                            string freqs = mx.Groups["index"].Value.Replace(",", ".");
                            if (double.TryParse(freqs, CultureInfo.InvariantCulture, out double freq) == true)
                            {
                                long ms = ((long)(DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0)).TotalMilliseconds);
                                val = ((float)Math.Round(Math.Sin(ms / 1000.0 * freq * 2.0 * Math.PI), 3)).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                val = "0";
                            }
                            found = true;
                        }
                    }
                    else if (x.IndexOf("_cos") == 0)
                    {
                        mx = rexlidx.Match(x);
                        if (mx.Success == true)
                        {
                            string freqs = mx.Groups["index"].Value.Replace(",", ".");
                            if (double.TryParse(freqs, CultureInfo.InvariantCulture, out double freq) == true)
                            {
                                long ms = ((long)(DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0)).TotalMilliseconds);
                                val = ((float)Math.Round(Math.Cos(ms / 1000.0 * freq * 2.0 * Math.PI), 3)).ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                val = "0";
                            }
                            found = true;
                        }
                    }
                    else if (x.IndexOf("_ffxiventity") == 0)
                    {
                        mx = rexnump.Match(x);
                        if (mx.Success == true)
                        {
                            string gindex = mx.Groups["index"].Value;
                            string gprop = mx.Groups["prop"].Value;
                            ulong honk;
                            GameObject go = null;
                            if (ulong.TryParse(gindex, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out honk) == true)
                            {
                                go = GetEntityById(honk);
                            }
                            else
                            {
                                go = GetEntityByName(gindex);
                            }
                            if (go != null)
                            {
                                val = GetEntityProperty(go, gprop).ToString();
                            }
                        }
                        found = true;
                    }
                }
                newexpr = newexpr.Substring(0, m.Index) + val + newexpr.Substring(m.Index + m.Length);
                i++;
            };
            return newexpr;
        }

        internal double EvaluateNumericExpression(Doodle dee, string expr)
        {
            return EvaluateNumericExpression(new Context() { doo = dee }, expr);
        }

        internal double EvaluateNumericExpression(GameObject goo, string expr)
        {
            return EvaluateNumericExpression(new Context() { go = goo }, expr);
        }

        internal double EvaluateNumericExpression(Context ctx, string expr)
        {
            string exp = ExpandVariables(ctx, expr == null ? "" : expr);
            lock (mp)
            {
                return mp.Parse(exp);
            }
        }

        internal Vector3 GetLocalPosition()
        {
            if (_cs.LocalPlayer == null)
            {
                return new Vector3();
            }
            return _cs.LocalPlayer.Position;
        }

        internal Vector3 TranslateToScreen(double x, double y, double z)
        {
            Vector2 tenp;
            _gg.WorldToScreen(
                new Vector3((float)x, (float)y, (float)z),
                out tenp
            );
            return new Vector3(tenp.X, tenp.Y, (float)z);
        }

        private object HandleEnableDoodle(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            Doodle dee = Doodle.Deserialize(d);
            dee.p = this;
            lock (Doodles)
            {
                Doodles[dee.Name] = dee;
            }
            return null;
        }

        private object HandleDisableDoodle(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            string id = d["name"].ToString();
            lock (Doodles)
            {
                if (Doodles.ContainsKey(id) == true)
                {
                    Doodle dee = Doodles[id];
                    Doodles.Remove(id);
                    dee.Dispose();
                }
            }
            return null;
        }

        private object HandleDisableDoodleRegex(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            Regex rex = new Regex(d["regex"].ToString());
            List<string> toRem = new List<string>();
            lock (Doodles)
            {
                foreach (string key in Doodles.Keys)
                {
                    if (rex.IsMatch(key) == true)
                    {
                        toRem.Add(key);
                    }
                }
                foreach (string key in toRem)
                {
                    Doodle dee = Doodles[key];
                    Doodles.Remove(key);
                    dee.Dispose();
                }
            }
            return null;
        }

        private object HandleSubscribe(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            string id = d["id"].ToString().ToLower();
            string type = d["type"].ToString().ToLower();
            string endpoint = d["endpoint"].ToString();
            switch (type)
            {
                case "memory":
                    {
                        string start = d["start"].ToString().ToLower();
                        string length = d["length"].ToString().ToLower();
                        string repr = d.ContainsKey("representation") == true ? d["representation"].ToString().ToLower() : "";
                        lock (Subscriptions)
                        {
                            Subscription s = null;
                            Subscriptions.Memory sm = null;
                            if (Subscriptions.ContainsKey(id) == true)
                            {
                                s = Subscriptions[id];
                                if (s.type != type)
                                {
                                    TurnSubscriptionTypeOff(s.type);
                                    s = null;
                                }
                            }
                            if (s == null)
                            {
                                sm = new Subscriptions.Memory();
                                Subscriptions[id] = sm;
                                s = sm;
                            }
                            else
                            {
                                sm = (Subscriptions.Memory)s;
                            }
                            sm.start = start;
                            sm.length = length;
                            sm.endpoint = endpoint;
                            sm.representation = repr;
                            s.id = id;
                            s.type = type;
                            s.p = this;
                            s.first = true;
                            TurnSubscriptionTypeOn(s.type);
                        }
                    }
                    break;
            }
            return null;
        }

        private object HandleUnsubscribe(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            string id = d["id"].ToString().ToLower();
            string gonetype = "";
            lock (Subscriptions)
            {
                if (Subscriptions.ContainsKey(id) == true)
                {
                    gonetype = Subscriptions[id].type;
                    Subscriptions.Remove(id);
                }
                if (gonetype != "")
                {
                    var rem = (from ix in Subscriptions.Values where ix.type == gonetype select ix).Count();
                    if (rem == 0)
                    {
                        TurnSubscriptionTypeOff(gonetype);
                    }
                }
            }
            return null;
        }

        private void TurnSubscriptionTypeOn(string id)
        {
            switch (id)
            {
                case "memory":
                    _pollForMemory = true;
                    break;
            }
        }

        private void TurnSubscriptionTypeOff(string id)
        {
            switch (id)
            {
                case "memory":
                    _pollForMemory = false;
                    break;
            }
        }

        private void DrawDoodles()
        {
            List<Doodle> ds = new List<Doodle>();
            List<Doodle> dead = new List<Doodle>();
            lock (Doodles)
            {
                ds.AddRange(Doodles.Values);
            }
            _numDoodles = ds.Count;
            if (_numDoodles > 0)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
                ImGuiHelpers.ForceNextWindowMainViewport();
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
                ImGui.Begin("TelestoChirpsHappily",
                    ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
                ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
            }
            bool ter = _territoryChanged;
            bool log = _loggedIn;
            bool blargimded = _cs.LocalPlayer != null ? _cs.LocalPlayer.CurrentHp == 0 : false;
            bool ded = _destroyDoodles;
            foreach (Doodle d in ds)
            {
                if (ded == true)
                {
                    dead.Add(d);
                    continue;
                }
                if (((d.ExpiresOn & Doodle.ExpiryTypeEnum.NotLoggedIn) != 0) && log == false)
                {
                    dead.Add(d);
                    continue;
                }
                if (((d.ExpiresOn & Doodle.ExpiryTypeEnum.OnZoneChange) != 0) && ter == true)
                {
                    dead.Add(d);
                    continue;
                }
                if (((d.ExpiresOn & Doodle.ExpiryTypeEnum.OnDeath) != 0) && blargimded == true)
                {
                    dead.Add(d);
                    continue;
                }
                if (d.Update() == false)
                {
                    dead.Add(d);
                    continue;
                }
                d.Draw();
            }
            if (dead.Count > 0)
            {
                lock (Doodles)
                {
                    foreach (Doodle d in dead)
                    {
                        if (Doodles.ContainsKey(d.Name) == true)
                        {
                            Doodles.Remove(d.Name);
                        }
                        d.Dispose();
                    }
                }
            }
            if (_numDoodles > 0)
            {
                ImGui.End();
                ImGui.PopStyleVar();
            }
            if (ter == true)
            {
                _territoryChanged = false;
            }
            if (ded == true)
            {
                _destroyDoodles = false;
            }
        }

        private unsafe void PollForMemoryChanges()
        {
            List<Subscription> firstsubs = new List<Subscription>();
            Context ctx = new Context();
            foreach (GameObject go in _ot)
            {
                ctx.go = go;
                foreach (Subscription s in Subscriptions.Values)
                {
                    if (s.type != "memory")
                    {
                        continue;
                    }
                    if (s.Refresh(ctx) == false)
                    {
                        continue;
                    }
                    if (s.first == true)
                    {
                        if (firstsubs.Contains(s) == false)
                        {
                            firstsubs.Add(s);
                        }
                        continue;
                    }
                    s.GetRepresentation(ctx, out string oldrep, out string newrep);
                    Subscriptions.Memory sm = (Subscriptions.Memory)s;
                    //_cg.Print(String.Format("{0} {1}: old {2} new {3}", go.Address, go.Name.TextValue, oldrep, newrep));
                    QueueSendTelegram(
                        s.endpoint,
                        JsonSerializer.Serialize<Notification>(
                            new Notification()
                            {
                                id = 1,
                                version = 1,
                                notificationid = s.id,
                                notificationtype = "memory",
                                payload = new PropertyChangeNotification()
                                {
                                    objectid = go.ObjectId.ToString("X8"),
                                    name = go.Name.TextValue,
                                    oldvalue = oldrep,
                                    newvalue = newrep
                                }
                            }
                        )
                    );
                }
            }
            foreach (Subscription s in firstsubs)
            {
                s.first = false;
            }
        }

        private Tuple<int, string> SendJson(string url, string json)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return new Tuple<int, string>((int)httpResponse.StatusCode, streamReader.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new Tuple<int, string>(-1, "");
            }
        }

        public void SendThreadProc(object o)
        {
            _sendThreadRunning = true;
            Plugin p = (Plugin)o;
            WaitHandle[] wh = new WaitHandle[2];
            wh[0] = p.StopEvent;
            wh[1] = p.SendPendingEvent;
            while (true)
            {
                switch (WaitHandle.WaitAny(wh, Timeout.Infinite))
                {
                    case 0:
                        {
                            _sendThreadRunning = false;
                            return;
                        }
                    case 1:
                        {
                            Tuple<string, string> send = null;
                            lock (Sends)
                            {
                                _sendLastTimestamp = DateTime.Now;
                                if (Sends.Count > 0)
                                {
                                    send = Sends.Dequeue();
                                }
                                if (Sends.Count == 0)
                                {
                                    SendPendingEvent.Reset();
                                }
                            }
                            if (send != null)
                            {
                                _sentTelegrams++;
                                Task t = new Task(() =>
                                {
                                    try
                                    {
                                        SendJson(send.Item1, send.Item2);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                );
                                t.Start();
                            }
                        }
                        break;
                }
            }            
        }

 
    }
}
