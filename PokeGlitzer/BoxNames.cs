using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public class BoxNames : IDisposable
    {
        RangeObservableCollection<byte> sourceData;
        byte[] data;
        string[] boxNames;

        public const int BOX_NAME_LENGTH = 8;
        public const int BOX_NAME_BYTE_SIZE = 9;

        public BoxNames(RangeObservableCollection<byte> rawData)
        {
            sourceData = rawData;
            data = new byte[rawData.Count];
            boxNames = new string[MainWindowViewModel.BOX_NUMBER];
            names = Utils.CollectionOfSize<string>(MainWindowViewModel.BOX_NUMBER);

            UpdateViewFromSource(true);
            sourceData.CollectionChanged += SourceDataChanged;
            Names.CollectionChanged += NamesChanged;
        }

        RangeObservableCollection<string> names;
        public RangeObservableCollection<string> Names { get => names; }

        void ForwardLocalData()
        {
            if (!Enumerable.SequenceEqual(names, boxNames))
                Utils.UpdateCollectionRange(names, boxNames);
            if (!Enumerable.SequenceEqual(sourceData, data))
                Utils.UpdateCollectionRange(sourceData, data);
        }

        void NamesChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (Utils.IsNonTrivialReplacement<string>(args) && !Enumerable.SequenceEqual(Names, boxNames))
                UpdateFromNames(Names.ToArray());
        }
        void SourceDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (Utils.IsNonTrivialReplacement<byte>(args))
                UpdateViewFromSource(false);
        }

        void UpdateViewFromSource(bool force)
        {
            byte[] dataArr = sourceData.ToArray();
            if (force || !Enumerable.SequenceEqual(dataArr, data))
                UpdateFromData(dataArr);
        }

        void UpdateFromData(byte[] dataArr)
        {
            data = dataArr;
            for (int i = 0; i < boxNames.Length; i++)
            {
                int offset = BOX_NAME_BYTE_SIZE * i;
                boxNames[i] = StringConverter.GetString3(data, offset, BOX_NAME_BYTE_SIZE, Settings.Text_useJapanese);
            }
            ForwardLocalData();
        }

        public static bool IsValidName(string name, int maxLen = BOX_NAME_BYTE_SIZE, Language language = Language.Invalid)
        {
            if (name.Length > maxLen) return false;
            foreach (char c in name)
            {
                if (!StringConverter.IsCharValid(c,
                    (language == Language.Invalid && Settings.Text_useJapanese) || language == Language.Japanese))
                    return false;
            }
            return true;
        }
        const char ASCII_START = (char)0x0021;
        const char ASCII_END = (char)0x007E;
        const char FULLWIDTH_START = (char)0xFF01;
        const char FULLWIDTH_END = (char)0xFF5E;
        public static string NormalizeName(string name, Language language = Language.Invalid)
        {
            name = name.Replace("␣", " ");
            name = name.Replace("_", " ");
            name = name.Replace("＿", " ");
            name = name.Replace("'", "’");
            name = name.Replace("–", "-");
            name = name.Replace("—", "-");
            name = name.Replace("ー", "-");
            if ((language == Language.Invalid && Settings.Text_useJapanese) ||
                language == Language.Japanese)
            {
                name = name.Replace("⋯", "‥");
                name = name.Replace("…", "‥");
                name = name.Replace(" ", "　");
                char fw = FULLWIDTH_START;
                for (char c = ASCII_START; c <= ASCII_END; c++)
                {
                    name = name.Replace(c, fw);
                    fw++;
                }
            }
            else
            {
                name = name.Replace("⋯", "…");
                name = name.Replace("‥", "…");
                name = name.Replace("　", " ");
                char a = ASCII_START;
                for (char c = FULLWIDTH_START; c <= FULLWIDTH_END; c++)
                {
                    name = name.Replace(c, a);
                    a++;
                }
            }
            if ((language == Language.Invalid && Settings.Text_lang == Settings.Lang.FRA) ||
                language == Language.French)
            {
                name = name.Replace("«", "“");
                name = name.Replace("»", "”");
            }
            if ((language == Language.Invalid && Settings.Text_lang == Settings.Lang.GER) ||
                language == Language.German)
            {
                name = name.Replace("“", "”");
                name = name.Replace("„", "“");
            }
            return name;
        }
        public static string MakeNameLookBetter(string name, Language language = Language.Invalid)
        {
            name = name.Replace(" ", "␣");
            name = name.Replace("　", "␣");
            if ((language == Language.Invalid && Settings.Text_lang == Settings.Lang.FRA) ||
                language == Language.French)
            {
                name = name.Replace("“", "«");
                name = name.Replace("”", "»");
            }
            if ((language == Language.Invalid && Settings.Text_lang == Settings.Lang.GER) ||
                language == Language.German)
            {
                name = name.Replace("“", "„");
                name = name.Replace("”", "“");
            }
            return name;
        }

        void UpdateFromNames(string[] names)
        {
            byte[] res = new byte[data.Length];
            for (int i = 0; i < boxNames.Length; i++)
            {
                int offset = BOX_NAME_BYTE_SIZE * i;
                byte[] d = StringConverter.SetString3(names[i], BOX_NAME_BYTE_SIZE, Settings.Text_useJapanese, BOX_NAME_BYTE_SIZE, 0xFF);
                Array.Copy(d, 0, res, offset, d.Length);
            }
            UpdateFromData(res);
        }

        public void Dispose()
        {
            sourceData.CollectionChanged -= SourceDataChanged;
        }
    }
}
