using Dalamud.Game;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using FFXIVClientStructs.FFXIV.Client.System.String;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Types;
using BattleChara = Dalamud.Game.ClientState.Objects.Types.IBattleChara;
using Telesto.Interop;
using static Telesto.Endpoint;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Game.NativeWrapper;

namespace Telesto
{

    // Telegram Service for Triggernometry Operations (TELESTO)
    public sealed class Plugin : IDalamudPlugin
    {

        internal class Response
        {

            public int version { get; set; } = 1;
            public int id { get; set; } = 0;
            public string source { get; set; } = "undefined";
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

        internal class Waymark
        {

            internal enum WaymarkEnum
            {
                A, B, C, D,
                One, Two, Three, Four,
            }
            
            internal bool Active;
            internal float X, Y, Z;

        }

        private class Combatant
        {

            public string displayname { get; set; }
            public string fullname { get; set; }
            public int order { get; set; }
            public uint jobid { get; set; }
            public byte level { get; set; }
            public string actor { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }

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
                        case 41: return "VPR";
                        case 42: return "PCT";
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
                        case 41: return "DPS";
                        case 42: return "DPS";
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
            public Response Response { get; set; } = null;

            public void Dispose()
            {
                ReadyEvent.Dispose();
            }

        }

        public string Version = "1.0.0.8";

        private Parser _pr = null;
        private Endpoint _ep = null;
        internal Config _cfg = new Config();

        public string Name => "Telesto";
      
        private Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        private ManualResetEvent SendPendingEvent = new ManualResetEvent(false);
        private ManualResetEvent StopEvent = new ManualResetEvent(false);
        private Thread SendThread = null;
        private bool _pollForMemory = false;
        private bool _sendThreadRunning = false;
        private DateTime _sendLastTimestamp = DateTime.Now;
        private float _adjusterX = 0.0f;

        private string _debugTeleTest = "";        
        private bool _loggedIn = false;
        private bool _funcPtrChatboxFound = false;
        private bool _destroyDoodles = false;
        private bool _destroyForms = false;
        private Dictionary<Waymark.WaymarkEnum, Waymark> _waymarks = new Dictionary<Waymark.WaymarkEnum, Waymark>();

        private bool _territoryChanged = true;

        private int _reqServed = 0;
        internal int _sentResponses = 0;
        internal int _sentTelegrams = 0;
        private int _numDoodles = 0;
        private int _numForms = 0;
        internal static Regex rex = new Regex(@"\$\{(?<id>[^\}\{\$]*)\}");
        internal static Regex rexnum = new Regex(@"\$(?<id>[0-9]+)");
        internal static Regex rexlidx = new Regex(@"(?<name>[^\[]+)\[(?<index>.+?)\]");
        internal static Regex rexnump = new Regex(@"\[(?<index>.+?)\]\.(?<prop>[a-zA-Z]+)");
        private static MathParser mp = new MathParser();

        private delegate void PostCommandDelegate(IntPtr ui, IntPtr cmd, IntPtr unk1, byte unk2);
        private delegate void GetWaymarkDelegate(IntPtr pObj, IntPtr pData);
        private IntPtr _chatBoxModPtr = IntPtr.Zero;
        private bool _waymarksAvailable = false;
        private PostCommandDelegate postCmdFuncptr = null;

        private Dictionary<string, Doodle> Doodles = new Dictionary<string, Doodle>();
        private Dictionary<string, Form> Forms = new Dictionary<string, Form>();
        private Queue<PendingRequest> Requests = new Queue<PendingRequest>();
        private Queue<Tuple<string, string>> Sends = new Queue<Tuple<string, string>>();
        private Dictionary<int, ISharedImmediateTexture> _textures = new Dictionary<int, ISharedImmediateTexture>();
        private Service _svc = null;

        [PluginService]
        public static ISigScanner TargetModuleScanner { get; private set; }

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            _svc = pluginInterface.Create<Service>();
            _cfg = _svc.pi.GetPluginConfig() as Config ?? new Config();
            _svc.pi.UiBuilder.Draw += DrawUI;
            _svc.pi.UiBuilder.OpenConfigUi += OpenConfigUI;
            _svc.pi.UiBuilder.OpenMainUi += OpenConfigUI;
            _svc.cm.AddHandler("/telesto", new CommandInfo(OnCommand)
            {
                HelpMessage = "Open Telesto configuration"
            });
            _svc.cs.Login += _cs_Login;
            _svc.cs.Logout += _cs_Logout;
            _svc.cs.TerritoryChanged += _cs_TerritoryChanged;
            _pr = new Parser();
            _ep = new Endpoint() { plug = this };
            LoadTextures();
            if (_svc.cs.IsLoggedIn == true)
            {
                _cs_Login();
            }
            SendThread = new Thread(new ParameterizedThreadStart(SendThreadProc));
            SendThread.Name = "Telesto send thread";
            SendThread.Start(this);
            _waymarks[Waymark.WaymarkEnum.A] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.B] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.C] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.D] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.One] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.Two] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.Three] = new Waymark();
            _waymarks[Waymark.WaymarkEnum.Four] = new Waymark();
        }

        private void _cs_TerritoryChanged(ushort e)
        {
            _territoryChanged = true;
        }

        private void LoadTextures()
        {
            _textures[1] = GetTexture(5);
        }

        private void UnloadTextures()
        {            
            _textures.Clear();
        }

        private void ResetSigs()
        {
            _funcPtrChatboxFound = false;
            _waymarksAvailable = false;
        }

        private void FindSigs()
        {            
            try
            {
                _chatBoxModPtr = SearchForSig("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F2 48 8B F9 45 84 C9");
            }
            catch (Exception)
            {                
            }
            if (_chatBoxModPtr != IntPtr.Zero)
            {
                postCmdFuncptr = Marshal.GetDelegateForFunctionPointer<PostCommandDelegate>(_chatBoxModPtr);
                _funcPtrChatboxFound = (postCmdFuncptr != null);
            }            
        }

        internal ISharedImmediateTexture GetTexture(uint id)
        {
            return _svc.tp.GetFromGameIcon(new GameIconLookup() { IconId = id });            
        }

        private IntPtr SearchForSig(string sig)
        {
            return TargetModuleScanner.ScanText(sig);
        }

        private IntPtr SearchForStaticAddress(string sig, int offset)
        {
            return TargetModuleScanner.GetStaticAddressFromSig(sig, 11);
        }

        private void _cs_Logout(int type, int code)
        {
            _loggedIn = false;
            ResetSigs();
            try
            {
                if (_ep != null)
                {
                    _ep.Stop();
                }
            }
            catch (Exception)
            {
            }
        }

        private void _cs_Login()
        {
            _loggedIn = true;
            ResetSigs();
            FindSigs();
            if (_cfg.AutostartEndpoint == true)
            {
                _ep.Start();
            }
        }

        internal static void KeepWindowInSight()
        {
            Vector2 pt = ImGui.GetWindowPos();
            Vector2 szy = ImGui.GetWindowSize();
            bool moved = false;
            Vector2 szx = ImGui.GetIO().DisplaySize;
            if (szy.X > szx.X || szy.Y > szx.Y)
            {
                szy.X = Math.Min(szy.X, szx.X);
                szy.Y = Math.Min(szy.Y, szx.Y);
                ImGui.SetWindowSize(szy);
            }
            if (pt.X < 0)
            {
                pt.X += (0.0f - pt.X) / 5.0f;
                moved = true;
            }
            if (pt.Y < 0)
            {
                pt.Y += (0.0f - pt.Y) / 5.0f;
                moved = true;
            }
            if (pt.X + szy.X > szx.X)
            {
                pt.X -= ((pt.X + szy.X) - szx.X) / 5.0f;
                moved = true;
            }
            if (pt.Y + szy.Y > szx.Y)
            {
                pt.Y -= ((pt.Y + szy.Y) - szx.Y) / 5.0f;
                moved = true;
            }
            if (moved == true)
            {
                ImGui.SetWindowPos(pt);
            }
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
                return JsonSerializer.Serialize<Response>(pr.Response);
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
            Doodles.Clear();
            Forms.Clear();
            _svc.cm.RemoveHandler("/telesto");
            _svc.cs.Logout -= _cs_Logout;
            _svc.cs.Login -= _cs_Login;
            _svc.pi.UiBuilder.Draw -= DrawUI;
            _svc.pi.UiBuilder.OpenMainUi -= OpenConfigUI;
            _svc.pi.UiBuilder.OpenConfigUi -= OpenConfigUI;
            UnloadTextures();
            SaveConfig();
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
                _svc.cg.PrintError(String.Format("Exception in telegram processing: {0}", ex.Message));
            }
            if (_pollForMemory == true)
            {
                PollForMemoryChanges();
            }
            if (_cfg.Opened == false)
            {
                DrawDoodles();
                DrawForms();
                return;
            }
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.496f, 0.058f, 0.323f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.496f, 0.058f, 0.323f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);
            bool open = true;
            if (ImGui.Begin(Name, ref open, ImGuiWindowFlags.NoCollapse) == false)
            {
                ImGui.End();
                ImGui.PopStyleColor(3);
                return;
            }
            if (open == false)
            {
                _cfg.Opened = false;
                ImGui.End();
                ImGui.PopStyleColor(3);
                return;
            }
            KeepWindowInSight();
            ImGuiStylePtr style = ImGui.GetStyle();
            Vector2 fsz = ImGui.GetContentRegionAvail();
            fsz.Y -= ImGui.GetTextLineHeight() + (style.ItemSpacing.Y * 2) + style.WindowPadding.Y;
            ImGui.BeginChild("TellyFrame", fsz);
            ImGui.BeginTabBar("Telesto_Main", ImGuiTabBarFlags.None);
            if (ImGui.BeginTabItem("Endpoint"))
            {
                ImGui.BeginChild("EndpointChild");
                if (_cfg.DismissUpgrade == false)
                {
                    Vector2 cps = ImGui.GetCursorPos();
                    IDalamudTextureWrap tx = _textures[1].GetWrapOrEmpty();
                    ImGui.Image(tx.Handle, new Vector2(tx.Width, tx.Height));
                    ImGui.SetCursorPos(new Vector2(cps.X + tx.Width + 10, cps.Y));
                    ImGui.TextWrapped("There is now a new, much easier and reliable way to get automarkers! No configuration needed, you don't even need ACT or Telesto; Lemegeton is everything you need in one easy to use Dalamud plugin!" + Environment.NewLine + Environment.NewLine + "You can find it in the same Dalamud repository you added to get this plugin (Telesto), so you're just a couple of clicks away from the next generation of automarkers - just head back to the plugin installer, find Lemegeton from the list, and enjoy!" + Environment.NewLine + Environment.NewLine);
                    Vector2 cps2 = ImGui.GetCursorPos();
                    ImGui.SetCursorPos(new Vector2(cps.X + tx.Width + 10, cps2.Y));
                    if (ImGui.Button("Open plugin installer") == true)
                    {
                        SubmitCommand("/xlplugins");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Lemegeton homepage") == true)
                    {
                        Task tsk = new Task(() =>
                        {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.FileName = @"https://github.com/paissaheavyindustries/Lemegeton";
                            p.Start();
                        });
                        tsk.Start();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Dismiss") == true)
                    {
                        _cfg.DismissUpgrade = true;
                    }
                    ImGui.Separator();
                }
                bool autoStart = _cfg.AutostartEndpoint;
                if (ImGui.Checkbox("Start endpoint automatically on login", ref autoStart) == true)
                {
                    _cfg.AutostartEndpoint = autoStart;
                }
                bool logged = _loggedIn;
                bool canstart = false;
                bool canstop = false;
                if (logged == true)
                {
                    switch (_ep.Status)
                    {
                        case StatusEnum.Started:
                            canstop = true;
                            break;
                        case StatusEnum.Stopped:
                            canstart = true;
                            break;
                    }
                }
                if (canstart == false)
                {
                    ImGui.BeginDisabled();
                }
                string endpoint = _cfg.HttpEndpoint;
                if (ImGui.InputText("HTTP POST endpoint", ref endpoint, 2048) == true)
                {
                    _cfg.HttpEndpoint = endpoint;
                }
                if (canstart == false)
                {
                    ImGui.EndDisabled();
                }
                ImGui.Separator();
                if (logged == false)
                {
                    ImGui.BeginDisabled();
                }
                if (logged == true && canstart == false)
                {
                    ImGui.BeginDisabled();
                }
                if (ImGui.Button("Start endpoint"))
                {
                    SaveConfig();
                    _ep.Start();
                }
                if (logged == true && canstart == false)
                {
                    ImGui.EndDisabled();
                }
                ImGui.SameLine();
                if (logged == true && canstop == false)
                {
                    ImGui.BeginDisabled();
                }
                if (ImGui.Button("Stop endpoint"))
                {
                    _ep.Stop();
                }
                if (logged == true && canstop == false)
                {
                    ImGui.EndDisabled();
                }
                if (logged == false)
                {
                    ImGui.EndDisabled();
                }
                ImGui.Separator();
                if (_loggedIn == false && _cfg.AutostartEndpoint == true)
                {
                    ImGui.Text(String.Format("Endpoint status: {0} (waiting for login)", _ep.Status));
                }
                else
                {
                    ImGui.Text(String.Format("Endpoint status: {0}", _ep.Status));
                }
                ImGui.BeginChildFrame(1, ImGui.GetContentRegionAvail());
                ImGui.TextWrapped(_ep.StatusDescription);
                ImGui.EndChildFrame();
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Debug"))
            {
                ImGui.BeginChild("DebugChild");
                if (ImGui.CollapsingHeader("State"))
                {
                    ImGui.Text(String.Format("Logged in: {0}", _loggedIn));
                    ImGui.Text(String.Format("Pointers found: cmd={0}", _funcPtrChatboxFound));
                    ImGui.Text(String.Format("Pointer: cmd={0:X} pla={1:X}", _chatBoxModPtr, GetPartyListAgent()));
                }
                if (ImGui.CollapsingHeader("Subscriptions"))
                {
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
                }
                if (ImGui.CollapsingHeader("Doodles"))
                {

                    ImGui.Text(String.Format("Doodles active: {0}", _numDoodles));
                    if (ImGui.Button("Test doodles"))
                    {
                        {
                            Dictionary<string, object> pms = new Dictionary<string, object>();
                            pms["name"] = "__telesto_test_doodle1";
                            pms["type"] = "line";
                            pms["r"] = "1";
                            pms["g"] = "1";
                            pms["b"] = "0";
                            pms["a"] = "1";
                            pms["expiresin"] = "10000";
                            pms["thickness"] = 2;
                            Dictionary<string, object> pos;
                            pos = new Dictionary<string, object>();
                            pos["coords"] = "screen";
                            pos["x"] = "${_screenwidth}-30";
                            pos["y"] = "${_screenheight}-30";
                            pms["start"] = pos;
                            pos = new Dictionary<string, object>();
                            pos["name"] = "${_ffxivplayer}";
                            pos["coords"] = "entity";
                            pms["end"] = pos;
                            Doodle dee = Doodle.Deserialize(pms);
                            dee.p = this;
                            lock (Doodles)
                            {
                                Doodles[dee.Name] = dee;
                            }
                        }
                        {
                            Dictionary<string, object> pms = new Dictionary<string, object>();
                            pms["name"] = "__telesto_test_doodle2";
                            pms["type"] = "circle";
                            pms["r"] = "1";
                            pms["g"] = "1";
                            pms["b"] = "0";
                            pms["a"] = "1";
                            pms["expiresin"] = "10000";
                            pms["radius"] = "2+${_cos[1]}";
                            pms["thickness"] = 2;
                            Dictionary<string, object> pos;
                            pos = new Dictionary<string, object>();
                            pos["coords"] = "entity";
                            pos["name"] = "${_ffxivplayer}";
                            pos["offsetx"] = "${_cos[2]}";
                            pos["offsetz"] = "${_sin[2]}";
                            pms["position"] = pos;
                            pms["system"] = "world";
                            Doodle dee = Doodle.Deserialize(pms);
                            dee.p = this;
                            lock (Doodles)
                            {
                                Doodles[dee.Name] = dee;
                            }
                        }
                    }
                    if (ImGui.Button("Destroy all doodles"))
                    {
                        _destroyDoodles = true;
                    }
                }
                if (ImGui.CollapsingHeader("Forms"))
                {
                    ImGui.Text(String.Format("Forms active: {0}", _numForms));
                    if (ImGui.Button("Test forms"))
                    {
                        {
                            Form f = new Form();
                            f.plug = this;
                            f.Title = "My form 1";
                            f.Id = "my_super_form_1";
                            f.Callback = "http://localhost:51423";
                            f.Elements.Add(new FormElements.Label() { Owner = f, Text = "This is a test form!" });
                            f.Elements.Add(new FormElements.Layout() { Owner = f, Action = FormElements.Layout.LayoutEnum.Break });
                            f.Elements.Add(new FormElements.Label() { Owner = f, Text = "Yes it is!" });
                            f.Elements.Add(new FormElements.Layout() { Owner = f, Action = FormElements.Layout.LayoutEnum.Separator });
                            f.Elements.Add(new FormElements.Checkbox() { Owner = f, Id = "truebox", Text = "Defaults to true", Value = "true" });
                            f.Elements.Add(new FormElements.Checkbox() { Owner = f, Id = "falsebox", Text = "Defaults to false", Value = "false" });
                            f.Elements.Add(new FormElements.Layout() { Owner = f, Action = FormElements.Layout.LayoutEnum.Separator });
                            f.Elements.Add(new FormElements.InputText() { Owner = f, Id = "textdata", Text = "Text input field", Value = "Meow" });
                            f.Elements.Add(new FormElements.InputInt() { Owner = f, Id = "intdata", Text = "Int input field", Value = "123", MinValue = 69, MaxValue = 420 });
                            f.Elements.Add(new FormElements.Layout() { Owner = f, Action = FormElements.Layout.LayoutEnum.Separator });
                            f.Elements.Add(new FormElements.Button() { Owner = f, Id = "nyaa", Text = "Submit", Value = "normal", Action = FormElements.Button.ActionEnum.Submit });
                            f.Elements.Add(new FormElements.Button() { Owner = f, Id = "nyaa", Text = "Submit mindfully", Value = "mindful", Action = FormElements.Button.ActionEnum.Submit });
                            f.Elements.Add(new FormElements.Layout() { Owner = f, Action = FormElements.Layout.LayoutEnum.Separator });
                            f.Elements.Add(new FormElements.Button() { Owner = f, Text = "Cancel", Action = FormElements.Button.ActionEnum.Cancel });
                            lock (Forms)
                            {
                                Forms[f.Id] = f;
                            }
                        }
                    }
                    if (ImGui.Button("Destroy all forms"))
                    {
                        _destroyForms = true;
                    }
                }
                if (ImGui.CollapsingHeader("Telegrams"))
                {
                    lock (Sends)
                    {
                        ImGui.Text(String.Format("Sent: resp={0} tele={1} ({2} queued, running={3}, last={4})", _sentResponses, _sentTelegrams, Sends.Count, _sendThreadRunning, _sendLastTimestamp));
                    }
                    ImGui.Text(String.Format("Request queue size: {0}", queueSize));
                    ImGui.Text(String.Format("Requests served: {0}", _reqServed));
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
                            _svc.cg.PrintError(String.Format("Exception in telegram test: {0}", ex.Message));
                        }
                    }
                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
            ImGui.EndChild();
            ImGui.Separator();
            Vector2 fp = ImGui.GetCursorPos();
            ImGui.SetCursorPosY(fp.Y + 2);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
            ImGui.Text("v" + Version + " (" + _svc._ffxivPid + ")");
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(new Vector2(_adjusterX, fp.Y));
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.496f, 0.058f, 0.323f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.496f, 0.058f, 0.323f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            if (ImGui.Button("Discord") == true)
            {
                Task tx = new Task(() =>
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = @"https://discord.gg/6f9MY55";
                    p.Start();
                });
                tx.Start();
            }
            ImGui.SameLine();
            if (ImGui.Button("GitHub") == true)
            {
                Task tx = new Task(() =>
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = @"https://github.com/paissaheavyindustries/Telesto";
                    p.Start();
                });
                tx.Start();
            }
            ImGui.SameLine();
            _adjusterX += ImGui.GetContentRegionAvail().X;
            ImGui.PopStyleColor(3);
            ImGui.End();
            ImGui.PopStyleColor(3);
            DrawDoodles();
            DrawForms();
        }

        private void OpenConfigUI()
        {
            _cfg.Opened = true;
        }

        private void SaveConfig()
        {
            _svc.pi.SavePluginConfig(_cfg);
        }

        public unsafe void SubmitCommand(string cmd)
        {
            if (_loggedIn == false || _funcPtrChatboxFound == false)
            {
                return;
            }
            AtkUnitBasePtr ptr = _svc.gg.GetAddonByName("ChatLog", 1);
            if (ptr != null && ptr.IsVisible == true)
            {
                IntPtr uiModule = _svc.gg.GetUIModule();
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
            
            if (_loggedIn == false) 
            {
                return;
            }
            try
            {
                lock (_waymarks)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var marker = MarkingController.Instance()->FieldMarkers[i];
                        Waymark w = _waymarks[(Waymark.WaymarkEnum)i];
                        w.Active = marker.Active;
                        if (w.Active == false)
                        {                            
                            continue;
                        }
                        w.X = marker.Position.X;
                        w.Y = marker.Position.Y;
                        w.Z = marker.Position.Z;                        
                    }                    
                }
            }
            catch (Exception)
            {
            }
        }

        private void OpenMapRaw(ushort territoryId, uint mapId, int x, int y)
        {
            _svc.gg.OpenMapWithMapLink(
                new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(territoryId, mapId, x, y)
            );
        }

        private void OpenMapWorld(ushort territoryId, uint mapId, float x, float y)
        {
            _svc.gg.OpenMapWithMapLink(
                new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(territoryId, mapId, x, y)
            );
        }

        internal Response ProcessRequest(PendingRequest pr)
        {
            string json = pr.Request;
            Dictionary<string, object> d = _pr.Parse(json);
            pr.Id = Convert.ToInt32(d["id"]);
            Response r = ProcessTelegramDictionary(d);
            if (r != null)
            {
                r.id = pr.Id;
            }
            return r;
        }

        internal static Response WrapToResponse(object o, string source = "undefined")
        {
            if (o == null)
            {
                return null;
            }
            return new Response() { source = source, response = o };
        }

        private Response ProcessTelegramDictionary(Dictionary<string, object> d)
        {
            switch (d["type"].ToString().ToLower())
            {
                case "printmessage":
                    return WrapToResponse(HandlePrintMessage(d["payload"]), "PrintMessage");
                case "printerror":
                    return WrapToResponse(HandlePrintError(d["payload"]), "PrintError");
                case "executecommand":
                    return WrapToResponse(HandleExecuteCommand(d["payload"]), "ExecuteCommand");
                case "openmap":
                    return WrapToResponse(HandleOpenMap(d["payload"]), "OpenMap");
                case "bundle":
                    return WrapToResponse(HandleBundle(d["payload"]), "Bundle");
                case "getpartymembers":
                    return WrapToResponse(HandleGetPartyMembers(), "GetPartyMembers");
                case "ping":
                    return WrapToResponse(HandlePing(), "Ping");
                case "enabledoodle":
                    return WrapToResponse(HandleEnableDoodle(d["payload"]), "EnableDoodle");
                case "disabledoodle":
                    return WrapToResponse(HandleDisableDoodle(d["payload"]), "DisableDoodle");
                case "disabledoodleregex":
                    return WrapToResponse(HandleDisableDoodleRegex(d["payload"]), "DisableDoodleRegex");
                case "subscribe":
                    return WrapToResponse(HandleSubscribe(d["payload"]), "Subscribe");
                case "unsubscribe":
                    return WrapToResponse(HandleUnsubscribe(d["payload"]), "Unsubscribe");
                case "macro":
                    return WrapToResponse(HandleMacro(d["payload"]), "Macro");
                case "form":
                    return WrapToResponse(HandleForm(d["payload"]), "Form");
            }
            _svc.cg.PrintError(String.Format("Unhandled Telesto telegram type '{0}'", d["type"].ToString()));
            return null;
        }

        private object HandlePrintMessage(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            _svc.cg.Print(d["message"].ToString());
            return null;
        }

        private object HandlePrintError(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            _svc.cg.PrintError(d["message"].ToString());
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
            if (s.IsUsingInlineBuffer == true)
            {
                return System.Text.Encoding.UTF8.GetString(s.InlineBuffer);
            }
            return System.Text.Encoding.UTF8.GetString(s.StringPtr, (int)s.BufUsed);
        }

        private unsafe IntPtr GetPartyListAgent()
        {
            AtkUnitBasePtr pl = _svc.gg.GetAddonByName("_PartyList", 1);
            return _svc.gg.FindAgentInterface(pl);
        }

        private unsafe object HandleGetPartyMembers()
        {
            List<Combatant> cbs = new List<Combatant>();
            Dictionary<string, int> idx = new Dictionary<string, int>();
            AgentHUD *ah = AgentHUD.Instance();
            for (int i = 0; i < ah->PartyMemberCount; i++)
            {
                var pm = ah->PartyMembers[i];
                string temp = pm.Name.ToString();
                idx[temp] = pm.Index;
            }            
            foreach (IPartyMember pm in _svc.pl)
            {
                string name = pm.Name.ToString();
                cbs.Add(new Combatant()
                {
                    displayname = pm.Name.ToString(),
                    fullname = pm.Name.ToString(),
                    order = idx.ContainsKey(name) == true ? idx[name] + 1 : 0,
                    jobid = pm.ClassJob.RowId,
                    level = pm.Level,
                    actor = String.Format("{0:x8}", pm.GameObject != null ? pm.GameObject.GameObjectId : 0),
                    x = pm.Position.X,
                    y = pm.Position.Y,
                    z = pm.Position.Z
                });
            }
            if (cbs.Count == 0)
            {
                var pc = _svc.cs.LocalPlayer;
                cbs.Add(new Combatant()
                {
                    displayname = pc.Name.ToString(),
                    fullname = pc.Name.ToString(),
                    order = 1,
                    jobid = pc.ClassJob.RowId,
                    level = pc.Level,
                    actor = String.Format("{0:x8}", pc.GameObjectId),
                    x = pc.Position.X,
                    y = pc.Position.Y,
                    z = pc.Position.Z
                });
            }
            cbs.Sort((a, b) => a.order.CompareTo(b.order));
            return cbs;
        }

        private object HandlePing()
        {
            return "pong";
        }

        internal IGameObject GetEntityById(ulong id)
        {
            return _svc.ot.SearchById(id);
        }

        internal IGameObject GetEntityByName(string id)
        {
            foreach (IGameObject go in _svc.ot)
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
                    case "a": wm = _waymarks[Waymark.WaymarkEnum.A]; break;
                    case "b": wm = _waymarks[Waymark.WaymarkEnum.B]; break;
                    case "c": wm = _waymarks[Waymark.WaymarkEnum.C]; break;
                    case "d": wm = _waymarks[Waymark.WaymarkEnum.D]; break;
                    case "one": case "1": wm = _waymarks[Waymark.WaymarkEnum.One]; break;
                    case "two": case "2": wm = _waymarks[Waymark.WaymarkEnum.Two]; break;
                    case "three": case "3": wm = _waymarks[Waymark.WaymarkEnum.Three]; break;
                    case "four": case "4": wm = _waymarks[Waymark.WaymarkEnum.Four]; break;
                }
            }
            return wm;
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
                case "41": return "VPR";
                case "42": return "PCT";
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
                case "41": return "DPS";
                case "42": return "DPS";
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

        private string GetEntityProperty(IGameObject go, string prop)
        {
            switch (prop.ToLower())
            {
                case "address":
                    return go.Address.ToString();
                case "name":
                    return go.Name.TextValue;
                case "job":
                    return TranslateJob(go is BattleChara ? ((BattleChara)go).ClassJob.RowId.ToString() : "0");
                case "jobid":
                    return go is BattleChara ? ((BattleChara)go).ClassJob.RowId.ToString() : "0";
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
                    return go.Position.X.ToString(CultureInfo.InvariantCulture);
                case "y":
                    return go.Position.Y.ToString(CultureInfo.InvariantCulture);
                case "z":
                    return go.Position.Z.ToString(CultureInfo.InvariantCulture);
                case "id":
                    return go.GameObjectId.ToString("X8");
                case "heading":
                    return go.Rotation.ToString(CultureInfo.InvariantCulture);
                case "targetid":
                    return go.TargetObjectId.ToString("X8");
                case "casttargetid":
                    return go is BattleChara ? ((BattleChara)go).CastTargetObjectId.ToString("X8") : "00000000";
                case "distance":
                    {
                        if (_svc.cs.LocalPlayer != null)
                        {
                            float xdev = _svc.cs.LocalPlayer.Position.X - go.Position.X;
                            float zdev = _svc.cs.LocalPlayer.Position.Z - go.Position.Z;
                            return Math.Sqrt((xdev * xdev) + (zdev * zdev)).ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    break;
                case "role":
                    return TranslateRole(go is BattleChara ? ((BattleChara)go).ClassJob.RowId.ToString() : "0");
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
                    else if (x == "_screenwidth")
                    {
                        Vector2 disp = ImGui.GetIO().DisplaySize;
                        val = disp.X.ToString();
                        found = true;
                    }
                    else if (x == "_screenheight")
                    {
                        Vector2 disp = ImGui.GetIO().DisplaySize;
                        val = disp.Y.ToString();
                        found = true;
                    }
                    else if (x == "_ffxivplayer")
                    {                        
                        val = _svc.cs.LocalPlayer.Name.ToString();
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
                            IGameObject go = null;
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
                                val = GetEntityProperty(go, gprop);
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

        internal double EvaluateNumericExpression(IGameObject goo, string expr)
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
            if (_svc.cs.LocalPlayer == null)
            {
                return new Vector3();
            }
            return _svc.cs.LocalPlayer.Position;
        }

        internal Vector3 TranslateToScreen(double x, double y, double z)
        {
            Vector2 tenp;
            _svc.gg.WorldToScreen(
                new Vector3((float)x, (float)y, (float)z),
                out tenp
            );
            return new Vector3(tenp.X, tenp.Y, (float)z);
        }

        private object HandleForm(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            Form f = Form.Deserialize(d);
            f.plug = this;
            lock (Forms)
            {
                Forms[f.Id] = f;
            }
            return null;
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

        private unsafe object HandleMacro(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            bool shared = false;
            int id = -1;
            if (d.ContainsKey("id"))
            {
                Int32.TryParse(d["id"].ToString(), out id);
            }
            if (d.ContainsKey("shared"))
            {
                bool.TryParse(d["shared"].ToString(), out shared);
            }
            _svc.cg.PrintError(string.Format("{0}, {1}", id, shared));
            if (id >= 0 && id <= 99)
            {
                var macro = RaptureMacroModule.Instance()->GetMacro((uint)(shared ? 1 : 0), (uint)id);
                if ((nint)macro != IntPtr.Zero)
                {
                    RaptureShellModule.Instance()->ExecuteMacro(macro);
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

        private void DrawForms()
        {
            List<Form> ds = new List<Form>();
            List<Form> dead = new List<Form>();
            lock (Forms)
            {
                if (_destroyForms)
                {
                    Forms.Clear();
                    _destroyForms = false;
                    return;
                }
                ds.AddRange(Forms.Values);
            }
            _numForms = ds.Count;
            if (_numForms == 0)
            {
                return;
            }
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.496f, 0.058f, 0.323f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.496f, 0.058f, 0.323f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            foreach (Form f in ds)
            {
                f.Render();
                if (f.Finished == true)
                {
                    dead.Add(f);
                }
            }            
            ImGui.PopStyleColor(3);
            if (dead.Count > 0)
            {
                lock (Forms)
                {
                    foreach (Form d in dead)
                    {
                        Forms.Remove(d.Id);
                    }
                }
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
            bool blargimded = _svc.cs.LocalPlayer != null ? _svc.cs.LocalPlayer.CurrentHp == 0 : false;
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
                Dictionary<string, string> killed = new Dictionary<string, string>();
                lock (Doodles)
                {
                    foreach (Doodle d in dead)
                    {
                        if (Doodles.ContainsKey(d.Name) == true)
                        {
                            Doodles.Remove(d.Name);
                            if (d.ExpiryNotifyEndpoint != null)
                            {
                                killed[d.Name] = d.ExpiryNotifyEndpoint;
                            }
                        }
                        d.Dispose();
                    }
                }
                foreach (KeyValuePair<string, string> kp in killed)
                {
                    QueueSendTelegram(
                        kp.Value,
                        JsonSerializer.Serialize<Notification>(
                            new Notification()
                            {
                                id = 1,
                                version = 1,
                                notificationid = kp.Key,
                                notificationtype = "doodleexpired",
                            }
                        )
                    );
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
            foreach (IGameObject go in _svc.ot)
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
                    //_svc.cg.Print(String.Format("{0} {1}: old {2} new {3}", go.Address, go.Name.TextValue, oldrep, newrep));
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
                                    objectid = go.GameObjectId.ToString("X8"),
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
