using ImGuiNET;
using System.Collections.Generic;
using System;

namespace Telesto.FormElements
{

    internal class Checkbox : FormElement
    {

        internal override bool Reported => true;
        private bool _Value { get; set; } = false;

        public override string Value
        {
            get
            {
                return _Value.ToString();
            }
            set
            {
                if (bool.TryParse(value, out bool temp))
                {
                    _Value = temp;
                }
            }
        }
        public string Text { get; set; }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Text = (d.ContainsKey("text") == true) ? d["text"].ToString() : "";
        }

        public override void Render()
        {
            bool val = _Value;
            if (ImGui.Checkbox(Text + "##" + Guid.ToString(), ref val) == true)
            {
                _Value = val;
            }
        }

    }

}
