﻿using System;
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
                boxNames[i] = StringConverter.GetString3(data, offset, BOX_NAME_BYTE_SIZE, Settings.Text_japaneseCharset);
            }
            ForwardLocalData();
        }

        void UpdateFromNames(string[] names)
        {
            throw new NotImplementedException();
            /*byte[] res = new byte[data.Length];
            // TODO
            UpdateFromData(res);*/
        }

        public void Dispose()
        {
            sourceData.CollectionChanged -= SourceDataChanged;
        }
    }
}
