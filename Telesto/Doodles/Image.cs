using Dalamud.Interface.Internal;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vector2 = System.Numerics.Vector2;

namespace Telesto.Doodles
{

    internal class Image : Doodle
    {

        internal enum AlignmentEnum
        {
            Near,
            Center,
            Far
        }

        internal override string DefaultR => "1";
        internal override string DefaultG => "1";
        internal override string DefaultB => "1";
        internal override string DefaultA => "1";

        internal string Width { get; set; }
        internal string Height { get; set; }

        internal Regex rex = new Regex(@"^(?<val>\d*)%$");
        internal AlignmentEnum halign = AlignmentEnum.Near;
        internal AlignmentEnum valign = AlignmentEnum.Near;
        internal Coordinate position { get; set; }
        internal uint IconId = 0;
        internal IDalamudTextureWrap? Texture { get; set; } = null;

        internal int calcWidth;
        internal int calcHeight;

        public override void Dispose()
        {
            base.Dispose();
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }

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
            Width = (d.ContainsKey("width") == true) ? d["width"].ToString() : "100%";
            Height = (d.ContainsKey("height") == true) ? d["height"].ToString() : "100%";
            if (d.ContainsKey("icon") == true)
            {
                uint.TryParse(d["icon"].ToString(), out IconId);
            }
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
        }

        internal override bool Update()
        {
            if (base.Update() == false)
            {
                return false;
            }
            position.RefreshVector(p);
            if (Texture == null && IconId != 0)
            {
                Texture = p.GetTexture(IconId);
                IconId = 0;
            }
            Match m;
            m = rex.Match(Width);
            if (m.Success == true)
            {
                calcWidth = (int)Math.Ceiling((float)Texture.Width * (float)int.Parse(m.Groups["val"].Value) / 100.0f);
            }
            else
            {
                calcWidth = (int)Math.Ceiling(p.EvaluateNumericExpression(this, Width));
            }
            m = rex.Match(Height);
            if (m.Success == true)
            {
                calcHeight = (int)Math.Ceiling((float)Texture.Height * (float)int.Parse(m.Groups["val"].Value) / 100.0f);
            }
            else
            {
                calcHeight = (int)Math.Ceiling(p.EvaluateNumericExpression(this, Height));
            }
            return true;
        }

        internal override void Draw()
        {
            if (Texture == null)
            {
                return;
            }
            Vector2 pt = new Vector2(position.cp.X, position.cp.Y);
            if (halign != AlignmentEnum.Near || valign != AlignmentEnum.Near)
            {
                if (halign == AlignmentEnum.Center)
                {
                    pt.X -= calcWidth / 2.0f;
                }
                if (halign == AlignmentEnum.Far)
                {
                    pt.X -= calcWidth;
                }
                if (valign == AlignmentEnum.Center)
                {
                    pt.Y -= calcHeight / 2.0f;
                }
                if (valign == AlignmentEnum.Far)
                {
                    pt.Y -= calcHeight;
                }
            }
            ImGui.SetCursorPos(pt);
            ImGui.Image(Texture.ImGuiHandle, new Vector2(calcWidth, calcHeight), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), col);
        }

    }

}
