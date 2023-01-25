using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Telesto.Doodles
{

    internal class Beam : Doodle
    {

        internal enum CoordSystemEnum
        {
            Screen,
            World
        }

        internal CoordSystemEnum csys { get; set; }
        internal Coordinate from { get; set; }
        internal Coordinate at { get; set; }
        internal float linechonk { get; set; }
        internal string Thickness { get; set; }
        internal float widthchonk { get; set; }
        internal string Width { get; set; }
        internal float lengthchonk { get; set; }
        internal string Length { get; set; }

        internal override Coordinate GetCoordinateByName(string id)
        {
            switch (id.ToLower())
            {
                default:
                case "from": return from;
                case "at": return at;
            }
        }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Thickness = (d.ContainsKey("thickness") == true) ? d["thickness"].ToString() : "3";
            Width = (d.ContainsKey("width") == true) ? d["width"].ToString() : "1";
            Length = (d.ContainsKey("length") == true) ? d["length"].ToString() : "1";
            from = new Coordinate();
            if (d.ContainsKey("from") == true)
            {
                from.Initialize((Dictionary<string, object>)d["from"]);
            }
            at = new Coordinate();
            if (d.ContainsKey("at") == true)
            {
                at.Initialize((Dictionary<string, object>)d["at"]);
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
            from.RefreshVector(p);
            at.RefreshVector(p);
            linechonk = (float)p.EvaluateNumericExpression(Thickness);
            widthchonk = (float)p.EvaluateNumericExpression(Width);
            lengthchonk = (float)p.EvaluateNumericExpression(Length);
            return true;
        }

        internal override void Draw()
        {
            if (csys == CoordSystemEnum.Screen)
            {
                // ehh maybe some day
            }
            else
            {
                Vector3 tf = from.UnadjustedPosition(p);
                Vector3 tt = at.UnadjustedPosition(p);
                float distance = Vector3.Distance(tf, tt);
                float length;
                length = lengthchonk < 0.0 ? distance : lengthchonk;
                double anglexz = Math.Atan2(tf.Z - tt.Z, tf.X - tt.X);
                float mul = length / distance;
                Vector3 tx;
                List<Vector3> verts = new List<Vector3>();
                verts.Add(tf);
                verts.Add(tx = new Vector3(tf.X + (float)(Math.Cos(anglexz + (Math.PI / 2.0)) * widthchonk), tf.Y, tf.Z + (float)(Math.Sin(anglexz + (Math.PI / 2.0)) * widthchonk)));
                verts.Add(tx = new Vector3(tx.X + (float)(Math.Cos(anglexz + Math.PI) * length), tx.Y + ((tt.Y - tf.Y) * mul), tx.Z + (float)(Math.Sin(anglexz + Math.PI) * length)));
                verts.Add(tx = new Vector3(tx.X - (float)(Math.Cos(anglexz + (Math.PI / 2.0)) * widthchonk * 2), tx.Y, tx.Z - (float)(Math.Sin(anglexz + (Math.PI / 2.0)) * widthchonk * 2)));
                tx = tf;
                verts.Add(tx = new Vector3(tf.X - (float)(Math.Cos(anglexz + (Math.PI / 2.0)) * widthchonk), tf.Y, tf.Z - (float)(Math.Sin(anglexz + (Math.PI / 2.0)) * widthchonk)));
                verts.Add(tf);
                foreach (Vector3 v in verts)
                {
                    Vector3 vx = p.TranslateToScreen(v.X, v.Y, v.Z);
                    ImGui.GetWindowDrawList().PathLineTo(new Vector2(vx.X, vx.Y));
                }
                ImGui.GetWindowDrawList().PathStroke(
                    ImGui.GetColorU32(col),
                    ImDrawFlags.None,
                    linechonk
                );
            }
        }

    }

}
