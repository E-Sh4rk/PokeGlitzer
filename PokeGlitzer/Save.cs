using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public class Save
    {
        public enum SaveSlot { SlotA, SlotB }

        string path;
        PokemonSaveFile save;
        SaveSlot currentSaveSlot;
        uint gameCode;
        public Save(string path)
        {
            this.path = path;
            byte[] data = File.ReadAllBytes(path);
            string backupName = Path.GetFileNameWithoutExtension(path) + ".old" + Path.GetExtension(path);
            string? dirName = Path.GetDirectoryName(path);
            string backupPath = dirName != null ? Path.Combine(dirName, backupName) : backupName;
            File.WriteAllBytes(backupPath, data);

            byte[] resizedData = new byte[Marshal.SizeOf(typeof(PokemonSaveFile))];
            Array.Copy(data, resizedData, Math.Min(data.Length, resizedData.Length));
            save = Utils.ByteToType<PokemonSaveFile>(resizedData);

            uint siA = GetSaveIndex(save.sectionsA);
            uint siB = GetSaveIndex(save.sectionsB);
            if (siA >= siB && CheckSections(save.sectionsA))
                currentSaveSlot = SaveSlot.SlotA;
            else if (CheckSections(save.sectionsB))
                currentSaveSlot = SaveSlot.SlotB;
            else
                throw new FormatException("The save file is corrupted.");

            Section tiSection = GetSection(TrainerInfoData.SECTION_ID)!.Value;
            byte[] tiData = new byte[Marshal.SizeOf(typeof(TrainerInfoData))];
            Array.Copy(tiSection.data, tiData, tiData.Length);
            TrainerInfoData ti = Utils.ByteToType<TrainerInfoData>(tiData);
            gameCode = ti.gameCode;
        }
        uint GetSaveIndex(Section[] sections)
        {
            uint max = 0;
            foreach (Section s in sections)
                if (s.saveIndex > max) max = s.saveIndex;
            return max;
        }
        ushort ComputeChecksum(byte[] data, int size)
        {
            uint checksum = 0;
            for (int i = 0; i < size; i += 4)
                checksum += BitConverter.ToUInt32(data, i);
            return (ushort)((checksum >> 16) + (checksum & 0xFFFF));
        }
        bool CheckSection(Section s)
        {
            if (s.signature != Section.MAGIC_NUMBER) return false;
            int sectionSize = Section.DATA_SIZE[s.sectionID];
            if (ComputeChecksum(s.data, sectionSize) != s.checksum) return false;
            return true;
        }
        bool CheckSections(Section[] sections)
        {
            foreach (Section s in sections)
                if (!CheckSection(s)) return false;
            return true;
        }

        public void SaveToFile()
        {
            byte[] data = Utils.TypeToByte(save);
            File.WriteAllBytes(path, data);
        }

        Section[] CurrentSlot
        {
            get { return currentSaveSlot == SaveSlot.SlotA ? save.sectionsA : save.sectionsB; }
            set { if (currentSaveSlot == SaveSlot.SlotA) save.sectionsA = value; else save.sectionsB = value; }
        }

        Section? GetSection(int ID)
        {
            foreach (Section s in CurrentSlot)
                if (s.sectionID == ID) return s;
            return null;
        }

        void SetSection(Section s)
        {
            Section[] sections = CurrentSlot;
            for (int i = 0; i < sections.Length; i++)
                if (sections[i].sectionID == s.sectionID)
                    sections[i] = s;
            CurrentSlot = sections;
        }

        public PCData RetrievePCData()
        {
            byte[] res = new byte[Marshal.SizeOf(typeof(PCData))];
            int offset = 0;
            for (int i = PCData.MIN_SECTION_ID; i <= PCData.MAX_SECTION_ID; i++)
            {
                Section s = GetSection(i)!.Value;
                int sectionSize = Section.DATA_SIZE[i];
                Array.Copy(s.data, 0, res, offset, sectionSize);
                offset += sectionSize;
            }
            return Utils.ByteToType<PCData>(res);
        }

        public void SetPCData(PCData data)
        {
            byte[] d = Utils.TypeToByte(data);
            int offset = 0;
            for (int i = PCData.MIN_SECTION_ID; i <= PCData.MAX_SECTION_ID; i++)
            {
                int sectionSize = Section.DATA_SIZE[i];
                Section s = GetSection(i)!.Value;
                Array.Copy(d, offset, s.data, 0, sectionSize);
                s.checksum = ComputeChecksum(s.data, sectionSize);
                SetSection(s);
                offset += sectionSize;
            }
        }

        public TeamItemsData RetrieveTeamData()
        {
            int offset = gameCode == TrainerInfoData.GAMECODE_FRLG ? TeamItemsData.SECTION_OFFSET_FRLG : TeamItemsData.SECTION_OFFSET_RSE;
            Section tSection = GetSection(TeamItemsData.SECTION_ID)!.Value;
            byte[] tData = new byte[Marshal.SizeOf(typeof(TeamItemsData))];
            Array.Copy(tSection.data, offset, tData, 0, tData.Length);
            return Utils.ByteToType<TeamItemsData>(tData);
        }
        public void SetTeamData(TeamItemsData ti)
        {
            int offset = gameCode == TrainerInfoData.GAMECODE_FRLG ? TeamItemsData.SECTION_OFFSET_FRLG : TeamItemsData.SECTION_OFFSET_RSE;

            byte[] tData = Utils.TypeToByte(ti);
            Section tSection = GetSection(TeamItemsData.SECTION_ID)!.Value;
            Array.Copy(tData, 0, tSection.data, offset, tData.Length);
            tSection.checksum = ComputeChecksum(tSection.data, Section.DATA_SIZE[TeamItemsData.SECTION_ID]);
            SetSection(tSection);
        }
    }
}
