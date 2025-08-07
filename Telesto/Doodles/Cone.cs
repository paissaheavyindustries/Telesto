using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Telesto.Doodles
{

    internal class Cone : Doodle
    {

        internal enum CoordSystemEnum
        {
            Screen,
            World
        }

        internal CoordSystemEnum csys { get; set; }
        internal Coordinate position { get; set; }
        internal Coordinate target { get; set; }
        internal float radiuschonk { get; set; }
        internal float linechonk { get; set; }
        internal float anglechonk { get; set; }
        internal float headingchonk { get; set; }
        internal string Radius { get; set; }
        internal string Thickness { get; set; }
        internal string Angle { get; set; }
        internal string Heading { get; set; }
        internal bool filled { get; set; }
        internal bool targeted { get; set; }

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
            Angle = (d.ContainsKey("angle") == true) ? d["angle"].ToString() : "pi05";
            Heading = (d.ContainsKey("heading") == true) ? d["heading"].ToString() : "0";
            string fill = (d.ContainsKey("filled") == true) ? d["filled"].ToString() : "false";
            filled = false;
            bool.TryParse(fill, out bool filledtemp);
            filled = filledtemp;
            position = new Coordinate() { doo = this };
            if (d.ContainsKey("position") == true)
            {
                position.Initialize((Dictionary<string, object>)d["position"]);
            }
            if (d.ContainsKey("target") == true)
            {
                target = new Coordinate() { doo = this };
                target.Initialize((Dictionary<string, object>)d["target"]);
                targeted = true;
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
            anglechonk = (float)p.EvaluateNumericExpression(this, Angle);
            if (targeted == true)
            {
                Vector3 temp1 = position.UntranslatedPosition(p);
                Vector3 temp2 = target.UntranslatedPosition(p);
                headingchonk = -1.0f * (float)Math.Atan2(temp1.Z - temp2.Z, temp1.X - temp2.X) - (float)(Math.PI / 2.0f);
            }
            else
            {
                headingchonk = (float)p.EvaluateNumericExpression(this, Heading);
            }
            return true;
        }

        internal override void Draw()
        {
            if (csys == CoordSystemEnum.Screen)
            {
            }
            else
            {
                Vector3 temp = position.UntranslatedPosition(p);
                int segments = (int)Math.Ceiling(48.0 / Math.PI * 2.0 * anglechonk);
                segments = segments < 2 ? 2 : segments;
                float segangle = anglechonk / (float)segments;
                Vector3 mid = p.TranslateToScreen(temp.X, temp.Y, temp.Z);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(mid.X, mid.Y));
                float baseangle = headingchonk - (anglechonk / 2.0f);
                for (int i = 0; i < segments; i++)
                {
                    Vector3 mauw = p.TranslateToScreen(
                        temp.X + (radiuschonk * Math.Sin(baseangle + (segangle * i))),
                        temp.Y,
                        temp.Z + (radiuschonk * Math.Cos(baseangle + (segangle * i)))
                    );
                    ImGui.GetWindowDrawList().PathLineTo(new Vector2(mauw.X, mauw.Y));
                }
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(mid.X, mid.Y));
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
