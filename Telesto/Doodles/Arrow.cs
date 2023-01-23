using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Telesto.Doodles
{

    internal class Arrow : Doodle
    {

        internal enum CoordSystemEnum
        {
            Screen,
            World
        }

        internal CoordSystemEnum csys { get; set; }
        internal Coordinate from { get; set; }
        internal Coordinate to { get; set; }
        internal float radiuschonk { get; set; }
        internal float linechonk { get; set; }
        internal string Radius { get; set; }
        internal string Thickness { get; set; }

        internal override void Initialize(Dictionary<string, object> d)
        {
            base.Initialize(d);
            Thickness = (d.ContainsKey("thickness") == true) ? d["thickness"].ToString() : "1";
            from = new Coordinate();
            if (d.ContainsKey("from") == true)
            {
                from.Initialize((Dictionary<string, object>)d["from"]);
            }
            to = new Coordinate();
            if (d.ContainsKey("to") == true)
            {
                to.Initialize((Dictionary<string, object>)d["to"]);
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
            to.RefreshVector(p);
            linechonk = (float)p.EvaluateNumericExpression(Thickness);
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
                Vector3 tt = to.UnadjustedPosition(p);
                float distance = Vector3.Distance(tf, tt);
                double anglexy = Math.Atan2(tf.Y - tt.Y, tf.X - tt.X);
                double anglexz = Math.Atan2(tf.Z - tt.Z, tf.X - tt.X);
                float head = distance * 0.7f;
                float width = distance / 20.0f;                
                width = float.Clamp(width, 0.1f, 1.0f);
                Vector3 tx;
                List <Vector3> verts = new List<Vector3>();
                verts.Add(tf);
                verts.Add(tx = new Vector3(tf.X + (float)(Math.Cos(anglexz + (Math.PI / 2.0)) * width), tf.Y, tf.Z + (float)(Math.Sin(anglexz + (Math.PI / 2.0)) * width)));
                verts.Add(tx = new Vector3(tx.X + (float)(Math.Cos(anglexz + Math.PI) * head), tx.Y + ((tt.Y - tf.Y) * 0.7f), tx.Z + (float)(Math.Sin(anglexz + Math.PI) * head)));
                verts.Add(tx = new Vector3(tx.X + (float)(Math.Cos(anglexz + (Math.PI / 2.0)) * width * 2), tx.Y, tx.Z + (float)(Math.Sin(anglexz + (Math.PI / 2.0)) * width * 2)));
                verts.Add(tt);
                tx = verts[3];
                verts.Add(tx = new Vector3(tx.X + (float)(Math.Cos(anglexz - (Math.PI / 2.0)) * width * 6), tx.Y, tx.Z + (float)(Math.Sin(anglexz - (Math.PI / 2.0)) * width * 6)));
                verts.Add(tx = new Vector3(tx.X + (float)(Math.Cos(anglexz + (Math.PI / 2.0)) * width * 2), tx.Y, tx.Z + (float)(Math.Sin(anglexz + (Math.PI / 2.0)) * width * 2)));
                verts.Add(tx = new Vector3(tf.X + (float)(Math.Cos(anglexz - (Math.PI / 2.0)) * width), tf.Y, tf.Z + (float)(Math.Sin(anglexz - (Math.PI / 2.0)) * width)));
                verts.Add(tf);
                foreach (Vector3 v in verts)
                {
                    Vector3 vx = p.TranslateToScreen(v.X, v.Y, v.Z);
                    ImGui.GetWindowDrawList().PathLineTo(new Vector2(vx.X, vx.Y));
                }                
                ImGui.GetWindowDrawList().PathStroke(
                    ImGui.GetColorU32(col),
                    ImDrawFlags.None,
                    3.0f
                );
            }
        }

    }

}
