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
            var data = Properties.Resources.personal_e;
            var count = data.Length / SIZE;
            PersonalInfo[] table = new PersonalInfo[count];
            for (int i = 0; i < table.Length; i++)
                table[i] = new PersonalInfo(data.Slice(SIZE * i, SIZE).ToArray());
            Table = table;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static PersonalInfo[] Table { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Taken from https://github.com/kwsch/PKHeX/blob/fc754b346bf7d83ac400974922a3846d7cc267ed/PKHeX.Core/PersonalInfo/PersonalInfoG3.cs
        byte[] data;
        public PersonalInfo(byte[] data)
        {
            this.data = data;
        }
        public byte Gender { get => data[0x10]; }
    }
}
