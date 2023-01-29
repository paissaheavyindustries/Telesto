using ImGuiNET;
using System;
using System.Collections.Generic;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Telesto.Doodles
{

    internal class Waymark : Doodle
    {

        internal Coordinate position { get; set; }

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
            return true;
        }

        internal Int64 Turninator()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        internal override void Draw()
        {
            Vector3 temp = position.UnadjustedPosition(p);
            Vector3 me = p.GetLocalPosition();
            float distance = Vector3.Distance(temp, me);
            float alpha = col.W / 4.0f;
            if (distance < 2.0f)
            {
                alpha = alpha * (distance - 1.0f);
            }
            Vector4 acol = new Vector4(col.X, col.Y, col.Z, alpha);
            double turn = ((Turninator() % 10000) / 10000.0) * Math.PI * 2.0;
            for (int i = 0; i <= 48; i++)
            {
                Vector3 mauw = p.TranslateToScreen(
                    temp.X + (1.2f *
                        Math.Sin(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    ),
                    temp.Y,
                    temp.Z + (1.2f *
                        Math.Cos(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    )
                );
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(mauw.X, mauw.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(
                ImGui.GetColorU32(col),
                ImDrawFlags.None,
                5.0f
            );
            for (int i = 0; i <= 48; i++)
            {
                Vector3 mauw = p.TranslateToScreen(
                    temp.X + (1.2f *
                        Math.Sin(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    ),
                    temp.Y,
                    temp.Z + (1.2f *
                        Math.Cos(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    )
                );
                ImGui.GetWindowDrawList().PathLineTo(new Vector2(mauw.X, mauw.Y));
            }
            ImGui.GetWindowDrawList().PathFillConvex(
                ImGui.GetColorU32(acol)
            );
            for (int i = 0; i <= 48; i+=4)
            {
                Vector3 mauw = p.TranslateToScreen(
                    temp.X + (1.2f *
                        Math.Sin(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    ),
                    temp.Y,
                    temp.Z + (1.2f *
                        Math.Cos(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    )
                );
                Vector3 muaw = p.TranslateToScreen(
                    temp.X + (1.2f *
                        Math.Sin(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    ),
                    temp.Y + 1.2f + (Math.Sin(turn + ((i / 48.0) * Math.PI) )),
                    temp.Z + (1.2f *
                        Math.Cos(
                            ((Math.PI / 24.0) * i)
                            + turn
                        )
                    )
                );
                ImGui.GetWindowDrawList().AddLine(
                    new Vector2(mauw.X, mauw.Y),
                    new Vector2(muaw.X, muaw.Y),
                    ImGui.GetColorU32(acol),
                    10.0f
                );
            }
        }

    }

}
