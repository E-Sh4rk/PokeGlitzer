using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    [StructLayout(LayoutKind.Sequential/*Explicit*/, Size = 80, Pack = 1)]
    struct PokemonStruct
    {
        //[FieldOffset(0)]
        public uint PID;
        //[FieldOffset(4)]
        public uint OTID;
        //[FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] nickname;
        //[FieldOffset(18)]
        public byte lang;
        //[FieldOffset(19)]
        public byte isEgg;
        //[FieldOffset(20)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] originalTrainerName;
        //[FieldOffset(27)]
        public byte markings;
        //[FieldOffset(28)]
        public ushort checksum;
        //[FieldOffset(30)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] unused;
        //[FieldOffset(32)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12 * 4)]
        public byte[] data;
    }
}
