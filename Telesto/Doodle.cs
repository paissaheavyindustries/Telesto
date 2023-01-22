using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Common.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Telesto
{

    internal abstract class Doodle
    {

        internal class Coordinate
        {

            internal enum CoordinateTypeEnum
            {
                Screen,
                World,
                Entity
            }


            internal CoordinateTypeEnum ct { get; set; }
            internal Vector3 cp { get; set; }

            internal string X { get; set; }
            internal string Y { get; set; }
            internal string Z { get; set; }
            internal ulong id { get; set; }
            internal string name { get; set; }

            internal void RefreshVector(Plugin p)
            {
                switch (ct)
                {
                    case CoordinateTypeEnum.Screen:
                        cp = new Vector3(
                            (float)p.EvaluateNumericExpression(X),
                            (float)p.EvaluateNumericExpression(Y),
                            (float)p.EvaluateNumericExpression(Z)
                        );
                        break;
                    case CoordinateTypeEnum.World:
                        cp = p.TranslateToScreen(
                            p.EvaluateNumericExpression(X),
                            p.EvaluateNumericExpression(Y),
                            p.EvaluateNumericExpression(Z)
                        );
                        break;
                    case CoordinateTypeEnum.Entity:
                        GameObject go;
                        if (id > 0)
                        {
                            go = p.GetEntityById(id);
                        }
                        else
                        {
                            go = p.GetEntityByName(name);
                        }
                        if (go != null)
                        {
                            cp = p.TranslateToScreen(
                                go.Position.X,
                                go.Position.Y,
                                go.Position.Z
                            );
                        }
                        else
                        {
                            cp = p.TranslateToScreen(
                                p.EvaluateNumericExpression(X),
                                p.EvaluateNumericExpression(Y),
                                p.EvaluateNumericExpression(Z)
                            );
                        }
                        break;
                }
            }

            internal void Initialize(Dictionary<string, object> d)
            {
                string coords = (d.ContainsKey("coords") == true) ? d["coords"].ToString() : "screen";
                switch (coords)
                {
                    case "world":
                        ct = CoordinateTypeEnum.World;
                        break;
                    case "entity":
                        ct = CoordinateTypeEnum.Entity;
                        break;
                    default:
                        ct = CoordinateTypeEnum.Screen;
                        break;
                }
                X = (d.ContainsKey("x") == true) ? d["x"].ToString() : "0";
                Y = (d.ContainsKey("y") == true) ? d["y"].ToString() : "0";
                Z = (d.ContainsKey("z") == true) ? d["z"].ToString() : "0";
                id = 0;
                if (d.ContainsKey("id") == true)
                {
                    ulong honk;
                    if (ulong.TryParse(d["id"].ToString(), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out honk) == true)
                    {
                        id = honk;
                    }
                }
                if (id == 0)
                {
                    name = (d.ContainsKey("name") == true) ? d["name"].ToString() : "";
                }
            }

        }

        [Flags]
        internal enum ExpiryTypeEnum
        {
            Timed = 0x01,
            OnDeath = 0x02,
            OnWipe = 0x04,
            OnZoneChange = 0x08,
            NotLoggedIn = 0x10
        }

        internal Plugin p { get; set; }
        internal string Name { get; set; }
        internal DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddSeconds(5);
        internal ExpiryTypeEnum ExpiresOn { get; set; } = ExpiryTypeEnum.Timed | ExpiryTypeEnum.NotLoggedIn | ExpiryTypeEnum.OnZoneChange | ExpiryTypeEnum.OnWipe;
        internal Vector4 col { get; set; } = new Vector4();

        internal string R { get; set; }
        internal string G { get; set; }
        internal string B { get; set; }
        internal string A { get; set; }

        internal virtual void Initialize(Dictionary<string, object> d)
        {
            Name = d["name"].ToString();
            R = (d.ContainsKey("r") == true) ? d["r"].ToString() : "0";
            G = (d.ContainsKey("g") == true) ? d["g"].ToString() : "0";
            B = (d.ContainsKey("b") == true) ? d["b"].ToString() : "0";
            A = (d.ContainsKey("a") == true) ? d["a"].ToString() : "1";
            if (d.ContainsKey("expireson") == true)
            {
                ExpiryTypeEnum nt = 0;
                string[] vals = d["expireson"].ToString().Split(",", StringSplitOptions.TrimEntries);
                foreach (string val in vals)
                {
                    switch (val)
                    {
                        case "timed": nt |= ExpiryTypeEnum.Timed; break;
                        case "ondeath": nt |= ExpiryTypeEnum.OnDeath; break;
                        case "onwipe": nt |= ExpiryTypeEnum.OnWipe; break;
                        case "onzonechange": nt |= ExpiryTypeEnum.OnZoneChange; break;
                        case "notloggedin": nt |= ExpiryTypeEnum.NotLoggedIn; break;
                    }
                }
                ExpiresOn = nt;
            }
            if (d.ContainsKey("expiresin") == true)
            {
                int ms = Int32.Parse(d["expiresin"].ToString());
                ExpiresAt = DateTime.UtcNow.AddMilliseconds(ms);
                ExpiresOn |= ExpiryTypeEnum.Timed;
            }
        }

        internal static Doodle Deserialize(Dictionary<string, object> d)
        {
            string type = d["type"].ToString();
            Doodle doo = null;
            switch (type)
            {
                case "line":
                    doo = new Doodles.Line();
                    break;
                case "text":
                    doo = new Doodles.Text();
                    break;
                case "circle":
                    doo = new Doodles.Circle();
                    break;
            }
            if (doo != null)
            {
                doo.Initialize(d);
            }
            return doo;
        }

        internal virtual bool Update()
        {
            if ((ExpiresOn & ExpiryTypeEnum.Timed) != 0)
            {
                if (DateTime.UtcNow >= ExpiresAt)
                {
                    return false;
                }
            }
            col = new Vector4(
                (float)p.EvaluateNumericExpression(R),
                (float)p.EvaluateNumericExpression(G),
                (float)p.EvaluateNumericExpression(B),
                (float)p.EvaluateNumericExpression(A)
            );
            return true;
        }

        internal abstract void Draw();

    }

}
