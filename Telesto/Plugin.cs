﻿using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client;
using System;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Collections.Generic;

namespace Telesto
{

    // Telegram Service for Triggernometry Operations (TELESTO)
    public sealed class Plugin : IDalamudPlugin
    {

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
        private string _debugSigTest = "";
        private bool _debugSigFindHit = false;
        private bool _configOpen = false;
        private bool _loggedIn = false;
        private bool _funcPtrFound = false;

        private bool _cfgAutostartEndpoint;
        private string _cfgHttpEndpoint;

        private delegate void PostCommandDelegate(IntPtr ui, IntPtr cmd, IntPtr unk1, byte unk2);
        private PostCommandDelegate postCmdFuncptr = null;

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
            IntPtr chatBoxModulePointer = SearchForSig("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
            if (chatBoxModulePointer != IntPtr.Zero)
            {
                postCmdFuncptr = Marshal.GetDelegateForFunctionPointer<PostCommandDelegate>(chatBoxModulePointer);
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
            if (_configOpen == false)
            {
                return;
            }
            bool stateLoggedIn = _loggedIn;
            bool stateFuncPtrFound = _funcPtrFound;
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
                ImGui.Checkbox("Logged in", ref stateLoggedIn);
                ImGui.Checkbox("Function pointer found", ref stateFuncPtrFound);
                ImGui.Separator();
                ImGui.InputText("Find sig", ref _debugSigTest, 2048);
                ImGui.Text(_debugSigFindHit == true ? "Hit found" : "Hit not found");
                if (ImGui.Button("Find"))
                {
                    try
                    {
                        _debugSigFindHit = false;
                        IntPtr res = SearchForSig(_debugSigTest);
                        _debugSigFindHit = res != (IntPtr.Zero);
                        _debugSigTest = "";
                    }
                    catch (Exception ex)
                    {
                        _cg.PrintError(String.Format("Exception in find sig test: {0}", ex.Message));
                    }
                }
                ImGui.Separator();
                ImGui.InputText("Telegram test input", ref _debugTeleTest, 2048);
                if (ImGui.Button("Process test input"))
                {
                    try
                    {
                        ProcessTelegram(_debugTeleTest);
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
                        IntPtr p = Marshal.AllocHGlobal(400);
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

        internal string ProcessTelegram(string json)
        {            
            Dictionary<string, object> d = _pr.Parse(json);
            return ProcessTelegramDictionary(d);
        }

        private string ProcessTelegramDictionary(Dictionary<string, object> d)
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
            }
            _cg.PrintError(String.Format("Unhandled Telesto telegram type '{0}'", d["type"].ToString()));
            return null;
        }

        private string HandlePrintMessage(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            _cg.Print(d["message"].ToString());
            return null;
        }

        private string HandlePrintError(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            _cg.PrintError(d["message"].ToString());
            return null;
        }

        private string HandleExecuteCommand(object o)
        {
            Dictionary<string, object> d = (Dictionary<string, object>)o;
            SubmitCommand(d["command"].ToString());
            return null;
        }

        private string HandleOpenMap(object o)
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

        private string HandleBundle(object o)
        {
            List<object> obs = (List<object>)o;
            foreach (object ob in obs)
            {
                ProcessTelegramDictionary((Dictionary<string, object>)ob);
            }
            return null;
        }

        private string HandleGetPartyMembers()
        {
            int num = 0;
             _pl.GetPartyMemberAddress(0);
            using (IEnumerator<PartyMember> pms = _pl.GetEnumerator())
            {
                while (pms.MoveNext() == true)
                {
                    num++;
                    PartyMember pm = pms.Current;
                    _cg.Print(pm.Name);
                }
            }
            _cg.Print("Nium is " + num);
            return null;
        }

    }

}