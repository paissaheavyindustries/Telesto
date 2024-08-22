using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using Telesto.FormElements;

namespace Telesto
{

    internal class Form
    {

        internal Plugin plug = null;
        internal Guid Guid { get; set; } = Guid.Empty;
        internal bool Finished { get; set; } = false;
        internal bool _firstShow = true;

        public string Title { get; set; }
        public string Id { get; set; }
        public string Callback { get; set; }
        public List<FormElement> Elements { get; set; } = new List<FormElement>();

        public int Width { get; set; } = 500;
        public int Height { get; set; } = 500;

        public Form()
        {
            Guid = Guid.NewGuid();
        }

        internal static Form Deserialize(Dictionary<string, object> d)
        {
            Form f = new Form();
            f.Id = d["id"].ToString();
            f.Title = d["title"].ToString();
            f.Callback = d["callback"].ToString();
            List<object> eles = (List<object>)d["elements"];
            foreach (var rele in eles)
            {
                FormElement fe = null;
                Dictionary<string, object> ele = (Dictionary<string, object>)rele;
                string type = ele["type"].ToString();
                switch (type.ToLower())
                {
                    case "button":
                        fe = new FormElements.Button();
                        break;
                    case "checkbox":
                        fe = new FormElements.Checkbox();
                        break;
                    case "hidden":
                        fe = new FormElements.Hidden();
                        break;
                    case "inputint":
                        fe = new FormElements.InputInt();
                        break;
                    case "inputtext":
                        fe = new FormElements.InputText();
                        break;
                    case "label":
                        fe = new FormElements.Label();
                        break;
                }
                if (fe != null)
                {
                    fe.Owner = f;
                    fe.Initialize(ele);
                    f.Elements.Add(fe);
                }
            }
            return f;
        }

        public void Cancel()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Finished = true;
            result["_formid"] = Id;
            Plugin.Response r = Plugin.WrapToResponse(result, "FormCancel");
            if (Callback != "")
            {
                plug.QueueSendTelegram(Callback, JsonSerializer.Serialize<Plugin.Response>(r));
            }
        }

        public void Submit()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach(FormElement e in Elements)
            {
                if (e.Reported == false)
                {
                    continue;
                }
                result[e.Id] = e.Value;
            }
            Finished = true;
            result["_formid"] = Id;
            Plugin.Response r = Plugin.WrapToResponse(result, "FormSubmit");
            if (Callback != "")
            {
                plug.QueueSendTelegram(Callback, JsonSerializer.Serialize<Plugin.Response>(r));
            }
        }

        public void Render()
        {
            if (Finished == true)
            {
                return;
            }
            if (Width < 100)
            {
                Width = 100;
            }
            if (Height < 100)
            {
                Height = 100;
            }
            ImGui.SetNextWindowSize(new Vector2(Width, Height), ImGuiCond.FirstUseEver);
            if (_firstShow == true)
            {
                Vector2 szx = ImGui.GetIO().DisplaySize;
                Vector2 pt = new Vector2((szx.X / 2) - (Width / 2), (szx.Y / 2) - (Height / 2));
                ImGui.SetNextWindowPos(pt);
                _firstShow = false;
            }            
            bool open = true;
            if (ImGui.Begin(Title + "##" + Guid.ToString(), ref open, ImGuiWindowFlags.NoCollapse) == false)
            {
                ImGui.End();
                return;
            }
            if (open == false)
            {
                Finished = true;
                ImGui.End();
                Cancel();
                return;
            }
            Plugin.KeepWindowInSight();
            Vector2 szy = ImGui.GetWindowSize();
            if (szy.X < 100 || szy.Y < 100)
            {
                if (szy.X < 100)
                {
                    szy.X = 100;
                }
                if (szy.Y < 100)
                {
                    szy.Y = 100;
                }
                ImGui.SetWindowSize(szy);
            }
            foreach (FormElement e in Elements)
            {
                e.Render();                
            }
            ImGui.End();
        }

    }

}
