using System.Runtime.InteropServices;

namespace Telesto.Interop
{

    // structures from PunishedPineapple/WaymarkPresetPlugin
    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public class Waymark
    {
        [FieldOffset(0x00)] public float X_Float;
        [FieldOffset(0x04)] public float Y_Float;
        [FieldOffset(0x08)] public float Z_Float;
        [FieldOffset(0x10)] public int X_Int;
        [FieldOffset(0x14)] public int Y_Int;
        [FieldOffset(0x18)] public int Z_Int;

        [MarshalAs(UnmanagedType.Bool)]
        [FieldOffset(0x1C)] public bool Active;

        public Waymark Duplicate()
        {
            return (Waymark)MemberwiseClone();
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 0x100)]
    public class Waymarks
    {

        public Waymark A;
        public Waymark B;
        public Waymark C;
        public Waymark D;
        public Waymark One;
        public Waymark Two;
        public Waymark Three;
        public Waymark Four;

    }

}
