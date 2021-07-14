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
    }
}
