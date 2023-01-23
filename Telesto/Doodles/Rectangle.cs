using ImGuiNET;
using System;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Telesto.Doodles
{

    internal class Rectangle : Doodle
    {

        internal enum CoordSystemEnum
        {
            Screen,
            World
        }

        internal CoordSystemEnum csys { get; set; }
        internal Coordinate pos1 { get; set; }
        internal Coordinate pos2 { get; set; }
        internal float linechonk { get; set; }
        internal string Thickness { get; set; }
        internal bool filled { get; set; }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Thickness = (d.ContainsKey("thickness") == true) ? d["thickness"].ToString() : "1";
            string fill = (d.ContainsKey("filled") == true) ? d["filled"].ToString() : "false";
            filled = false;
            bool.TryParse(fill, out bool filledtemp);
            filled = filledtemp;
            pos1 = new Coordinate();
            if (d.ContainsKey("pos1") == true)
            {
                pos1.Initialize((Dictionary<string, object>)d["pos1"]);
            }
            pos2 = new Coordinate();
            if (d.ContainsKey("pos2") == true)
            {
                pos2.Initialize((Dictionary<string, object>)d["pos2"]);
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
            pos1.RefreshVector(p);
            pos2.RefreshVector(p);
            linechonk = (float)p.EvaluateNumericExpression(Thickness);
            return true;
        }

        internal override void Draw()
        {
            if (csys == CoordSystemEnum.Screen)
            {
                if (filled == true)
                {
                    ImGui.GetWindowDrawList().AddRectFilled(
                        new Vector2(pos1.cp.X, pos1.cp.Y),
                        new Vector2(pos2.cp.X, pos2.cp.Y),
                        ImGui.GetColorU32(col),
                        1.0f,
                        ImDrawFlags.None
                    );
                }
                else
                {
                    ImGui.GetWindowDrawList().AddRect(
                        new Vector2(pos1.cp.X, pos1.cp.Y),
                        new Vector2(pos2.cp.X, pos2.cp.Y),
                        ImGui.GetColorU32(col),
                        1.0f,
                        ImDrawFlags.None,
                        linechonk
                    );
                }
            }
            else
            {
                Vector3 t1 = pos1.UnadjustedPosition(p);
                Vector3 t3 = pos2.UnadjustedPosition(p);
                Vector3 t2 = new Vector3(t3.X, (t1.Y +  t3.Y) / 2.0f, t1.Z);
                Vector3 t4 = new Vector3(t1.X, (t1.Y + t3.Y) / 2.0f, t3.Z);
                Vector3 v1 = p.TranslateToScreen(t1.X, t1.Y, t1.Z);
                Vector3 v2 = p.TranslateToScreen(t2.X, t2.Y, t2.Z);
                Vector3 v3 = p.TranslateToScreen(t3.X, t3.Y, t3.Z);
                Vector3 v4 = p.TranslateToScreen(t4.X, t4.Y, t4.Z);
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(v1.X, v1.Y));
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(v2.X, v2.Y));
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(v3.X, v3.Y));
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(v4.X, v4.Y));
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(v1.X, v1.Y));
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
