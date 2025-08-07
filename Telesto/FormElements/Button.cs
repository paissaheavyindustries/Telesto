using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using static Telesto.Plugin;

namespace Telesto.FormElements
{

    internal class Button : FormElement
    {

        private bool _Reported { get; set; } = false;
        internal override bool Reported => _Reported;
        public override string Value { get; set; }

        public string Text { get; set; }
        public string Payload { get; set; }

        public enum ActionEnum
        {
            None,
            Payload,
            Submit,
            Cancel,
        }

        public ActionEnum Action { get; set; } = ActionEnum.None;

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Text = (d.ContainsKey("text") == true) ? d["text"].ToString() : "";
            Payload = (d.ContainsKey("payload") == true) ? d["payload"].ToString() : "";
            if (Enum.TryParse(typeof(ActionEnum), (d.ContainsKey("action") == true) ? d["action"].ToString() : "", out object temp))
            {
                Action = (ActionEnum)temp;
            }
        }

        public override void Render()
        {
            if (ImGui.Button(Text + "##" + Guid.ToString()))
            {
                switch (Action)
                {
                    case ActionEnum.None:
                        break;
                    case ActionEnum.Payload:
                        using (PendingRequest pr = new PendingRequest())
                        {
                            pr.Request = Payload;
                            Owner.plug.ProcessRequest(pr);
                        }
                        break;
                    case ActionEnum.Submit:
                        _Reported = true;
                        Owner.Submit();
                        break;
                    case ActionEnum.Cancel:
                        Owner.Cancel();
                        break;
                }
            }
        }

    }

}
