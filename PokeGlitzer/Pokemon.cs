using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    class Pokemon
    {
        RangeObservableCollection<byte> data;
        int index;
        int size;
        public Pokemon(RangeObservableCollection<byte> rawData, int rawDataIndex)
        {
            size = 80;
            data = rawData;
            index = rawDataIndex;
            view = new PokemonView(size);
            UpdateViewFromSource();
            data.CollectionChanged += SourceDataChanged;
            view.Data.CollectionChanged += ViewDataChanged;
            view.DecodedData.CollectionChanged += ViewDecodedDataChanged;
        }

        PokemonView view;
        public PokemonView View { get => view; }

        int[][] subOrders = new int[][]
        {
            new int[] { 0,1,2,3 }, // GAEM
            new int[] { 0,1,3,2 }, // GAME
            new int[] { 0,2,1,3 }, // GEAM
            new int[] { 0,2,3,1 }, // GEMA
            new int[] { 0,3,1,2 }, // GMAE
            new int[] { 0,3,2,1 }, // GMEA
            new int[] { 1,0,2,3 }, // AGEM
            new int[] { 1,0,3,2 }, // AGME
            new int[] { 1,2,0,3 }, // AEGM
            new int[] { 1,2,3,0 }, // AEMG
            new int[] { 1,3,0,2 }, // AMGE
            new int[] { 1,3,2,0 }, // AMEG
            new int[] { 2,0,1,3 }, // EGAM
            new int[] { 2,0,3,1 }, // EGMA
            new int[] { 2,1,0,3 }, // EAGM
            new int[] { 2,1,3,0 }, // EAMG
            new int[] { 2,3,0,1 }, // EMGA
            new int[] { 2,3,1,0 }, // EMAG
            new int[] { 3,0,1,2 }, // MGAE
            new int[] { 3,0,2,1 }, // MGEA
            new int[] { 3,1,0,2 }, // MAGE
            new int[] { 3,1,2,0 }, // MAEG
            new int[] { 3,2,0,1 }, // MEGA
            new int[] { 3,2,1,0 }  // MEAG
        };

        void ViewDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (sender == this) return;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    Utils.UpdateCollectionRange(data, args.NewItems!.Cast<byte>(), index + args.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Utils.UpdateCollectionRange(data, view.Data, index);
                    break;
                default:
                    throw new NotImplementedException();
            }
            UpdateView(view.Data.ToArray(), false, false, true);
        }

        void ViewDecodedDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (sender == this) return;

            UpdateView(view.DecodedData.ToArray(), true, true, false);
            Utils.UpdateCollectionRange(data, view.Data, index);
        }

        void SourceDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (sender == this) return;

            bool needUpdate;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    int l1 = args.OldStartingIndex;
                    int u1 = args.OldStartingIndex + args.NewItems!.Count;
                    int l2 = index;
                    int u2 = index + size;
                    needUpdate = Math.Max(l1, l2) < Math.Min(u1, u2);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    needUpdate = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (needUpdate) UpdateViewFromSource();
        }

        void UpdateViewFromSource()
        {
            byte[] dataArr = Utils.ExtractCollectionRange(data, index, size);
            UpdateView(dataArr, false, true, true);
        }
        ushort ComputeChecksum(PokemonStruct pkmn) // pkmn data must be decoded
        {
            ushort checksum = 0;
            for (int i = 0; i < pkmn.data.Length; i += 2)
                checksum += BitConverter.ToUInt16(pkmn.data, i);
            return checksum;
        }
        void UpdateChecksumAndViewData(PokemonStruct pkmn, byte[] dataArr, bool isDecoded, bool updateData, bool updateDecodedData)
        {
            if (isDecoded) view.ChecksumValid = ComputeChecksum(pkmn) == pkmn.checksum;
            if (isDecoded && updateDecodedData) Utils.UpdateCollectionRange(view.DecodedData, dataArr);
            if (!isDecoded && updateData) Utils.UpdateCollectionRange(view.Data, dataArr);
        }
        void UpdateView(byte[] dataArr, bool isDecoded, bool updateData, bool updateDecodedData)
        {
            PokemonStruct pkmn = Utils.ByteToType<PokemonStruct>(dataArr);
            // Checksum and views
            UpdateChecksumAndViewData(pkmn, dataArr, isDecoded, updateData, updateDecodedData);
            // Compute substructures position order
            uint m = pkmn.PID % 24;
            int[] order = subOrders[m];
            view.SubstructureAtPos1 = order[0] + 1;
            view.SubstructureAtPos2 = order[1] + 1;
            view.SubstructureAtPos3 = order[2] + 1;
            view.SubstructureAtPos4 = order[3] + 1;
            // XOR data
            uint key = pkmn.PID ^ pkmn.OTID;
            for (int i = 0; i < pkmn.data.Length; i += 4)
            {
                uint data = BitConverter.ToUInt32(pkmn.data, i);
                byte[] res = BitConverter.GetBytes(data^key);
                pkmn.data[i] = res[0];
                pkmn.data[i+1] = res[1];
                pkmn.data[i+2] = res[2];
                pkmn.data[i+3] = res[3];
            }
            dataArr = Utils.TypeToByte(pkmn);
            isDecoded = !isDecoded;
            // Checksum and views
            UpdateChecksumAndViewData(pkmn, dataArr, isDecoded, updateData, updateDecodedData);
        }
    }
}
