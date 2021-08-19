using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.FileProviders;
using PokeGlitzer.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    static class Settings
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static bool Text_japaneseCharset { get; private set; }

        public static string MMF_PC_IN { get; private set; }
        public static string MMF_PC_OUT { get; private set; }
        public static string MMF_PARTY_IN { get; private set; }
        public static string MMF_PARTY_OUT { get; private set; }

        public static uint Corruption_gPokemonStorage { get; private set; }
        public static uint Corruption_gPlayerParty { get; private set; }
        public static byte Corruption_aslrMask { get; private set; }
        public static byte Corruption_numberUpPresses { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void Initialize()
        {
            NumberToStringConverter conv = new NumberToStringConverter();

            IniConfigurationSource src = new IniConfigurationSource();
            src.Path = "config.ini";
            src.FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
            IniConfigurationProvider prov = new IniConfigurationProvider(src);
            prov.Load();
            // Text
            prov.TryGet("Text:japaneseCharset", out string text_japanese);
            Text_japaneseCharset = Convert.ToBoolean(text_japanese);
            // MMF
            prov.TryGet("MMF:PC_in", out string mmf_pc_in);
            prov.TryGet("MMF:PC_out", out string mmf_pc_out);
            prov.TryGet("MMF:party_in", out string mmf_party_in);
            prov.TryGet("MMF:party_out", out string mmf_party_out);
            MMF_PC_IN = mmf_pc_in; MMF_PC_OUT = mmf_pc_out; MMF_PARTY_IN = mmf_party_in; MMF_PARTY_OUT = mmf_party_out;
            // Corruption
            prov.TryGet("Corruption:gPokemonStorage", out string gPokemonStorage);
            prov.TryGet("Corruption:gPlayerParty", out string gPlayerParty);
            prov.TryGet("Corruption:aslrMask", out string aslrMask);
            prov.TryGet("Corruption:numberUpPresses", out string numberUpPresses);
            Corruption_gPokemonStorage = Utils.ToNumber<uint>(gPokemonStorage);
            Corruption_gPlayerParty = Utils.ToNumber<uint>(gPlayerParty);
            Corruption_aslrMask = Utils.ToNumber<byte>(aslrMask);
            Corruption_numberUpPresses = Utils.ToNumber<byte>(numberUpPresses);
        }
    }
}
