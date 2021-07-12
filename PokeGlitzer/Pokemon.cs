using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public class Pokemon : IDisposable
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
            view.PropertyChanged += ViewInterpretedChanged;
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
            UpdateView(view.Data.ToArray(), false, false, true, true);
        }

        void ViewDecodedDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateView(view.DecodedData.ToArray(), true, true, false, true);
            Utils.UpdateCollectionRange(data, view.Data, index);
        }

        void ViewInterpretedChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(PokemonView.Intepreted)) return;

            UpdateViewFromInterpreted();
            Utils.UpdateCollectionRange(data, view.Data, index);
        }

        void SourceDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
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
            UpdateView(dataArr, false, true, true, true);
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
        int[] SubstructuresOrder(PokemonStruct pkmn)
        {
            return subOrders[pkmn.PID % 24];
        }
        int OffsetOfSubstructure(int[] order, int sub)
        {
            for (int i = 0; i < order.Length; i++)
            {
                if (order[i] == sub) return i*PokemonStruct.SUBSTRUCTURE_SIZE;
            }
            throw new ArgumentOutOfRangeException();
        }
        byte[] GetSubstructure(PokemonStruct pkmn, int sub)
        {
            int offset = OffsetOfSubstructure(SubstructuresOrder(pkmn), sub);
            byte[] res = new byte[PokemonStruct.SUBSTRUCTURE_SIZE];
            Array.Copy(pkmn.data, offset, res, 0, PokemonStruct.SUBSTRUCTURE_SIZE);
            return res;
        }
        void UpdateView(byte[] dataArr, bool isDecoded, bool updateData, bool updateDecodedData, bool updateInterpreted)
        {
            PokemonStruct pkmn = Utils.ByteToType<PokemonStruct>(dataArr);
            // Checksum and data
            UpdateChecksumAndViewData(pkmn, dataArr, isDecoded, updateData, updateDecodedData);
            // Compute substructures position order
            int[] order = subOrders[pkmn.PID % 24];
            view.SubstructureAtPos0 = order[0];
            view.SubstructureAtPos1 = order[1];
            view.SubstructureAtPos2 = order[2];
            view.SubstructureAtPos3 = order[3];
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
            // Checksum and data
            UpdateChecksumAndViewData(pkmn, dataArr, isDecoded, updateData, updateDecodedData);
            // Interpreted data
            if (updateInterpreted)
            {
                // Retrieving substructures
                PokemonStruct decoded = Utils.ByteToType<PokemonStruct>(view.DecodedData.ToArray());
                Substructure3 sub3 = Utils.ByteToType<Substructure3>(GetSubstructure(decoded, 3));
                // Extracting interpreted data
                EggType eggType = EggType.Invalid;
                bool eggData = (sub3.ivEggAbility & Substructure3.EGG_MASK) != 0;
                bool egg = (decoded.isEgg & PokemonStruct.IS_EGG_MASK) != 0;
                bool badEgg = (decoded.isEgg & PokemonStruct.IS_BAD_EGG_MASK) != 0;
                if (eggData == egg)
                {
                    if (!egg && !badEgg) eggType = EggType.NotAnEgg;
                    else if (egg & badEgg) eggType = EggType.BadEgg;
                    else if (egg) eggType = EggType.Egg;
                }
                // Update interpreted data
                view.Intepreted = new InterpretedData(pkmn.PID, pkmn.OTID, eggType);
            }
        }
        void UpdateViewFromInterpreted()
        {
            InterpretedData interpreted = view.Intepreted!;
            PokemonStruct pkmn = Utils.ByteToType<PokemonStruct>(view.DecodedData.ToArray());
            // Retrieving substructures
            Substructure0 sub0 = Utils.ByteToType<Substructure0>(GetSubstructure(pkmn, 0));
            Substructure1 sub1 = Utils.ByteToType<Substructure1>(GetSubstructure(pkmn, 1));
            Substructure2 sub2 = Utils.ByteToType<Substructure2>(GetSubstructure(pkmn, 2));
            Substructure3 sub3 = Utils.ByteToType<Substructure3>(GetSubstructure(pkmn, 3));
            // Modifying data
            pkmn.PID = interpreted.PID;
            pkmn.OTID = interpreted.OTID;
            switch (interpreted.egg)
            {
                case EggType.Egg:
                    sub3.ivEggAbility = sub3.ivEggAbility | Substructure3.EGG_MASK;
                    pkmn.isEgg = (byte)((pkmn.isEgg | PokemonStruct.IS_EGG_MASK) & ~PokemonStruct.IS_BAD_EGG_MASK);
                    break;
                case EggType.BadEgg:
                    sub3.ivEggAbility = sub3.ivEggAbility | Substructure3.EGG_MASK;
                    pkmn.isEgg = (byte)(pkmn.isEgg | PokemonStruct.IS_EGG_MASK | PokemonStruct.IS_BAD_EGG_MASK);
                    break;
                case EggType.NotAnEgg:
                    sub3.ivEggAbility = sub3.ivEggAbility & ~Substructure3.EGG_MASK;
                    pkmn.isEgg = (byte)(pkmn.isEgg & ~PokemonStruct.IS_EGG_MASK & ~PokemonStruct.IS_BAD_EGG_MASK);
                    break;
                default:
                    break;
            }
            // Joining and updating substructures
            byte[] b0 = Utils.TypeToByte(sub0);
            byte[] b1 = Utils.TypeToByte(sub1);
            byte[] b2 = Utils.TypeToByte(sub2);
            byte[] b3 = Utils.TypeToByte(sub3);
            byte[] subs = new byte[PokemonStruct.SUBSTRUCTURE_SIZE*4];
            int[] order = SubstructuresOrder(pkmn);
            Array.Copy(b0, 0, subs, OffsetOfSubstructure(order, 0), PokemonStruct.SUBSTRUCTURE_SIZE);
            Array.Copy(b1, 0, subs, OffsetOfSubstructure(order, 1), PokemonStruct.SUBSTRUCTURE_SIZE);
            Array.Copy(b2, 0, subs, OffsetOfSubstructure(order, 2), PokemonStruct.SUBSTRUCTURE_SIZE);
            Array.Copy(b3, 0, subs, OffsetOfSubstructure(order, 3), PokemonStruct.SUBSTRUCTURE_SIZE);
            pkmn.data = subs;
            // Fixing checksum if it was valid initially
            if (view.ChecksumValid) pkmn.checksum = ComputeChecksum(pkmn);
            UpdateView(Utils.TypeToByte(pkmn), true, true, true, false);
        }

        // ===== PUBLIC FUNCTIONS =====
        
        public void FixChecksum()
        {
            if (!view.ChecksumValid)
            {
                ushort checksum = ComputeChecksum(Utils.ByteToType<PokemonStruct>(view.DecodedData.ToArray()));
                byte[] res = BitConverter.GetBytes(checksum);
                int offset = Utils.OffsetOf<PokemonStruct>("checksum");
                Utils.UpdateCollectionRange(data, res, index + offset);
                Utils.UpdateCollectionRange(view.Data, res, offset);
                Utils.UpdateCollectionRange(view.DecodedData, res, offset);
                view.ChecksumValid = true;
            }
        }

        public void Dispose()
        {
            data.CollectionChanged -= SourceDataChanged;
        }
    }
}
