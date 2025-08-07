using Dalamud.Interface.Style;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Telesto.Doodles
{

    internal class Donut : Doodle
    {

        internal enum CoordSystemEnum
        {
            Screen,
            World
        }

        internal CoordSystemEnum csys { get; set; }
        internal Coordinate position { get; set; }
        internal float innerradiuschonk { get; set; }
        internal float outerradiuschonk { get; set; }
        internal float linechonk { get; set; }
        internal string InnerRadius { get; set; }
        internal string OuterRadius { get; set; }
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
            InnerRadius = (d.ContainsKey("innerradius") == true) ? d["innerradius"].ToString() : "10";
            OuterRadius = (d.ContainsKey("outerradius") == true) ? d["outerradius"].ToString() : "15";
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
            innerradiuschonk = (float)p.EvaluateNumericExpression(this, InnerRadius);
            outerradiuschonk = (float)p.EvaluateNumericExpression(this, OuterRadius);
            linechonk = (float)p.EvaluateNumericExpression(this, Thickness);
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
                if (filled == false)
                {
                    for (int i = 0; i <= 48; i++)
                    {
                        Vector3 mauw = p.TranslateToScreen(
                            temp.X + (innerradiuschonk * Math.Sin((Math.PI / 24.0) * i)),
                            temp.Y,
                            temp.Z + (innerradiuschonk * Math.Cos((Math.PI / 24.0) * i))
                        );
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(mauw.X, mauw.Y));
                    }
                    ImGui.GetWindowDrawList().PathStroke(
                        ImGui.GetColorU32(col),
                        ImDrawFlags.None,
                        linechonk
                    );
                    for (int i = 0; i <= 48; i++)
                    {
                        Vector3 mauw = p.TranslateToScreen(
                            temp.X + (outerradiuschonk * Math.Sin((Math.PI / 24.0) * i)),
                            temp.Y,
                            temp.Z + (outerradiuschonk * Math.Cos((Math.PI / 24.0) * i))
                        );
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(mauw.X, mauw.Y));
                    }
                    ImGui.GetWindowDrawList().PathStroke(
                        ImGui.GetColorU32(col),
                        ImDrawFlags.None,
                        linechonk
                    );
                }
                else
                {
                    Vector3 v1, v2, v3, v4;
                    v1 = p.TranslateToScreen(
                        temp.X + (innerradiuschonk * Math.Sin((Math.PI / 23.0) * 0)),
                        temp.Y,
                        temp.Z + (innerradiuschonk * Math.Cos((Math.PI / 24.0) * 0))
                    );
                    v4 = p.TranslateToScreen(
                        temp.X + (outerradiuschonk * Math.Sin((Math.PI / 24.0) * 0)),
                        temp.Y,
                        temp.Z + (outerradiuschonk * Math.Cos((Math.PI / 24.0) * 0))
                    );
                    for (int i = 0; i <= 47; i++)
                    {
                        v2 = p.TranslateToScreen(
                            temp.X + (innerradiuschonk * Math.Sin((Math.PI / 24.0) * (i + 1))),
                            temp.Y,
                            temp.Z + (innerradiuschonk * Math.Cos((Math.PI / 24.0) * (i + 1)))
                        );
                        v3 = p.TranslateToScreen(
                            temp.X + (outerradiuschonk * Math.Sin((Math.PI / 24.0) * (i + 1))),
                            temp.Y,
                            temp.Z + (outerradiuschonk * Math.Cos((Math.PI / 24.0) * (i + 1)))
                        );
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(v1.X, v1.Y));
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(v2.X, v2.Y));
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(v3.X, v3.Y));
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(v4.X, v4.Y));
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(v1.X, v1.Y));
                        ImGui.GetWindowDrawList().PathFillConvex(
                            ImGui.GetColorU32(col)
                        );
                        v1 = v2;
                        v4 = v3;
                    }
                }
            }
        }

    }

}
