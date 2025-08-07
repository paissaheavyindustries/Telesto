using Dalamud.Bindings.ImGui;
using System.Collections;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;

namespace Telesto.Doodles
{

    internal class Text : Doodle
    {

        internal enum AlignmentEnum
        {
            Near,
            Center,
            Far
        }

        internal AlignmentEnum halign = AlignmentEnum.Near;
        internal AlignmentEnum valign = AlignmentEnum.Near;
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
            if (d.ContainsKey("halign") == true)
            {
                switch (d["halign"].ToString().ToLower())
                {
                    default:
                    case "left":
                    case "near":
                        halign = AlignmentEnum.Near;
                        break;
                    case "center":
                    case "middle":
                        halign = AlignmentEnum.Center;
                        break;
                    case "right":
                    case "far":
                        halign = AlignmentEnum.Far;
                        break;
                }
            }
            if (d.ContainsKey("valign") == true)
            {
                switch (d["valign"].ToString().ToLower())
                {
                    default:
                    case "top":
                    case "near":
                        valign = AlignmentEnum.Near;
                        break;
                    case "center":
                    case "middle":
                        valign = AlignmentEnum.Center;
                        break;
                    case "bottom":
                    case "far":
                        valign = AlignmentEnum.Far;
                        break;
                }
            }
            position = new Coordinate() { doo = this };
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
            chonkiness = (float)p.EvaluateNumericExpression(this, Size);
            return true;
        }

        internal override void Draw()
        {
            Vector2 pt = new Vector2(position.cp.X, position.cp.Y);
            if (halign != AlignmentEnum.Near || valign != AlignmentEnum.Near)
            {
                float defSize = ImGui.GetFontSize();
                float mul = chonkiness / defSize;
                Vector2 sz = ImGui.CalcTextSize(DisplayedString);
                sz.X *= mul;
                sz.Y *= mul;
                if (halign == AlignmentEnum.Center)
                {
                    pt.X -= sz.X / 2.0f;
                }
                if (halign == AlignmentEnum.Far)
                {
                    pt.X -= sz.X;
                }
                if (valign == AlignmentEnum.Center)
                {
                    pt.Y -= sz.Y / 2.0f;
                }
                if (valign == AlignmentEnum.Far)
                {
                    pt.Y -= sz.Y;
                }
            }
            ImGui.GetWindowDrawList().AddText(
                ImGui.GetFont(),
                chonkiness,
                pt,
                ImGui.GetColorU32(col),
                DisplayedString
            );
        }

    }

}
