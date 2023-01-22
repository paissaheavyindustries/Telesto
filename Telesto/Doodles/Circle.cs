using ImGuiNET;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace Telesto.Doodles
{

    internal class Circle : Doodle
    {

        internal Coordinate position { get; set; }
        internal float chonkiness { get; set; }
        internal string Radius { get; set; }
        internal bool filled { get; set; }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Radius = (d.ContainsKey("radius") == true) ? d["radius"].ToString() : "10";
            string fill = (d.ContainsKey("filled") == true) ? d["filled"].ToString() : "false";
            filled = false;
            bool.TryParse(fill, out bool filledtemp);
            filled = filledtemp;
            position = new Coordinate();
            if (d.ContainsKey("position") == true)
            {
                position.Initialize((Dictionary<string, object>)d["position"]);
            }            
        }

        internal override bool Update()
        {
            if (base.Update() == false)
            {
                return false;
            }
            position.RefreshVector(p);
            chonkiness = (float)p.EvaluateNumericExpression(Radius);
            return true;
        }

        internal override void Draw()
        {
            if (filled == true)
            {
                ImGui.GetWindowDrawList().AddCircleFilled(
                    new Vector2(position.cp.X, position.cp.Y),
                    chonkiness,
                    ImGui.GetColorU32(col)
                );
            }
            else
            {
                ImGui.GetWindowDrawList().AddCircle(
                    new Vector2(position.cp.X, position.cp.Y),
                    chonkiness,
                    ImGui.GetColorU32(col)
                );
            }
        }

    }

}
