using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    class PersonalInfo
    {
        const int SIZE = 0x1C;
        public static void Initialize()
        {
            var data = Settings.Game_version switch {
                Settings.GameVersion.Emerald => Properties.Resources.personal_e,
                Settings.GameVersion.FireRed => Properties.Resources.personal_fr,
                Settings.GameVersion.LeafGreen => Properties.Resources.personal_lg,
                Settings.GameVersion.Ruby => Properties.Resources.personal_rs,
                Settings.GameVersion.Sapphire => Properties.Resources.personal_rs,
                _ => Properties.Resources.personal_e
            };
            var count = data.Length / SIZE;
            PersonalInfo[] table = new PersonalInfo[count];
            for (int i = 0; i < table.Length; i++)
                table[i] = new PersonalInfo(data.Slice(SIZE * i, SIZE).ToArray());
            Table = table;
        }
        public static string[] ABILITIES = Properties.Resources.abilities.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static PersonalInfo[] Table { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Taken from https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/PersonalInfo/Info/PersonalInfo3.cs
        byte[] data;
        public PersonalInfo(byte[] data)
        {
            this.data = data;
        }
        public byte Gender { get => data[0x10]; }
        public byte EXPGrowth { get => data[0x13]; }
        public int Ability1 { get => data[0x16]; }
        public int Ability2 { get => data[0x17]; }
        public bool HasSecondAbility { get => Ability1 != Ability2; }
    }
}
