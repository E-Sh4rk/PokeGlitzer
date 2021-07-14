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
        public const int SUBSTRUCTURE_SIZE = 12;
        public const byte IS_BAD_EGG_MASK = 0b0000_0001;
        public const byte HAS_SPECIES_MASK = 0b0000_0010;
        public const byte IS_EGG_MASK = 0b0000_0100;

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
    [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
    struct Substructure0
    {
        public ushort species;
        public ushort item;
        public uint experience;
        public byte ppBonuses;
        public byte friendship;
        public ushort filler;
    }
    [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
    struct Substructure1
    {
        public ushort move1;
        public ushort move2;
        public ushort move3;
        public ushort move4;
        public byte pp1;
        public byte pp2;
        public byte pp3;
        public byte pp4;
    }
    [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
    struct Substructure2
    {
        public byte hpEV;
        public byte atkEV;
        public byte defEV;
        public byte speedEV;
        public byte spAtkEV;
        public byte spDefEV;
        public byte coolness;
        public byte beauty;
        public byte cuteness;
        public byte smartness;
        public byte toughness;
        public byte feel;
    }
    [StructLayout(LayoutKind.Sequential, Size = 12, Pack = 1)]
    struct Substructure3
    {
        public const uint EGG_MASK = 0b0100_0000_0000_0000_0000_0000_0000_0000;

        public byte pokerus;
        public byte metLocation;
        public ushort origins;
        public uint ivEggAbility;
        public uint ribbonsObedience;
    }

    // ========== SAVE FILE ==========

    [StructLayout(LayoutKind.Sequential, Size = 0x020000, Pack = 1)]
    struct PokemonSaveFile
    {
        //[FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public Section[] sectionsA;
        //[FieldOffset(0x00E000)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public Section[] sectionsB;
        //[FieldOffset(0x01C000)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8192)]
        public byte[] hallOfFame;
        //[FieldOffset(0x01E000)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] mysteryGift;
        //[FieldOffset(0x01F000)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public byte[] recordedBattle;
    }
    [StructLayout(LayoutKind.Sequential, Size = 0x001000, Pack = 1)]
    struct Section
    {
        public const uint MAGIC_NUMBER = 0x08012025;
        public static readonly int[] DATA_SIZE = new int[] { 3884, 3968, 3968, 3968, 3848, 3968, 3968, 3968, 3968, 3968, 3968, 3968, 3968, 2000 };
        //[FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xFF4)]
        public byte[] data;
        //[FieldOffset(0xFF4)]
        public ushort sectionID;
        //[FieldOffset(0xFF6)]
        public ushort checksum;
        //[FieldOffset(0xFF8)]
        public uint signature;
        //[FieldOffset(0xFFC)]
        public uint saveIndex;
    }
    [StructLayout(LayoutKind.Sequential, Size = 0x83D0, Pack = 1)]
    public struct PCData
    {
        public const int MIN_SECTION_ID = 5;
        public const int MAX_SECTION_ID = 13;

        public uint currentPCBox;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8340)]
        public byte[] pokemonList;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x7E)]
        public byte[] boxNames;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE)]
        public byte[] boxWallpapers;
    }
}
