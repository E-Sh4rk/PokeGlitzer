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

        public static bool IsValidName(string name)
        {
            if (name.Length > BOX_NAME_BYTE_SIZE) return false;
            foreach (char c in name)
            {
                if (!StringConverter.IsCharValid(c, Settings.Text_useJapanese))
                    return false;
            }
            return true;
        }
        public static string NormalizeName(string name)
        {
            name = name.Replace("␣", " ");
            name = name.Replace("_", " ");
            name = name.Replace("'", "’");
            if (Settings.Text_useJapanese)
            {
                name = name.Replace("-", "ー");
                name = name.Replace("–", "ー");
                name = name.Replace("—", "ー");
                name = name.Replace("⋯", "‥");
                name = name.Replace("…", "‥");
            }
            else
            {
                name = name.Replace("–", "-");
                name = name.Replace("—", "-");
                name = name.Replace("ー", "-");
                name = name.Replace("⋯", "…");
                name = name.Replace("‥", "…");
            }
            if (Settings.Text_lang == Settings.Lang.FRA)
            {
                name = name.Replace("«", "“");
                name = name.Replace("»", "”");
            }
            if (Settings.Text_lang == Settings.Lang.GER)
            {
                name = name.Replace("“", "”");
                name = name.Replace("„", "“");
            }
            return name; // TODO
        }
        public static string MakeNameLookBetter(string name)
        {
            name = name.Replace(" ", "␣");
            if (Settings.Text_lang == Settings.Lang.FRA)
            {
                name = name.Replace("“", "«");
                name = name.Replace("”", "»");
            }
            if (Settings.Text_lang == Settings.Lang.GER)
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
