using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System;

namespace Telesto.FormElements
{

    internal class InputInt : FormElement
    {

        internal override bool Reported => true;
        private int _Value { get; set; } = 0;
        public override string Value
        {
            get
            {
                return _Value.ToString();
            }
            set
            {
                if (int.TryParse(value, out int temp))
                {
                    _Value = temp;
                }
            }
        }

        public string Text { get; set; } = "";
        public int MinValue { get; set; } = 0;
        public int MaxValue { get; set; } = 10;

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Text = (d.ContainsKey("text") == true) ? d["text"].ToString() : "";
            if (int.TryParse((d.ContainsKey("minvalue") == true) ? d["minvalue"].ToString() : "0", out int temp1))
            {
                MinValue = temp1;
            }
            if (int.TryParse((d.ContainsKey("maxvalue") == true) ? d["maxvalue"].ToString() : "10", out int temp2))
            {
                MaxValue = temp2;
            }
            if (_Value < MinValue)
            {
                _Value = MinValue;
            }
            if (_Value > MaxValue)
            {
                _Value = MaxValue;
            }
        }

        public override void Render()
        {
            int st = _Value;
            if (ImGui.SliderInt(Text + "##" + Guid.ToString(), ref st, MinValue, MaxValue) == true)
            {
                _Value = st;
            }
        }

    }

}
