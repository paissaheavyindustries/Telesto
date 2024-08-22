using ImGuiNET;
using System.Collections.Generic;

namespace Telesto.FormElements
{

    internal class InputText : FormElement
    {

        internal override bool Reported => true;
        public override string Value { get; set; }
        public string Text { get; set; } = "";
        public uint MaxLength { get; set; } = 256;

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Text = (d.ContainsKey("text") == true) ? d["text"].ToString() : "";
            if (uint.TryParse((d.ContainsKey("maxlength") == true) ? d["maxlength"].ToString() : "256", out uint temp))
            {
                MaxLength = temp;
            }
            Text = Text.Substring(0, (int)MaxLength);
        }

        public override void Render()
        {
            string st = Value;
            if (ImGui.InputText(Text + "##" + Guid.ToString(), ref st, MaxLength) == true)
            {
                Value = st;
            }
        }

    }

}
