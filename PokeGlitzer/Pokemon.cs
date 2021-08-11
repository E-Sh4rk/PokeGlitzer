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
    // TODO: Modify PokemonView so that modifications made here to the view are not notified here
    // TODO: Test whether it fixes the following issue: loading abra.sav and adding 0x40000000/0x41230000 to the TID alter the decoded data and break the checksum
    public record DataLocation(int offset, int size, bool inTeam)
    {
        public static bool Intersect(DataLocation dl1, DataLocation dl2)
        {
            if (dl1.inTeam != dl2.inTeam) return false;
            int l1 = dl1.offset; int r1 = l1 + dl1.size;
            int l2 = dl2.offset; int r2 = l2 + dl2.size;
            return Math.Max(l1, l2) < Math.Min(r1, r2);
        }
        public bool Intersect(DataLocation dl2)
        {
            return Intersect(this, dl2);
        }
        public static DataLocation DefaultPC = new DataLocation(0, Pokemon.PC_SIZE, false);
        public static DataLocation DefaultTeam = new DataLocation(0, Pokemon.TEAM_SIZE, true);
    }
    public class Pokemon : IDisposable
    {
        public const int TEAM_SIZE = 100;
        public const int PC_SIZE = 80;
        RangeObservableCollection<byte> sourceData;
        int index;
        int size;
        bool inTeam;
        public Pokemon(RangeObservableCollection<byte> rawData, DataLocation dl)
        {
            if (dl.size != PC_SIZE && dl.size != TEAM_SIZE) throw new ArgumentException();
            size = dl.size;
            inTeam = dl.inTeam;
            sourceData = rawData;
            index = dl.offset;
            view = new PokemonView(size);
            UpdateViewFromSource();
            sourceData.CollectionChanged += SourceDataChanged;
            view.Data.CollectionChanged += ViewDataChanged;
            view.DecodedData.CollectionChanged += ViewDecodedDataChanged;
            view.PropertyChanged += ViewInterpretedChanged;
        }
        /*byte[] data;
        byte[] decodedData;
        InterpretedData interpreted;
        TeamInterpretedData? teamInterpretedData;*/

        PokemonView view;
        public PokemonView View { get => view; }
        public DataLocation DataLocation { get => new DataLocation(index, size, inTeam); }

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
            if (Utils.IsNonTrivialReplacement(args))
            {
                UpdateView(view.Data.ToArray(), false, false, true, true, true);
                Utils.UpdateCollectionRange(sourceData, new ArraySegment<byte>(view.Data.ToArray(), args.OldStartingIndex, args.NewItems!.Count), index + args.OldStartingIndex);
            }
        }

        void ViewDecodedDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (Utils.IsNonTrivialReplacement(args))
            {
                UpdateView(view.DecodedData.ToArray(), true, true, false, true, true);
                Utils.UpdateCollectionRange(sourceData, new ArraySegment<byte>(view.Data.ToArray(), args.OldStartingIndex, args.NewItems!.Count), index + args.OldStartingIndex);
            }
        }

        void ViewInterpretedChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(PokemonView.Interpreted) &&
                args.PropertyName != nameof(PokemonView.TeamInterpreted)) return;

            if (args.PropertyName == nameof(PokemonView.Interpreted))
                UpdateViewFromInterpreted();
            else
                UpdateViewFromTeamInterpreted();
            Utils.UpdateCollectionRange(sourceData, view.Data, index);
        }

        void SourceDataChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (Utils.IsNonTrivialReplacement(args))
            {
                if (DataLocation.Intersect(new DataLocation(args.OldStartingIndex, args.NewItems!.Count, inTeam)))
                    UpdateViewFromSource();
            }
        }

        void UpdateViewFromSource()
        {
            byte[] dataArr = Utils.ExtractCollectionRange(sourceData, index, size);
            UpdateView(dataArr, false, true, true, true, true);
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
            else
            {
                bool hasData = false;
                foreach (byte b in dataArr)
                {
                    if (b != 0) { hasData = true; break; }
                }
                view.HasData = hasData;
            }
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
        /*PokemonStruct GetPkmnStruct(byte[] dataArr)
        {
            if (dataArr.Length == TEAM_SIZE)
                return Utils.ByteToType<PokemonTeamStruct>(dataArr).permanent;
            else
                return Utils.ByteToType<PokemonStruct>(dataArr);
        }*/
        PokemonTeamStruct GetPkmnTeamStruct(byte[] dataArr)
        {
            if (dataArr.Length == TEAM_SIZE)
                return Utils.ByteToType<PokemonTeamStruct>(dataArr);
            else
            {
                PokemonTeamStruct pt = new PokemonTeamStruct();
                pt.permanent = Utils.ByteToType<PokemonStruct>(dataArr);
                return pt;
            }
        }
        void UpdateView(byte[] dataArr, bool isDecoded, bool updateData, bool updateDecodedData, bool updateInterpreted, bool updateTeamInterpreted)
        {
            PokemonTeamStruct pkmn = GetPkmnTeamStruct(dataArr);
            // Checksum and data
            UpdateChecksumAndViewData(pkmn.permanent, dataArr, isDecoded, updateData, updateDecodedData);
            // Compute substructures position order
            int[] order = subOrders[pkmn.permanent.PID % 24];
            view.SubstructureAtPos0 = order[0];
            view.SubstructureAtPos1 = order[1];
            view.SubstructureAtPos2 = order[2];
            view.SubstructureAtPos3 = order[3];
            // XOR data
            uint key = pkmn.permanent.PID ^ pkmn.permanent.OTID;
            for (int i = 0; i < pkmn.permanent.data.Length; i += 4)
            {
                uint data = BitConverter.ToUInt32(pkmn.permanent.data, i);
                byte[] res = BitConverter.GetBytes(data^key);
                pkmn.permanent.data[i] = res[0];
                pkmn.permanent.data[i+1] = res[1];
                pkmn.permanent.data[i+2] = res[2];
                pkmn.permanent.data[i+3] = res[3];
            }
            if (dataArr.Length == TEAM_SIZE)
                dataArr = Utils.TypeToByte(pkmn);
            else
                dataArr = Utils.TypeToByte(pkmn.permanent);
            isDecoded = !isDecoded;
            // Checksum and data
            UpdateChecksumAndViewData(pkmn.permanent, dataArr, isDecoded, updateData, updateDecodedData);
            // Interpreted data
            if (updateInterpreted)
            {
                // Retrieving substructures
                PokemonStruct decoded = Utils.ByteToType<PokemonStruct>(view.DecodedData.ToArray());
                Substructure0 sub0 = Utils.ByteToType<Substructure0>(GetSubstructure(decoded, 0));
                Substructure3 sub3 = Utils.ByteToType<Substructure3>(GetSubstructure(decoded, 3));
                // Extracting interpreted data
                EggType eggType = EggType.Invalid;
                bool hasSpecies = (decoded.isEgg & PokemonStruct.HAS_SPECIES_MASK) != 0;
                bool eggData = (sub3.ivEggAbility & Substructure3.EGG_MASK) != 0;
                bool egg = (decoded.isEgg & PokemonStruct.IS_EGG_MASK) != 0;
                bool badEgg = (decoded.isEgg & PokemonStruct.IS_BAD_EGG_MASK) != 0;
                if (eggData == egg)
                {
                    if (hasSpecies)
                    {
                        if (!egg && !badEgg) eggType = EggType.NotAnEgg;
                        else if (egg & badEgg) eggType = EggType.BadEgg;
                        else if (egg) eggType = EggType.Egg;
                    }
                    else if (!egg && !badEgg) eggType = EggType.None;
                }
                view.Interpreted = new InterpretedData(pkmn.permanent.PID, pkmn.permanent.OTID, sub0.species, eggType);
            }
            // Team Interpreted data
            if (updateTeamInterpreted && size == TEAM_SIZE)
            {
                PkmnStatus status = new PkmnStatus((byte)(pkmn.status & PokemonTeamStruct.SLEEP_MASK), (pkmn.status & PokemonTeamStruct.POISON_MASK) != 0,
                    (pkmn.status & PokemonTeamStruct.BURN_MASK) != 0, (pkmn.status & PokemonTeamStruct.FREEZE_MASK) != 0,
                    (pkmn.status & PokemonTeamStruct.PARALYSIS_MASK) != 0, (pkmn.status & PokemonTeamStruct.BAD_POISON_MASK) != 0);
                view.TeamInterpreted = new TeamInterpretedData(status, pkmn.level, pkmn.pokerusRemaining, pkmn.currentHP, pkmn.maxHP, pkmn.attack,
                    pkmn.defense, pkmn.speed, pkmn.speAttack, pkmn.speDefense);
            }
        }
        void UpdateViewFromInterpreted()
        {
            InterpretedData interpreted = view.Interpreted;
            PokemonTeamStruct pkmn = GetPkmnTeamStruct(view.DecodedData.ToArray());
            // Retrieving substructures
            Substructure0 sub0 = Utils.ByteToType<Substructure0>(GetSubstructure(pkmn.permanent, 0));
            Substructure1 sub1 = Utils.ByteToType<Substructure1>(GetSubstructure(pkmn.permanent, 1));
            Substructure2 sub2 = Utils.ByteToType<Substructure2>(GetSubstructure(pkmn.permanent, 2));
            Substructure3 sub3 = Utils.ByteToType<Substructure3>(GetSubstructure(pkmn.permanent, 3));
            // Modifying data
            pkmn.permanent.PID = interpreted.PID;
            pkmn.permanent.OTID = interpreted.OTID;
            sub0.species = interpreted.species;
            switch (interpreted.egg)
            {
                case EggType.Egg:
                    sub3.ivEggAbility = sub3.ivEggAbility | Substructure3.EGG_MASK;
                    pkmn.permanent.isEgg = (byte)((pkmn.permanent.isEgg | PokemonStruct.HAS_SPECIES_MASK | PokemonStruct.IS_EGG_MASK) & ~PokemonStruct.IS_BAD_EGG_MASK);
                    break;
                case EggType.BadEgg:
                    sub3.ivEggAbility = sub3.ivEggAbility | Substructure3.EGG_MASK;
                    pkmn.permanent.isEgg = (byte)(pkmn.permanent.isEgg | PokemonStruct.HAS_SPECIES_MASK | PokemonStruct.IS_EGG_MASK | PokemonStruct.IS_BAD_EGG_MASK);
                    break;
                case EggType.NotAnEgg:
                    sub3.ivEggAbility = sub3.ivEggAbility & ~Substructure3.EGG_MASK;
                    pkmn.permanent.isEgg = (byte)((pkmn.permanent.isEgg | PokemonStruct.HAS_SPECIES_MASK) & ~PokemonStruct.IS_EGG_MASK & ~PokemonStruct.IS_BAD_EGG_MASK);
                    break;
                case EggType.None:
                    sub3.ivEggAbility = sub3.ivEggAbility & ~Substructure3.EGG_MASK;
                    pkmn.permanent.isEgg = (byte)(pkmn.permanent.isEgg & ~PokemonStruct.HAS_SPECIES_MASK & ~PokemonStruct.IS_EGG_MASK & ~PokemonStruct.IS_BAD_EGG_MASK);
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
            int[] order = SubstructuresOrder(pkmn.permanent);
            Array.Copy(b0, 0, subs, OffsetOfSubstructure(order, 0), PokemonStruct.SUBSTRUCTURE_SIZE);
            Array.Copy(b1, 0, subs, OffsetOfSubstructure(order, 1), PokemonStruct.SUBSTRUCTURE_SIZE);
            Array.Copy(b2, 0, subs, OffsetOfSubstructure(order, 2), PokemonStruct.SUBSTRUCTURE_SIZE);
            Array.Copy(b3, 0, subs, OffsetOfSubstructure(order, 3), PokemonStruct.SUBSTRUCTURE_SIZE);
            pkmn.permanent.data = subs;
            // Fixing checksum if it was valid initially
            if (view.ChecksumValid) pkmn.permanent.checksum = ComputeChecksum(pkmn.permanent);
            byte[] res;
            if (size == TEAM_SIZE)
                res = Utils.TypeToByte(pkmn);
            else
                res = Utils.TypeToByte(pkmn.permanent);
            UpdateView(res, true, true, true, false, false);
        }
        void UpdateViewFromTeamInterpreted()
        {
            if (size != TEAM_SIZE) return;
            PokemonTeamStruct pkmn = GetPkmnTeamStruct(view.DecodedData.ToArray());
            TeamInterpretedData teamInterpreted = view.TeamInterpreted!;
            pkmn.currentHP = teamInterpreted.currentHP;
            pkmn.maxHP = teamInterpreted.maxHP;
            pkmn.attack = teamInterpreted.attack;
            pkmn.defense = teamInterpreted.defense;
            pkmn.speed = teamInterpreted.speed;
            pkmn.speAttack = teamInterpreted.speAttack;
            pkmn.speDefense = teamInterpreted.speDefense;
            pkmn.level = teamInterpreted.level;
            pkmn.pokerusRemaining = teamInterpreted.pokerusRemaining;
            pkmn.status = (byte)(teamInterpreted.status.sleep & PokemonTeamStruct.SLEEP_MASK);
            if (teamInterpreted.status.poison)
                pkmn.status |= PokemonTeamStruct.POISON_MASK;
            if (teamInterpreted.status.burn)
                pkmn.status |= PokemonTeamStruct.BURN_MASK;
            if (teamInterpreted.status.freeze)
                pkmn.status |= PokemonTeamStruct.FREEZE_MASK;
            if (teamInterpreted.status.paralysis)
                pkmn.status |= PokemonTeamStruct.PARALYSIS_MASK;
            if (teamInterpreted.status.badPoison)
                pkmn.status |= PokemonTeamStruct.BAD_POISON_MASK;
            UpdateView(Utils.TypeToByte(pkmn), true, true, true, false, false);
        }

        // ===== PUBLIC FUNCTIONS =====

        public void FixChecksum()
        {
            if (!view.ChecksumValid)
            {
                ushort checksum = ComputeChecksum(Utils.ByteToType<PokemonStruct>(view.DecodedData.ToArray()));
                byte[] res = BitConverter.GetBytes(checksum);
                int offset = Utils.OffsetOf<PokemonStruct>("checksum");
                Utils.UpdateCollectionRange(sourceData, res, index + offset);
                Utils.UpdateCollectionRange(view.Data, res, offset);
                Utils.UpdateCollectionRange(view.DecodedData, res, offset);
                view.ChecksumValid = true;
            }
        }

        public void FlagAsBaddEggIfInvalid()
        {
            if (!View.ChecksumValid)
                View.Interpreted = View.Interpreted with { egg = EggType.BadEgg };
        }

        public void Dispose()
        {
            sourceData.CollectionChanged -= SourceDataChanged;
        }
    }
}
