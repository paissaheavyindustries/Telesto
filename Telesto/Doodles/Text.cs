using ImGuiNET;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace Telesto.Doodles
{

    internal class Text : Doodle
    {

        internal Coordinate position { get; set; }
        internal string DisplayedString { get; set; }
        internal float chonkiness { get; set; }
        internal string Size { get; set; }

        internal override Coordinate GetCoordinateByName(string id)
        {
            switch (id.ToLower())
            {
                default:
                case "position": return position;
            }
        }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Size = (d.ContainsKey("size") == true) ? d["size"].ToString() : "10";
            position = new Coordinate();
            if (d.ContainsKey("position") == true)
            {
                position.Initialize((Dictionary<string, object>)d["position"]);
            }
            DisplayedString = (d.ContainsKey("text") == true) ? d["text"].ToString() : "";
        }

        internal override bool Update()
        {
            if (base.Update() == false)
            {
                return false;
            }
            position.RefreshVector(p);
            chonkiness = (float)p.EvaluateNumericExpression(Size);
            return true;
        }

        internal override void Draw()
        {
            ImGui.GetWindowDrawList().AddText(
                ImGui.GetFont(),
                chonkiness,
                new Vector2(position.cp.X, position.cp.Y),
                ImGui.GetColorU32(col),
                DisplayedString
            );
        }

    }

}
