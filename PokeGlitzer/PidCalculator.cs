using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public class PidCalculator
    {
        public static uint HL(ushort h, ushort l) { return (uint)(h << 16) | l; }
        const int MAX_TRIES = 0x1000;
        public enum PkmnGender { Unknown, Male, Female }
        public static PkmnGender GenderOfPID(uint pid, ushort species)
        {
            int s = SpeciesConverter.SetG3Species(species);
            if (s == 0) return PkmnGender.Unknown;
            byte threshold = PersonalInfo.Table[s].Gender;
            if (threshold == 255) return PkmnGender.Unknown;
            if (threshold == 254) return PkmnGender.Female;
            if (threshold == 0) return PkmnGender.Male;
            return (pid & 0xFF) >= threshold ? PkmnGender.Male : PkmnGender.Female;
        }
        public static byte NatureOfPID(uint PID)
        {
            return (byte)(PID % TextData.FILTERED_NATURES.Length);
        }
        public static byte AbilityOfPID(uint PID, ushort species)
        {
            int s = SpeciesConverter.SetG3Species(species);
            if (s != 0 && !PersonalInfo.Table[s].HasSecondAbility) return 0;
            return (byte)(PID & 0x1);
        }
        public static bool ShinyOfPID(uint pid, uint otid)
        {
            ushort p1, p2, trainer, secret;
            trainer = (ushort)(otid & 0xFFFF);
            secret = (ushort)(otid >> 16);
            p1 = (ushort)(pid & 0xFFFF);
            p2 = (ushort)(pid >> 16);
            return (trainer ^ secret ^ p1 ^ p2) < 8;
        }
        public static uint? GenerateNewPID(uint otid, ushort species, bool shiny, PkmnGender gender, byte nature, byte ability)
        {
            var rand = new Random();
            ushort p1, p2, trainer, secret;
            for (int count = 0; count < MAX_TRIES; count++)
            {
                if (count > MAX_TRIES) break;
                p1 = (ushort)rand.Next(0, 0x10000 >> 3);
                // Generating p2 such that the shininess modulus is < 8
                trainer = (ushort)(otid & 0xFFFF);
                secret = (ushort)(otid >> 16);
                p2 = (ushort)(p1 ^ (trainer >> 3) ^ (secret >> 3));
                // If non-shiny, randomize p2 to any other value (so that the shininess modulus will be >= 8)
                if (!shiny) p2 = (ushort)(p2 ^ (rand.Next(1, 0x10000 >> 3)));
                // Generate 128 possibilities preserving shininess
                p1 = (ushort)(p1 << 3);
                p2 = (ushort)(p2 << 3);
                int l = 1 << 3;
                int m = l * l;
                uint[] a = new uint[m*2];
                for (int p1l = 0; p1l < l; p1l++)
                {
                    for (int p2l = 0; p2l < l; p2l++)
                    {
                        a[p2l * l + p1l] = HL((ushort)(p1 | p1l), (ushort)(p2 | p2l));
                        a[m + p2l * l + p1l] = HL((ushort)(p2 | p2l), (ushort)(p1 | p1l));
                    }
                }
                rand.Shuffle(a);
                // Check them
                for (int i = 0; i < a.Length; ++i)
                {
                    if (GenderOfPID(a[i], species) == gender && AbilityOfPID(a[i], species) == ability && NatureOfPID(a[i]) == nature)
                        return a[i];
                }
            }
            return null;
        }
    }
}
