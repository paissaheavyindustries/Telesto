using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;

namespace Telesto.FormElements
{

    internal class Layout : FormElement
    {
        
        internal override bool Reported => false;
        public override string Value { get; set; }

        public enum LayoutEnum
        {
            Break,
            Separator,
        }

        public LayoutEnum Action { get; set; } = LayoutEnum.Break;

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            if (Enum.TryParse(typeof(LayoutEnum), (d.ContainsKey("action") == true) ? d["action"].ToString() : "", out object temp))
            {
                Action = (LayoutEnum)temp;
            }
        }

        public override void Render()
        {
            switch (Action)
            {
                case LayoutEnum.Break:
                    ImGui.Text("");
                    break;
                case LayoutEnum.Separator:
                    ImGui.Separator();
                    break;
            }
        }

    }

}
