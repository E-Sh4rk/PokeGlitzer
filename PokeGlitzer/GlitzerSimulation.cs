using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    class GlitzerSimulation
    {
        const int BOX_DATA_OFFSET_FROM_STORAGE = 4;
        readonly RangeObservableCollection<byte> initialData;
        readonly int minOffset;
        readonly int baseOffset;
        readonly int maxOffset;
        readonly byte aslrMask;
        readonly byte numberSlots;
        public GlitzerSimulation(RangeObservableCollection<byte> data)
        {
            initialData = new RangeObservableCollection<byte>(data);
            numberSlots = Settings.Corruption_numberUpPresses;
            aslrMask = Settings.Corruption_aslrMask;
            uint gPokemonBox = Settings.Corruption_gPokemonStorage + BOX_DATA_OFFSET_FROM_STORAGE;
            uint gPlayerParty = Settings.Corruption_gPlayerParty;
            baseOffset = (int)((gPlayerParty + (0x100 - numberSlots) * Pokemon.TEAM_SIZE) - gPokemonBox);
            maxOffset = (int)((gPlayerParty + 0x100 * Pokemon.TEAM_SIZE) - gPokemonBox - 1);
            minOffset = baseOffset - aslrMask;
        }

        public enum CorruptionType
        {
            PID, TID, Other, None
        }
        record Corruption(InterpretedData interpreted, CorruptionType type);
        Corruption[] SimulateWithAslr(int aslr)
        {
            int cur_offset = (int)baseOffset - aslr;
            RangeObservableCollection<byte> data = new RangeObservableCollection<byte>(initialData);
            // Simulate
            for (int i = 0; i < numberSlots; i++)
            {
                Pokemon p = new Pokemon(data, cur_offset, Pokemon.TEAM_SIZE, false);
                p.FlagAsBaddEggIfInvalid();
                p.Dispose();
                cur_offset += Pokemon.TEAM_SIZE;
            }
            DataLocation impactedArea = new DataLocation(minOffset, maxOffset - minOffset + 1, false);
            // Interpret and gather results
            cur_offset = 0;
            byte[] newData = data.ToArray();
            byte[] oldData = initialData.ToArray();
            List<Corruption> res = new List<Corruption>();
            while (cur_offset + Pokemon.PC_SIZE <= newData.Length)
            {
                DataLocation dl = new DataLocation(cur_offset, Pokemon.PC_SIZE, false);
                if (impactedArea.Intersect(dl))
                {
                    Pokemon p = new Pokemon(data, dl.offset, dl.size, dl.inTeam);
                    if (p.View.Interpreted != null && p.View.ChecksumValid)
                    {
                        CorruptionType type = CorruptionType.None;
                        if (!Enumerable.SequenceEqual(new ArraySegment<byte>(newData, cur_offset, Pokemon.PC_SIZE),
                            new ArraySegment<byte>(oldData, cur_offset, Pokemon.PC_SIZE)))
                        {
                            Pokemon op = new Pokemon(initialData, cur_offset, Pokemon.PC_SIZE, false);
                            type = CorruptionType.Other;
                            if (op.View.Interpreted != null)
                            {
                                if (op.View.Interpreted.PID != p.View.Interpreted.PID) type = CorruptionType.PID;
                                else if (op.View.Interpreted.OTID != p.View.Interpreted.OTID) type = CorruptionType.TID;
                            }
                            op.Dispose();
                        }
                        res.Add(new Corruption(p.View.Interpreted, type));
                    }
                    p.Dispose();
                }
                cur_offset += Pokemon.PC_SIZE;
            }
            return res.ToArray();
        }

        public static int CompareEntries(SimulationEntry x, SimulationEntry y)
        {
            return ((int)x.type).CompareTo((int)y.type) switch
            {
                -1 => -1,
                1 => 1,
                0 => x.species.CompareTo(y.species),
                _ => throw new NotImplementedException()
            };
        }

        public record SimulationEntry(CorruptionType type, ushort species, EggType egg);
        public record OffsetASLR(int startOffset, int endOffset, int aslr);
        public record SimulationResult(int nbTries, Dictionary<SimulationEntry, List<OffsetASLR>> obtained);
        public SimulationResult Simulate()
        {
            Dictionary<SimulationEntry, List<OffsetASLR>> obtained = new Dictionary<SimulationEntry, List<OffsetASLR>>();
            int nbTries = 0;
            bool[] alreadySimulated = new bool[0x100];
            for (int i = 0; i < 0x100; i++)
            {
                int aslr = i & aslrMask;
                if (!alreadySimulated[aslr])
                {
                    alreadySimulated[aslr] = true;
                    nbTries++;
                    Corruption[] res = SimulateWithAslr(aslr);
                    foreach (Corruption cor in res)
                    {
                        SimulationEntry se = new SimulationEntry(cor.type, cor.interpreted.species, cor.interpreted.egg);
                        List<OffsetASLR> offsets;
                        if (obtained.ContainsKey(se))
                            offsets = obtained[se];
                        else
                            offsets = new List<OffsetASLR>();
                        OffsetASLR oa = new OffsetASLR((int)baseOffset - aslr, (int)maxOffset - aslr, aslr);
                        if (!offsets.Contains(oa)) offsets.Add(oa);
                        obtained[se] = offsets;
                    }
                }
            }
            return new SimulationResult(nbTries, obtained);
        }
    }
}
