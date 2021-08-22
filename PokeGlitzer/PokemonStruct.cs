using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    [StructLayout(LayoutKind.Sequential, Size = 100, Pack = 1)]
    struct PokemonTeamStruct
    {
        public const byte SLEEP_MASK = 0b0000_0111;
        public const byte POISON_MASK = 0b0000_1000;
        public const byte BURN_MASK = 0b0001_0000;
        public const byte FREEZE_MASK = 0b0010_0000;
        public const byte PARALYSIS_MASK = 0b0100_0000;
        public const byte BAD_POISON_MASK = 0b1000_0000;

        public PokemonStruct permanent;
        public uint status;
        public byte level;
        public byte pokerusRemaining;
        public ushort currentHP;
        public ushort maxHP;
        public ushort attack;
        public ushort defense;
        public ushort speed;
        public ushort speAttack;
        public ushort speDefense;
    }
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
        const byte ppb_mask = 0b00000011;
        const int ppb1s = 0;
        const int ppb2s = 2;
        const int ppb3s = 4;
        const int ppb4s = 6;
        public byte Ppb1 { get => (byte)((ppBonuses >> ppb1s) & ppb_mask); }
        public byte Ppb2 { get => (byte)((ppBonuses >> ppb2s) & ppb_mask); }
        public byte Ppb3 { get => (byte)((ppBonuses >> ppb3s) & ppb_mask); }
        public byte Ppb4 { get => (byte)((ppBonuses >> ppb4s) & ppb_mask); }
        public void SetPpBonuses(byte ppb1, byte ppb2, byte ppb3, byte ppb4)
        {
            ppBonuses  = (byte)((ppb1 & ppb_mask) << ppb1s);
            ppBonuses |= (byte)((ppb2 & ppb_mask) << ppb2s);
            ppBonuses |= (byte)((ppb3 & ppb_mask) << ppb3s);
            ppBonuses |= (byte)((ppb4 & ppb_mask) << ppb4s);
        }

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
        public const ushort GENDER_MASK = 0b1000_0000_0000_0000;
        public const ushort LEVEL_MET_MASK = 0b0000_0000_0111_1111;
        public const uint EGG_MASK = 0b0100_0000_0000_0000_0000_0000_0000_0000;
        public const uint ABILITY_MASK = 0b1000_0000_0000_0000_0000_0000_0000_0000;
        public const uint OBEDIENCE_MASK = 0b1000_0000_0000_0000_0000_0000_0000_0000;
        public const uint RIBBONS_MASK = ~OBEDIENCE_MASK; 
        const uint ivs_mask = 0b00111111_11111111_11111111_11111111;
        const uint iv_mask = 0b00011111;
        const int iv_hp = 0;
        const int iv_atk = 5;
        const int iv_def = 10;
        const int iv_speed = 15;
        const int iv_sp_atk = 20;
        const int iv_sp_def = 25;
        const byte pokerus_mask = 0b00001111;
        const int pokerus_days = 0;
        const int pokerus_strain = 4;
        const ushort goo_mask = 0b1111;
        const int goo = 7;
        const ushort ball_mask = 0b1111;
        const int ball = 11;
        public byte IV_hp { get => (byte)((ivEggAbility >> iv_hp) & iv_mask); }
        public byte IV_atk { get => (byte)((ivEggAbility >> iv_atk) & iv_mask); }
        public byte IV_def { get => (byte)((ivEggAbility >> iv_def) & iv_mask); }
        public byte IV_speed { get => (byte)((ivEggAbility >> iv_speed) & iv_mask); }
        public byte IV_sp_atk { get => (byte)((ivEggAbility >> iv_sp_atk) & iv_mask); }
        public byte IV_sp_def { get => (byte)((ivEggAbility >> iv_sp_def) & iv_mask); }
        public void SetIVs(byte hp, byte atk, byte def, byte speed, byte sp_atk, byte sp_def)
        {
            ivEggAbility &= ~ivs_mask;
            ivEggAbility |= (hp & iv_mask) << iv_hp;
            ivEggAbility |= (atk & iv_mask) << iv_atk;
            ivEggAbility |= (def & iv_mask) << iv_def;
            ivEggAbility |= (speed & iv_mask) << iv_speed;
            ivEggAbility |= (sp_atk & iv_mask) << iv_sp_atk;
            ivEggAbility |= (sp_def & iv_mask) << iv_sp_def;
        }
        public byte PokerusDays { get => (byte)((pokerus >> pokerus_days) & pokerus_mask); }
        public byte PokerusStrain { get => (byte)((pokerus >> pokerus_strain) & pokerus_mask); }
        public byte GameOfOrigin
        {
            get => (byte)((origins >> goo) & goo_mask);
            set => origins = (byte)((origins & ~(goo_mask << goo)) | (value & goo_mask) << goo);
        }
        public byte Ball
        {
            get => (byte)((origins >> ball) & ball_mask);
            set => origins = (byte)((origins & ~(ball_mask << ball)) | (value & ball_mask) << ball);
        }
        public void SetPokerus(byte days, byte strain)
        {
            pokerus  = (byte)((days & pokerus_mask) << pokerus_days);
            pokerus |= (byte)((strain & pokerus_mask) << pokerus_strain);
        }

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
    [StructLayout(LayoutKind.Sequential, Size = 610, Pack = 1)]
    public struct TeamItemsData
    {
        public const int SECTION_ID = 1;
        public const int SECTION_OFFSET_RSE = 0x0234;
        public const int SECTION_OFFSET_FRLG = 0x0034;

        public uint teamSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 600)]
        public byte[] pokemonList;
        public uint money;
        public ushort coins;
    }
    [StructLayout(LayoutKind.Sequential, Size = 0xB0, Pack = 1)]
    public struct TrainerInfoData
    {
        public const int SECTION_ID = 0;
        public const int GAMECODE_RS = 0;
        public const int GAMECODE_FRLG = 1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        public byte[] playerName;
        public byte playerGender;
        public byte unused1;
        public uint trainedID;
        public ushort hoursPlayed;
        public byte minutesPlayed;
        public byte secondsPlayed;
        public byte framesPlayed;
        public byte options1;
        public byte options2;
        public byte options3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x96)]
        public byte[] unused2;
        public uint gameCode;
    }
}
