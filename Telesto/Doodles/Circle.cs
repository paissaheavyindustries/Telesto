using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Telesto.Doodles
{

    internal class Circle : Doodle
    {

        internal enum CoordSystemEnum
        {
            Screen,
            World
        }

        internal CoordSystemEnum csys { get; set; }
        internal Coordinate position { get; set; }
        internal float radiuschonk { get; set; }
        internal float linechonk { get; set; }
        internal string Radius { get; set; }
        internal string Thickness { get; set; }
        internal bool filled { get; set; }

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
            Radius = (d.ContainsKey("radius") == true) ? d["radius"].ToString() : "10";
            Thickness = (d.ContainsKey("thickness") == true) ? d["thickness"].ToString() : "1";
            string fill = (d.ContainsKey("filled") == true) ? d["filled"].ToString() : "false";
            filled = false;
            bool.TryParse(fill, out bool filledtemp);
            filled = filledtemp;
            position = new Coordinate() { doo = this };
            if (d.ContainsKey("position") == true)
            {
                position.Initialize((Dictionary<string, object>)d["position"]);
            }
            string csystem = (d.ContainsKey("system") == true) ? d["system"].ToString() : "screen";
            switch (csystem.ToLower())
            {
                case "world":
                    csys = CoordSystemEnum.World;
                    break;
                default:
                    csys = CoordSystemEnum.Screen;
                    break;
            }
        }

        internal override bool Update()
        {
            if (base.Update() == false)
            {
                return false;
            }
            position.RefreshVector(p);
            radiuschonk = (float)p.EvaluateNumericExpression(this, Radius);
            linechonk = (float)p.EvaluateNumericExpression(this, Thickness);
            return true;
        }

        internal override void Draw()
        {
            if (csys == CoordSystemEnum.Screen)
            {
                if (filled == true)
                {
                    ImGui.GetWindowDrawList().AddCircleFilled(
                        new Vector2(position.cp.X, position.cp.Y),
                        radiuschonk,
                        ImGui.GetColorU32(col),
                        48
                    );
                }
                else
                {
                    ImGui.GetWindowDrawList().AddCircle(
                        new Vector2(position.cp.X, position.cp.Y),
                        radiuschonk,
                        ImGui.GetColorU32(col),
                        48,
                        linechonk
                    );
                }
            }
            else
            {
                Vector3 temp = position.UntranslatedPosition(p);
                for (int i = 0; i <= 48; i++)
                {
                    Vector3 mauw = p.TranslateToScreen(
                        temp.X + (radiuschonk * Math.Sin((Math.PI / 24.0) * i)),
                        temp.Y,
                        temp.Z + (radiuschonk * Math.Cos((Math.PI / 24.0) * i))
                    );
                    ImGui.GetWindowDrawList().PathLineTo(new Vector2(mauw.X, mauw.Y));
                }
                if (filled == true)
                {
                    ImGui.GetWindowDrawList().PathFillConvex(
                        ImGui.GetColorU32(col)
                    );
                }
                else
                {
                    ImGui.GetWindowDrawList().PathStroke(
                        ImGui.GetColorU32(col),
                        ImDrawFlags.None,
                        linechonk
                    );
                }
            }
        }

    }

}
