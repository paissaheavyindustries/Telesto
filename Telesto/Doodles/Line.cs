using ImGuiNET;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace Telesto.Doodles
{

    internal class Line : Doodle
    {

        internal Coordinate start { get; set; }
        internal Coordinate end { get; set; }
        internal float chonkiness { get; set; }
        internal string Thickness { get; set; }

        internal override Coordinate GetCoordinateByName(string id)
        {
            switch (id.ToLower())
            {
                default:
                case "start": return start;
                case "end": return end;
            }
        }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);            
            Thickness = (d.ContainsKey("thickness") == true) ? d["thickness"].ToString() : "1";
            start = new Coordinate();
            if (d.ContainsKey("start") == true)
            {
                start.Initialize((Dictionary<string, object>)d["start"]);
            }
            end = new Coordinate();
            if (d.ContainsKey("end") == true)
            {
                end.Initialize((Dictionary<string, object>)d["end"]);
            }
        }

        internal override bool Update()
        {
            if (base.Update() == false)
            {
                return false;
            }
            start.RefreshVector(p);
            end.RefreshVector(p);
            chonkiness = (float)p.EvaluateNumericExpression(Thickness);
            return true;
        }

        internal override void Draw()
        {
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(start.cp.X, start.cp.Y),
                new Vector2(end.cp.X, end.cp.Y),
                ImGui.GetColorU32(col),
                chonkiness
            );
        }

    }

}
