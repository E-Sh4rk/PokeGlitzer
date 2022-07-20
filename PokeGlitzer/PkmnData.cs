using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    // Most of these classes are taken from PKHeX:
    // https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/PKM/Strings/StringConverter3.cs

    /// <summary>
    /// Logic for converting a <see cref="string"/> for Generation 3.
    /// </summary>
    public static class StringConverter
    {
        private const byte TerminatorByte = 0xFF;
        private const char Terminator = (char)TerminatorByte;

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
            ' ',  '0',  '1',  '2', '3',  '4',  '5',  '6',  '7',  '8',  '9',  '!', '?',  '.',  '-',  '・', // A
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
            '：',  'Ä',  'Ö',  'Ü',  'ä',  'ö', 'ü', '⬆',  '⬇',  '⬅',                                    // F

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

}
