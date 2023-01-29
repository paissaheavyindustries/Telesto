using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Telesto.Subscriptions
{

    internal class Memory : Subscription
    {

        public string start { get; set; }
        public string length { get; set; }
        public string representation { get; set; }

        internal nint startaddr { get; set; }
        internal int bytelen { get; set; }

        private class State
        {

            public byte[] olddata { get; set; }
            public byte[] newdata { get; set; }

        }

        private Dictionary<nint, State> states = new Dictionary<nint, State>();

        internal override bool Refresh(Context ctx)
        {
            if (ctx.go == null)
            {
                return false;
            }
            nint baseaddr = ctx.go.Address;
            State s = null;
            if (states.ContainsKey(baseaddr) == true)
            {
                s = states[baseaddr];
            }
            else
            {
                s = new State();
                states[baseaddr] = s;
            }
            s.olddata = s.newdata;
            startaddr = (nint)p.EvaluateNumericExpression(ctx, start);
            bytelen = (int)p.EvaluateNumericExpression(ctx, length);
            byte[] tenp = new byte[bytelen];
            try
            {
                Marshal.Copy(startaddr, tenp, 0, bytelen);
            }
            catch (Exception)
            {
                return false;
            }
            s.newdata = tenp;
            if (s.olddata == null)
            {
                return true;
            }
            if (s.olddata.Length != s.newdata.Length)
            {
                return true;
            }
            ReadOnlySpan<byte> a = s.olddata;
            ReadOnlySpan<byte> b = s.newdata;
            return (a.SequenceEqual(b) == false);
        }

        internal override void GetRepresentation(Context ctx, out string oldrep, out string newrep)
        {
            if (ctx.go == null)
            {
                oldrep = "";
                newrep = "";
                return;
            }
            State s = null;
            nint baseaddr = ctx.go.Address;
            if (states.ContainsKey(baseaddr) == true)
            {
                s = states[baseaddr];
            }
            else
            {
                oldrep = "";
                newrep = "";
                return;
            }
            string actrep = "";
            if (representation == "")
            {
                switch (bytelen)
                {
                    case 1:
                        actrep = "byte";                        
                        break;
                    case 2:
                        actrep = "ushort"; 
                        break;
                    case 4:
                        actrep = "uint";
                        break;
                    case 8:
                        actrep = "ulong";
                        break;
                    default:
                        actrep = "string";
                        break;
                }
            }
            else
            {
                actrep = representation;
            }            
            object convold = s.olddata != null ? ConvertToType(s.olddata, actrep) : null;
            object convnew = ConvertToType(s.newdata, actrep);
            oldrep = convold != null ? convold.ToString() : "";
            newrep = convnew.ToString();
        }

        internal object ConvertToType(byte[] buf, string type)
        {
            switch (type.ToLower())
            {
                case "bool": return BitConverter.ToBoolean(buf);
                case "byte": return buf[0];
                case "char": return BitConverter.ToChar(buf);
                case "double": return BitConverter.ToDouble(buf);
                case "short": return BitConverter.ToInt16(buf);
                case "int": return BitConverter.ToInt32(buf);
                case "long": return BitConverter.ToInt64(buf);
                case "float": return BitConverter.ToSingle(buf);
                case "ushort": return BitConverter.ToUInt16(buf);
                case "uint": return BitConverter.ToUInt32(buf);
                case "ulong": return BitConverter.ToUInt64(buf);
                case "string": return BitConverter.ToString(buf);
            }
            return buf;
        }

    }

}
