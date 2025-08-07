using Dalamud.Bindings.ImGui;
using System.Collections.Generic;

namespace Telesto.FormElements
{

    internal class Label : FormElement
    {

        internal override bool Reported => false;
        public override string Value { get; set; }

        public string Text { get; set; }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Text = (d.ContainsKey("text") == true) ? d["text"].ToString() : "zoink";
        }

        public override void Render()
        {
            ImGui.TextWrapped(Text);
        }

    }

}
