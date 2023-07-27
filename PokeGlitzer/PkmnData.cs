using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    // Most of these classes are taken from PKHeX:
    // https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/PKM/Strings/StringConverter3.cs
    // https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/PKM/Util/Experience.cs

    /// <summary>
    /// Logic for converting a <see cref="string"/> for Generation 3.
    /// </summary>
    public static class StringConverter
    {
        private const byte TerminatorByte = 0xFF;
        private const char Terminator = (char)TerminatorByte;
        static int ControlStart { get => 0xFA; }

        private static readonly char[] G3_EN =
        {
            ' ',  'À',  'Á',  'Â', 'Ç',  'È',  'É',  'Ê',  'Ë',  'Ì', ' ', 'Î',  'Ï',  'Ò',  'Ó',  'Ô', // 0
            'Œ',  'Ù',  'Ú',  'Û', 'Ñ',  'ß',  'à',  'á',  ' ', 'ç',  'è', 'é',  'ê',  'ë',  'ì',  'í', // 1
            'î',  'ï',  'ò',  'ó', 'ô',  'œ',  'ù',  'ú',  'û',  'ñ',  'º', 'ª',  ' ', '&',  '+',  ' ', // 2
            ' ',  ' ',  ' ',  ' ', 'ˡ',  '=',  ';',  ' ',  ' ',  ' ',  ' ', ' ',  ' ', ' ',  ' ',  ' ', // 3
            ' ',  ' ',  ' ',  ' ', ' ',  ' ',  ' ',  ' ',  ' ',  ' ',  ' ', ' ',  ' ', ' ',  ' ',  ' ', // 4
            '▯',  '¿',  '¡',  ' ', ' ',  ' ',  ' ',  ' ',  ' ',  ' ',  'Í',  '%', '(', ')',  ' ',  ' ', // 5
            ' ',  ' ',  ' ',  ' ', ' ',  ' ',  ' ',  ' ',  'â',  ' ',  ' ', ' ',  ' ', ' ',  ' ',  'í', // 6
            ' ',  ' ',  ' ',  ' ', ' ',  ' ',  ' ',  ' ',  ' ',  '⬆',  '⬇', '⬅', '⮕', '*',  '*',  '*', // 7
            '*',  '*',  '*',  '*', 'ᵉ',  '<',  '>',  ' ',  ' ',  ' ',  ' ', ' ',  ' ', ' ',  ' ',  ' ', // 8
            ' ',  ' ',  ' ',  ' ', ' ',  ' ',  ' ',  ' ',  ' ',  ' ',  ' ', ' ',  ' ', ' ',  ' ',  ' ', // 9
            ' ',  '0',  '1',  '2', '3',  '4',  '5',  '6',  '7',  '8',  '9',  '!', '?',  '.',  '-', '・', // A
            '…',  '“',  '”',  '‘', '’',  '♂',  '♀',  '$',  ',',  '×',  '/',  'A', 'B',  'C',  'D',  'E', // B
            'F',  'G',  'H',  'I', 'J',  'K',  'L',  'M',  'N',  'O',  'P',  'Q', 'R',  'S',  'T',  'U', // C
            'V',  'W',  'X',  'Y', 'Z',  'a',  'b',  'c',  'd',  'e',  'f',  'g', 'h',  'i',  'j',  'k', // D
            'l',  'm',  'n',  'o', 'p',  'q',  'r',  's',  't',  'u',  'v',  'w', 'x',  'y',  'z',  '▶', // E
            ':',  'Ä',  'Ö',  'Ü', 'ä',  'ö',  'ü',  ' ',  ' ',  ' ',                                    // F
            // Make the total length 256 so that any byte access is always within the array
            Terminator, Terminator, Terminator, Terminator, Terminator, Terminator
        };

        private static readonly char[] G3_JP =
        {
            '　', 'あ', 'い', 'う', 'え', 'お', 'か', 'き', 'く', 'け', 'こ', 'さ', 'し', 'す', 'せ', 'そ', // 0
            'た', 'ち', 'つ', 'て', 'と', 'な', 'に', 'ぬ', 'ね', 'の', 'は', 'ひ', 'ふ', 'へ', 'ほ', 'ま', // 1
            'み', 'む', 'め', 'も', 'や', 'ゆ', 'よ', 'ら', 'り', 'る', 'れ', 'ろ', 'わ', 'を', 'ん', 'ぁ', // 2
            'ぃ', 'ぅ', 'ぇ', 'ぉ', 'ゃ', 'ゅ', 'ょ', 'が', 'ぎ', 'ぐ', 'げ', 'ご', 'ざ', 'じ', 'ず', 'ぜ', // 3
            'ぞ', 'だ', 'ぢ', 'づ', 'で', 'ど', 'ば', 'び', 'ぶ', 'べ', 'ぼ', 'ぱ', 'ぴ', 'ぷ', 'ぺ', 'ぽ', // 4
            'っ', 'ア', 'イ', 'ウ', 'エ', 'オ', 'カ', 'キ', 'ク', 'ケ', 'コ', 'サ', 'シ', 'ス', 'セ', 'ソ', // 5
            'タ', 'チ', 'ツ', 'テ', 'ト', 'ナ', 'ニ', 'ヌ', 'ネ', 'ノ', 'ハ', 'ヒ', 'フ', 'ヘ', 'ホ', 'マ', // 6
            'ミ', 'ム', 'メ', 'モ', 'ヤ', 'ユ', 'ヨ', 'ラ', 'リ', 'ル', 'レ', 'ロ', 'ワ', 'ヲ', 'ン', 'ァ', // 7
            'ィ', 'ゥ', 'ェ', 'ォ', 'ャ', 'ュ', 'ョ', 'ガ', 'ギ', 'グ', 'ゲ', 'ゴ', 'ザ', 'ジ', 'ズ', 'ゼ', // 8
            'ゾ', 'ダ', 'ヂ', 'ヅ', 'デ', 'ド', 'バ', 'ビ', 'ブ', 'ベ', 'ボ', 'パ', 'ピ', 'プ', 'ペ', 'ポ', // 9
            'ッ', '０', '１', '２', '３', '４', '５', '６', '７', '８', '９', '！', '？', '。', '－', '・', // A
            '‥',  '『', '』', '「', '」', '♂',  '♀',  '＄', '．', '×', '／', 'Ａ', 'Ｂ', 'Ｃ', 'Ｄ', 'Ｅ', // B
            'Ｆ', 'Ｇ', 'Ｈ', 'Ｉ', 'Ｊ', 'Ｋ', 'Ｌ', 'Ｍ', 'Ｎ', 'Ｏ', 'Ｐ', 'Ｑ', 'Ｒ', 'Ｓ', 'Ｔ', 'Ｕ', // C
            'Ｖ', 'Ｗ', 'Ｘ', 'Ｙ', 'Ｚ', 'ａ', 'ｂ', 'ｃ', 'ｄ', 'ｅ', 'ｆ', 'ｇ', 'ｈ', 'ｉ', 'ｊ', 'ｋ', // D
            'ｌ', 'ｍ', 'ｎ', 'ｏ', 'ｐ', 'ｑ', 'ｒ', 'ｓ', 'ｔ', 'ｕ', 'ｖ', 'ｗ', 'ｘ', 'ｙ', 'ｚ', '▶',  // E
            '：',  'Ä',  'Ö',  'Ü',  'ä',  'ö', 'ü', '⬆',  '⬇',  '⬅',                                   // F
            // Make the total length 256 so that any byte access is always within the array
            Terminator, Terminator, Terminator, Terminator, Terminator, Terminator
        };

        /// <summary>
        /// Decodes a character from a Generation 3 encoded value.
        /// </summary>
        /// <param name="chr">Generation 4 decoded character.</param>
        /// <param name="jp">Character destination is Japanese font.</param>
        /// <returns>Generation 3 encoded value.</returns>
        private static char GetG3Char(byte chr, bool jp)
        {
            var table = jp ? G3_JP : G3_EN;
            return table[chr];
        }

        /// <summary>
        /// Encodes a character to a Generation 3 encoded value.
        /// </summary>
        /// <param name="chr">Generation 4 decoded character.</param>
        /// <param name="jp">Character destination is Japanese font.</param>
        /// <returns>Generation 3 encoded value.</returns>
        private static byte SetG3Char(char chr, bool jp)
        {
            /*if (chr == '\'') // ’
                return 0xB4;*/
            var table = jp ? G3_JP : G3_EN;
            var index = Array.IndexOf(table, chr);
            if (index == -1)
                return TerminatorByte;
            return (byte)index;
        }

        public static bool IsCharValid(char chr, bool jp)
        {
            byte b = SetG3Char(chr, jp);
            return b != TerminatorByte;
        }

        /// <summary>
        /// Converts a Generation 3 encoded value array to string.
        /// </summary>
        /// <param name="data">Byte array containing string data.</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="count">Length of data to read.</param>
        /// <param name="jp">Value source is Japanese font.</param>
        /// <returns>Decoded string.</returns>
        public static string GetString3(byte[] data, int offset, int count, bool jp) => GetString3(data.AsSpan(offset, count), jp);

        /// <summary>
        /// Converts a Generation 3 encoded value array to string.
        /// </summary>
        /// <param name="data">Byte array containing string data.</param>
        /// <param name="jp">Value source is Japanese font.</param>
        /// <returns>Decoded string.</returns>
        public static string GetString3(ReadOnlySpan<byte> data, bool jp)
        {
            var s = new StringBuilder(data.Length);
            foreach (var val in data)
            {
                var c = GetG3Char(val, jp); // Convert to Unicode
                if (c == Terminator) // Stop if Terminator/Invalid
                    break;
                s.Append(c);
            }
            return s.ToString();
        }

        /// <summary>
        /// Converts a string to a Generation 3 encoded value array.
        /// </summary>
        /// <param name="value">Decoded string.</param>
        /// <param name="maxLength">Maximum length of the input <see cref="value"/></param>
        /// <param name="jp">String destination is Japanese font.</param>
        /// <param name="padTo">Pad the input <see cref="value"/> to given length</param>
        /// <param name="padWith">Pad the input <see cref="value"/> with this character value</param>
        /// <returns>Encoded data.</returns>
        public static byte[] SetString3(string value, int maxLength, bool jp, int padTo = 0, ushort padWith = 0)
        {
            if (value.Length > maxLength)
                value = value[..maxLength]; // Hard cap
            var data = new byte[value.Length + 1]; // +1 for Terminator
            for (int i = 0; i < value.Length; i++)
            {
                var chr = value[i];
                var val = SetG3Char(chr, jp);
                if (val == Terminator) // end
                {
                    Array.Resize(ref data, i + 1);
                    break;
                }
                data[i] = val;
            }
            if (data.Length > 0)
                data[^1] = TerminatorByte;
            if (data.Length > maxLength && padTo <= maxLength)
            {
                // Truncate
                Array.Resize(ref data, maxLength);
                return data;
            }
            if (data.Length < padTo)
            {
                var start = data.Length;
                Array.Resize(ref data, padTo);
                for (int i = start; i < data.Length; i++)
                    data[i] = (byte)padWith;
            }
            return data;
        }
    }

    public static class TextData
    {
        const int MAX_MOVE_ID = 354;
        public static string[] MOVES = Properties.Resources.moves.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        public static string[] FILTERED_MOVES = MOVES.Where((s, i) => i <= MAX_MOVE_ID && !string.IsNullOrEmpty(s)).ToArray();
        public static string[] MOVES_LOWERCASE = MOVES.Select(str => str.ToLowerInvariant()).ToArray();
        public static string[] NATURES = Properties.Resources.natures.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        public static string[] FILTERED_NATURES = NATURES;
        public static string[] NATURES_LOWERCASE = NATURES.Select(str => str.ToLowerInvariant()).ToArray();
        public static string[] LOCATIONS = Properties.Resources.locations.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        public static string[] FILTERED_LOCATIONS = LOCATIONS.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        public static string[] LOCATIONS_LOWERCASE = LOCATIONS.Select(str => str.ToLowerInvariant()).ToArray();
        public static string[] ITEMS = Properties.Resources.items.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        public static string[] FILTERED_ITEMS = ITEMS.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        public static string[] ITEMS_LOWERCASE = ITEMS.Select(str => str.ToLowerInvariant()).ToArray();

        static int NormalizeElemID(long id, string[] elems)
        {
            if (id >= elems.Length) return -1;
            if (string.IsNullOrEmpty(elems[id])) return -1;
            return (int)id;
        }
        public static int NormalizeItem(long id)
        {
            return NormalizeElemID(id, ITEMS);
        }
        public static int NormalizeNature(long id)
        {
            return NormalizeElemID(id, NATURES);
        }
        public static int NormalizeLocation(long id)
        {
            return NormalizeElemID(id, LOCATIONS);
        }
        public static int NormalizeMove(long id)
        {
            if (id > MAX_MOVE_ID) return -1;
            return NormalizeElemID(id, MOVES);
        }
    }

    /// <summary>
    /// Logic for converting a National Pokédex Species ID to/from generation specific values.
    /// </summary>
    /// <remarks>Generation 4+ always use the national dex ID. Prior generations do not.</remarks>
    public static class SpeciesConverter
    {
        /// <summary>
        /// Converts a National Dex ID to Generation 3 species ID.
        /// </summary>
        /// <param name="species">National Dex ID</param>
        /// <returns>Generation 3 species ID.</returns>
        public static int GetG3Species(long species) => (ulong)species >= (ulong)table3_Internal.Length ? 0 : table3_Internal[species];

        /// <summary>
        /// Converts Generation 3 species ID to National Dex ID.
        /// </summary>
        /// <param name="raw">Generation 3 species ID.</param>
        /// <returns>National Dex ID.</returns>
        public static int SetG3Species(long raw) => (ulong)raw >= (ulong)table3_National.Length ? 0 : table3_National[raw];

        /// <summary>
        /// National Dex IDs (index) and the associated Gen3 Species IDs (value)
        /// </summary>
        private static readonly ushort[] table3_Internal =
        {
            000, 001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019,
            020, 021, 022, 023, 024, 025, 026, 027, 028, 029, 030, 031, 032, 033, 034, 035, 036, 037, 038, 039,
            040, 041, 042, 043, 044, 045, 046, 047, 048, 049, 050, 051, 052, 053, 054, 055, 056, 057, 058, 059,
            060, 061, 062, 063, 064, 065, 066, 067, 068, 069, 070, 071, 072, 073, 074, 075, 076, 077, 078, 079,
            080, 081, 082, 083, 084, 085, 086, 087, 088, 089, 090, 091, 092, 093, 094, 095, 096, 097, 098, 099,
            100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
            120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139,
            140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
            160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
            180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
            200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
            220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
            240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 277, 278, 279, 280, 281, 282, 283, 284,
            285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 304, 305, 309, 310,
            392, 393, 394, 311, 312, 306, 307, 364, 365, 366, 301, 302, 303, 370, 371, 372, 335, 336, 350, 320,
            315, 316, 322, 355, 382, 383, 384, 356, 357, 337, 338, 353, 354, 386, 387, 363, 367, 368, 330, 331,
            313, 314, 339, 340, 321, 351, 352, 308, 332, 333, 334, 344, 345, 358, 359, 380, 379, 348, 349, 323,
            324, 326, 327, 318, 319, 388, 389, 390, 391, 328, 329, 385, 317, 377, 378, 361, 362, 369, 411, 376,
            360, 346, 347, 341, 342, 343, 373, 374, 375, 381, 325, 395, 396, 397, 398, 399, 400, 401, 402, 403,
            407, 408, 404, 405, 406, 409, 410
        };

        /// <summary>
        /// Gen3 Species IDs (index) and the associated National Dex IDs (value)
        /// </summary>
        private static readonly ushort[] table3_National =
        {
            000, 001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019,
            020, 021, 022, 023, 024, 025, 026, 027, 028, 029, 030, 031, 032, 033, 034, 035, 036, 037, 038, 039,
            040, 041, 042, 043, 044, 045, 046, 047, 048, 049, 050, 051, 052, 053, 054, 055, 056, 057, 058, 059,
            060, 061, 062, 063, 064, 065, 066, 067, 068, 069, 070, 071, 072, 073, 074, 075, 076, 077, 078, 079,
            080, 081, 082, 083, 084, 085, 086, 087, 088, 089, 090, 091, 092, 093, 094, 095, 096, 097, 098, 099,
            100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
            120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139,
            140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
            160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
            180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
            200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
            220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
            240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 000, 000, 000, 000, 000, 000, 000, 000,
            000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 000, 252, 253, 254,
            255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274,
            275, 290, 291, 292, 276, 277, 285, 286, 327, 278, 279, 283, 284, 320, 321, 300, 301, 352, 343, 344,
            299, 324, 302, 339, 340, 370, 341, 342, 349, 350, 318, 319, 328, 329, 330, 296, 297, 309, 310, 322,
            323, 363, 364, 365, 331, 332, 361, 362, 337, 338, 298, 325, 326, 311, 312, 303, 307, 308, 333, 334,
            360, 355, 356, 315, 287, 288, 289, 316, 317, 357, 293, 294, 295, 366, 367, 368, 359, 353, 354, 336,
            335, 369, 304, 305, 306, 351, 313, 314, 345, 346, 347, 348, 280, 281, 282, 371, 372, 373, 374, 375,
            376, 377, 378, 379, 382, 383, 384, 380, 381, 385, 386, 358,
        };

        public static string[] SPECIES = Properties.Resources.species.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        public static string[] FILTERED_SPECIES = SPECIES.Where((_, i) => i != 0 && Array.Exists(table3_National, j => i == j)).ToArray();
        public static string[] SPECIES_LOWERCASE = SPECIES.Select(str => str.ToLowerInvariant()).ToArray();
    }
    public static class ItemConverter
    {
        /// <summary>Unused item ID, placeholder for item/sprite finding in Generations 2-4.</summary>
        private const ushort NaN = 128;

        /// <summary>
        /// Checks if the item can be kept during 3->4 conversion.
        /// </summary>
        /// <param name="item">Generation 3 Item ID.</param>
        /// <returns>True if transferable, False if not transferable.</returns>
        public static bool IsItemTransferable34(ushort item) => item != NaN && item > 0;

        /// <summary>
        /// Converts a Generation 3 Item ID to Generation 4+ Item ID.
        /// </summary>
        /// <param name="item">Generation 3 Item ID.</param>
        /// <returns>Generation 4+ Item ID.</returns>
        public static ushort GetItemFuture3(ushort item) => item >= arr3.Length ? NaN : arr3[item];

        /// <summary>
        /// Converts a Generation 4+ Item ID to Generation 3 Item ID.
        /// </summary>
        /// <param name="item">Generation 4+ Item ID.</param>
        /// <returns>Generation 3 Item ID.</returns>
        public static ushort GetItemOld3(ushort item)
        {
            if (item == NaN)
                return 0;
            int index = Array.IndexOf(arr3, item);
            return (ushort)Math.Max(0, index);
        }

        #region Item Mapping Tables
        /// <summary> Gen3 items (index) and their corresponding Gen4 item ID (value) </summary>
        private static readonly ushort[] arr3 =
        {
            000, 001, 002, 003, 004, 005, 006, 007, 008, 009,
            010, 011, 012, 017, 018, 019, 020, 021, 022, 023,
            024, 025, 026, 027, 028, 029, 030, 031, 032, 033,
            034, 035, 036, 037, 038, 039, 040, 041, 042, 065,
            066, 067, 068, 069, 043, 044, 070, 071, 072, 073,
            074, 075, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, 045, 046, 047, 048, 049, 050, 051,
            052, 053, NaN, 055, 056, 057, 058, 059, 060, 061,
            063, 064, NaN, 076, 077, 078, 079, NaN, NaN, NaN,
            NaN, NaN, NaN, 080, 081, 082, 083, 084, 085, NaN,
            NaN, NaN, NaN, 086, 087, NaN, 088, 089, 090, 091,
            092, 093, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, 149, 150, 151, 152, 153, 154, 155,
            156, 157, 158, 159, 160, 161, 162, 163, 164, 165,
            166, 167, 168, 169, 170, 171, 172, 173, 174, 175,
            176, 177, 178, 179, 180, 181, 182, 183, 201, 202,
            203, 204, 205, 206, 207, 208, NaN, NaN, NaN, 213,
            214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
            224, 225, 226, 227, 228, 229, 230, 231, 232, 233,
            234, 235, 236, 237, 238, 239, 240, 241, 242, 243,
            244, 245, 246, 247, 248, 249, 250, 251, 252, 253,
            254, 255, 256, 257, 258, 259, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, NaN, 260, 261, 262, 263, 264,

            NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN, NaN,
            328, 329, 330, 331, 332, 333, 334, 335, 336, 337,
            338, 339, 340, 341, 342, 343, 344, 345, 346, 347,
            348, 349, 350, 351, 352, 353, 354, 355, 356, 357,
            358, 359, 360, 361, 362, 363, 364, 365, 366, 367,
            368, 369, 370, 371, 372, 373, 374, 375, 376, 377,
        };
        #endregion

        /// <summary>
        /// Checks if an item ID is an HM
        /// </summary>
        /// <param name="item">Item ID</param>
        /// <returns>True if is an HM</returns>
        public static bool IsItemHM(ushort item) => item is (>= 339 and <= 346);

        /// <summary>
        /// Checks if an item ID is a TM
        /// </summary>
        /// <param name="item">Item ID</param>
        /// <returns>True if is an HM</returns>
        public static bool IsItemTM(ushort item) => item is (>= 289 and <= 338);
    }

    /// <summary>
    /// Calculations for <see cref="PKM.EXP"/> and <see cref="PKM.CurrentLevel"/>.
    /// </summary>
    public static class Experience
    {
        /// <summary>
        /// Gets the current level of a species.
        /// </summary>
        /// <param name="exp">Experience points</param>
        /// <param name="growth">Experience growth rate</param>
        /// <returns>Current level of the species.</returns>
        public static int GetLevel(uint exp, byte growth)
        {
            var table = GetTable(growth);
            if (exp >= table[^1])
                return 100;
            int tl = 1; // Initial Level. Iterate upwards to find the level
            while (exp >= table[tl])
                ++tl;
            return tl;
        }

        /// <summary>
        /// Gets the minimum Experience points for the specified level.
        /// </summary>
        /// <param name="level">Current level</param>
        /// <param name="growth">Growth Rate type</param>
        /// <returns>Experience points needed to have specified level.</returns>
        public static uint GetEXP(int level, byte growth)
        {
            if (level <= 1)
                return 0;
            if (level > 100)
                level = 100;

            var table = GetTable(growth);
            return table[level - 1];
        }

        private static ReadOnlySpan<uint> GetTable(byte growth) => growth switch
        {
            0 => Growth0,
            1 => Growth1,
            2 => Growth2,
            3 => Growth3,
            4 => Growth4,
            5 => Growth5,
            _ => throw new ArgumentOutOfRangeException(nameof(growth)),
        };

        /// <summary>
        /// Gets the amount of EXP to be earned until the next level-up occurs.
        /// </summary>
        /// <param name="level">Current Level</param>
        /// <param name="growth">Growth Rate type</param>
        /// <returns>EXP to level up</returns>
        public static uint GetEXPToLevelUp(int level, byte growth)
        {
            if ((uint)level >= 100)
                return 0;
            var table = GetTable(growth);
            var current = table[level - 1];
            var next = table[level];
            return next - current;
        }

        /// <summary>
        /// Gets a percentage for Experience Bar progress indication.
        /// </summary>
        /// <param name="level">Current Level</param>
        /// <param name="exp">Current Experience</param>
        /// <param name="growth">Growth Rate type</param>
        /// <returns>Percentage [0,1.00)</returns>
        public static double GetEXPToLevelUpPercentage(int level, uint exp, byte growth)
        {
            if ((uint)level >= 100)
                return 0;

            var table = GetTable(growth);
            var current = table[level - 1];
            var next = table[level];
            var amount = next - current;
            double progress = exp - current;
            return progress / amount;
        }

        #region ExpTable

        private static ReadOnlySpan<uint> Growth0 => new uint[]
        {
        0000000, 0000008, 0000027, 0000064, 0000125, 0000216, 0000343, 0000512, 0000729, 0001000,
        0001331, 0001728, 0002197, 0002744, 0003375, 0004096, 0004913, 0005832, 0006859, 0008000,
        0009261, 0010648, 0012167, 0013824, 0015625, 0017576, 0019683, 0021952, 0024389, 0027000,
        0029791, 0032768, 0035937, 0039304, 0042875, 0046656, 0050653, 0054872, 0059319, 0064000,
        0068921, 0074088, 0079507, 0085184, 0091125, 0097336, 0103823, 0110592, 0117649, 0125000,
        0132651, 0140608, 0148877, 0157464, 0166375, 0175616, 0185193, 0195112, 0205379, 0216000,
        0226981, 0238328, 0250047, 0262144, 0274625, 0287496, 0300763, 0314432, 0328509, 0343000,
        0357911, 0373248, 0389017, 0405224, 0421875, 0438976, 0456533, 0474552, 0493039, 0512000,
        0531441, 0551368, 0571787, 0592704, 0614125, 0636056, 0658503, 0681472, 0704969, 0729000,
        0753571, 0778688, 0804357, 0830584, 0857375, 0884736, 0912673, 0941192, 0970299, 1000000,
        };

        private static ReadOnlySpan<uint> Growth1 => new uint[]
        {
        0000000, 0000015, 0000052, 0000122, 0000237, 0000406, 0000637, 0000942, 0001326, 0001800,
        0002369, 0003041, 0003822, 0004719, 0005737, 0006881, 0008155, 0009564, 0011111, 0012800,
        0014632, 0016610, 0018737, 0021012, 0023437, 0026012, 0028737, 0031610, 0034632, 0037800,
        0041111, 0044564, 0048155, 0051881, 0055737, 0059719, 0063822, 0068041, 0072369, 0076800,
        0081326, 0085942, 0090637, 0095406, 0100237, 0105122, 0110052, 0115015, 0120001, 0125000,
        0131324, 0137795, 0144410, 0151165, 0158056, 0165079, 0172229, 0179503, 0186894, 0194400,
        0202013, 0209728, 0217540, 0225443, 0233431, 0241496, 0249633, 0257834, 0267406, 0276458,
        0286328, 0296358, 0305767, 0316074, 0326531, 0336255, 0346965, 0357812, 0367807, 0378880,
        0390077, 0400293, 0411686, 0423190, 0433572, 0445239, 0457001, 0467489, 0479378, 0491346,
        0501878, 0513934, 0526049, 0536557, 0548720, 0560922, 0571333, 0583539, 0591882, 0600000,
        };

        private static ReadOnlySpan<uint> Growth2 => new uint[]
        {
        0000000, 0000004, 0000013, 0000032, 0000065, 0000112, 0000178, 0000276, 0000393, 0000540,
        0000745, 0000967, 0001230, 0001591, 0001957, 0002457, 0003046, 0003732, 0004526, 0005440,
        0006482, 0007666, 0009003, 0010506, 0012187, 0014060, 0016140, 0018439, 0020974, 0023760,
        0026811, 0030146, 0033780, 0037731, 0042017, 0046656, 0050653, 0055969, 0060505, 0066560,
        0071677, 0078533, 0084277, 0091998, 0098415, 0107069, 0114205, 0123863, 0131766, 0142500,
        0151222, 0163105, 0172697, 0185807, 0196322, 0210739, 0222231, 0238036, 0250562, 0267840,
        0281456, 0300293, 0315059, 0335544, 0351520, 0373744, 0390991, 0415050, 0433631, 0459620,
        0479600, 0507617, 0529063, 0559209, 0582187, 0614566, 0639146, 0673863, 0700115, 0737280,
        0765275, 0804997, 0834809, 0877201, 0908905, 0954084, 0987754, 1035837, 1071552, 1122660,
        1160499, 1214753, 1254796, 1312322, 1354652, 1415577, 1460276, 1524731, 1571884, 1640000,
        };

        private static ReadOnlySpan<uint> Growth3 => new uint[]
        {
        0000000, 0000009, 0000057, 0000096, 0000135, 0000179, 0000236, 0000314, 0000419, 0000560,
        0000742, 0000973, 0001261, 0001612, 0002035, 0002535, 0003120, 0003798, 0004575, 0005460,
        0006458, 0007577, 0008825, 0010208, 0011735, 0013411, 0015244, 0017242, 0019411, 0021760,
        0024294, 0027021, 0029949, 0033084, 0036435, 0040007, 0043808, 0047846, 0052127, 0056660,
        0061450, 0066505, 0071833, 0077440, 0083335, 0089523, 0096012, 0102810, 0109923, 0117360,
        0125126, 0133229, 0141677, 0150476, 0159635, 0169159, 0179056, 0189334, 0199999, 0211060,
        0222522, 0234393, 0246681, 0259392, 0272535, 0286115, 0300140, 0314618, 0329555, 0344960,
        0360838, 0377197, 0394045, 0411388, 0429235, 0447591, 0466464, 0485862, 0505791, 0526260,
        0547274, 0568841, 0590969, 0613664, 0636935, 0660787, 0685228, 0710266, 0735907, 0762160,
        0789030, 0816525, 0844653, 0873420, 0902835, 0932903, 0963632, 0995030, 1027103, 1059860,
        };

        private static ReadOnlySpan<uint> Growth4 => new uint[]
        {
        0000000, 0000006, 0000021, 0000051, 0000100, 0000172, 0000274, 0000409, 0000583, 0000800,
        0001064, 0001382, 0001757, 0002195, 0002700, 0003276, 0003930, 0004665, 0005487, 0006400,
        0007408, 0008518, 0009733, 0011059, 0012500, 0014060, 0015746, 0017561, 0019511, 0021600,
        0023832, 0026214, 0028749, 0031443, 0034300, 0037324, 0040522, 0043897, 0047455, 0051200,
        0055136, 0059270, 0063605, 0068147, 0072900, 0077868, 0083058, 0088473, 0094119, 0100000,
        0106120, 0112486, 0119101, 0125971, 0133100, 0140492, 0148154, 0156089, 0164303, 0172800,
        0181584, 0190662, 0200037, 0209715, 0219700, 0229996, 0240610, 0251545, 0262807, 0274400,
        0286328, 0298598, 0311213, 0324179, 0337500, 0351180, 0365226, 0379641, 0394431, 0409600,
        0425152, 0441094, 0457429, 0474163, 0491300, 0508844, 0526802, 0545177, 0563975, 0583200,
        0602856, 0622950, 0643485, 0664467, 0685900, 0707788, 0730138, 0752953, 0776239, 0800000,
        };

        private static ReadOnlySpan<uint> Growth5 => new uint[]
        {
        0000000, 0000010, 0000033, 0000080, 0000156, 0000270, 0000428, 0000640, 0000911, 0001250,
        0001663, 0002160, 0002746, 0003430, 0004218, 0005120, 0006141, 0007290, 0008573, 0010000,
        0011576, 0013310, 0015208, 0017280, 0019531, 0021970, 0024603, 0027440, 0030486, 0033750,
        0037238, 0040960, 0044921, 0049130, 0053593, 0058320, 0063316, 0068590, 0074148, 0080000,
        0086151, 0092610, 0099383, 0106480, 0113906, 0121670, 0129778, 0138240, 0147061, 0156250,
        0165813, 0175760, 0186096, 0196830, 0207968, 0219520, 0231491, 0243890, 0256723, 0270000,
        0283726, 0297910, 0312558, 0327680, 0343281, 0359370, 0375953, 0393040, 0410636, 0428750,
        0447388, 0466560, 0486271, 0506530, 0527343, 0548720, 0570666, 0593190, 0616298, 0640000,
        0664301, 0689210, 0714733, 0740880, 0767656, 0795070, 0823128, 0851840, 0881211, 0911250,
        0941963, 0973360, 1005446, 1038230, 1071718, 1105920, 1140841, 1176490, 1212873, 1250000,
        };

        #endregion
    }

}
