using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.MemoryMappedFiles;
using System.Linq;

namespace PokeGlitzer
{
    public class MMFSync : INotifyPropertyChanged
    {
        RangeObservableCollection<byte> data;
        RangeObservableCollection<byte> teamData;
        DispatcherTimer? timer;

        public MMFSync(RangeObservableCollection<byte> data, RangeObservableCollection<byte> teamData)
        {
            this.data = data;
            this.teamData = teamData;
        }

        enum CHANNEL_DIR { IN = 0, OUT = 1 }
        const int NUMBER_CHANNELS = 4;
        const int PC_IN = 0;
        const int PC_OUT = 1;
        const int TEAM_IN = 2;
        const int TEAM_OUT = 3;
        static readonly CHANNEL_DIR[] CHANNELS_DIRECTION =
            new CHANNEL_DIR[] { CHANNEL_DIR.IN, CHANNEL_DIR.OUT, CHANNEL_DIR.IN, CHANNEL_DIR.OUT };
        static readonly int[] CHANNELS_LENGTH =
            new int[] { MainWindowViewModel.BOX_NUMBER* MainWindowViewModel.BOX_SIZE*Pokemon.PC_SIZE,
                        MainWindowViewModel.BOX_NUMBER* MainWindowViewModel.BOX_SIZE*Pokemon.PC_SIZE,
                        MainWindowViewModel.TEAM_SIZE * Pokemon.TEAM_SIZE,
                        MainWindowViewModel.TEAM_SIZE * Pokemon.TEAM_SIZE
            };

        MemoryMappedFile[]? mmfData;
        MemoryMappedViewAccessor[]? mmfAcc;
        MemoryMappedFile[]? mmfCData;
        MemoryMappedViewAccessor[]? mmfCAcc;
        byte[]? mmfC;
        bool[]? boxLock;
        bool[]? teamLock;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "42")]
        public bool Start()
        {
            if (IsRunning) return false;
            try
            {
                timer = new DispatcherTimer(DispatcherPriority.Input);
                timer.Interval = TimeSpan.FromMilliseconds(250);

                mmfData = new MemoryMappedFile[NUMBER_CHANNELS];
                mmfAcc = new MemoryMappedViewAccessor[NUMBER_CHANNELS];
                mmfCData = new MemoryMappedFile[NUMBER_CHANNELS];
                mmfCAcc = new MemoryMappedViewAccessor[NUMBER_CHANNELS];
                mmfC = new byte[NUMBER_CHANNELS];

                string[] CHANNELS_NAME = new string[] { Settings.MMF_PC_IN, Settings.MMF_PC_OUT, Settings.MMF_PARTY_IN, Settings.MMF_PARTY_OUT };
                for (int i = 0; i < NUMBER_CHANNELS; i++)
                {
                    string name = CHANNELS_NAME[i];
                    int len = CHANNELS_LENGTH[i];
                    if (CHANNELS_DIRECTION[i] == CHANNEL_DIR.IN)
                    {
                        mmfData[i] = MemoryMappedFile.OpenExisting(name, MemoryMappedFileRights.Read);
                        mmfAcc[i] = mmfData[i].CreateViewAccessor(0, len, MemoryMappedFileAccess.Read);
                        mmfCData[i] = MemoryMappedFile.OpenExisting(name + "_c", MemoryMappedFileRights.Read);
                        mmfCAcc[i] = mmfCData[i].CreateViewAccessor(0, 0x1, MemoryMappedFileAccess.Read);
                        mmfC[i] = (byte)(mmfCAcc[i].ReadByte(0) - 1);
                    }
                    else
                    {
                        mmfData[i] = MemoryMappedFile.OpenExisting(name, MemoryMappedFileRights.Write);
                        mmfAcc[i] = mmfData[i].CreateViewAccessor(0, len, MemoryMappedFileAccess.Write);
                        mmfCData[i] = MemoryMappedFile.OpenExisting(name + "_c", MemoryMappedFileRights.ReadWrite);
                        mmfCAcc[i] = mmfCData[i].CreateViewAccessor(0, 0x1, MemoryMappedFileAccess.ReadWrite);
                        mmfC[i] = (byte)(mmfCAcc[i].ReadByte(0) + 1);
                    }
                }
                boxLock = new bool[MainWindowViewModel.BOX_NUMBER * MainWindowViewModel.BOX_SIZE];
                teamLock = new bool[MainWindowViewModel.TEAM_SIZE];

                timer.Tick += Refresh;
                timer.Start();
                data.CollectionChanged += DataChanged;
                teamData.CollectionChanged += DataChanged;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                return true;
            }
            catch { Stop(); }
            return false;
        }

        public bool IsRunning { get => timer != null; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Stop()
        {
            data.CollectionChanged -= DataChanged;
            teamData.CollectionChanged -= DataChanged;
            if (IsRunning)
            {
                timer!.Stop();
                timer!.Tick -= Refresh;
                timer = null;
            }

            for (int i = 0; i < NUMBER_CHANNELS; i++)
            {
                if (mmfAcc != null && mmfAcc[i] != null) mmfAcc[i].Dispose();
                if (mmfCAcc != null && mmfCAcc[i] != null) mmfCAcc[i].Dispose();
                if (mmfData != null && mmfData[i] != null) mmfData[i].Dispose();
                if (mmfCData != null && mmfCData[i] != null) mmfCData[i].Dispose();
            }
            mmfData = null; mmfAcc = null; mmfCData = null; mmfCAcc = null; mmfC = null;
            boxLock = null; teamLock = null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }

        bool CheckChannelUpdate(int id)
        {
            byte c = mmfCAcc![id].ReadByte(0);
            if (c != mmfC![id])
            {
                mmfC![id] = c;
                return true;
            }
            return false;
        }
        byte[] ReadChannel(int id)
        {
            int len = CHANNELS_LENGTH[id];
            byte[] res = new byte[len];
            mmfAcc![id].ReadArray(0, res, 0, len);
            return res;
        }
        byte[] ReadChannel(int id, int start, int count)
        {
            byte[] res = new byte[count];
            mmfAcc![id].ReadArray(start, res, 0, count);
            return res;
        }
        void WriteChannel(int id, byte[] data, int position = 0)
        {
            mmfAcc![id].WriteArray(position, data, 0, data.Length);
            mmfCAcc![id].Write(0, mmfC![id]);
            mmfC![id]++;
        }

        private bool IsLocked(DataLocation dl)
        {
            bool[] l = dl.inTeam ? teamLock! : boxLock!;
            int s = dl.inTeam ? Pokemon.TEAM_SIZE : Pokemon.PC_SIZE;
            int offset = 0;
            foreach (bool b in l)
            {
                if (b && dl.Intersect(new DataLocation(offset,s,dl.inTeam)))
                    return true;
                offset += s;
            }
            return false;
        }
        private void DataChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsRunning) return;

            if (Utils.IsNonTrivialReplacement(e))
            {
                int i = e.OldStartingIndex;
                int c = e.NewItems!.Count;
                if (sender == data && !IsLocked(new DataLocation(i, c, false))) // Boxes
                {
                    // NOTE: The check below is not needed anymore (locks are enough). Also, it can cause issue when the emulator is paused.
                    /*byte[] emu_data = ReadChannel(PC_IN, i, c);
                    byte[] new_data = Utils.ExtractCollectionRange(data, i, c);
                    if (!Enumerable.SequenceEqual(new_data, emu_data))*/
                    WriteChannel(PC_OUT, data.ToArray());
                }
                else if (sender == teamData && !IsLocked(new DataLocation(i, c, true))) // Team
                {
                    // NOTE: The check below is not needed anymore (locks are enough). Also, it can cause issue when the emulator is paused.
                    /*byte[] emu_data = ReadChannel(TEAM_IN, i, c);
                    byte[] new_data = Utils.ExtractCollectionRange(teamData, i, c);
                    if (!Enumerable.SequenceEqual(new_data, emu_data))*/
                    WriteChannel(TEAM_OUT, teamData.ToArray());
                }

            }
        }

        private void Refresh(object? sender, EventArgs e)
        {
            if (!IsRunning) return;

            if (CheckChannelUpdate(PC_IN))
            {
                // Boxes
                byte[] oldData = data.ToArray();
                for (int i = 0; i < boxLock!.Length; i++)
                {
                    int offset = i * Pokemon.PC_SIZE;
                    byte[] pkmn = ReadChannel(PC_IN, offset, Pokemon.PC_SIZE);
                    IEnumerable<byte> oldPkmn = new ArraySegment<byte>(oldData, offset, Pokemon.PC_SIZE);
                    if (!Enumerable.SequenceEqual(pkmn, oldPkmn))
                    {
                        // We do not allow modification of a slot that has just been modified by the game, because its data can be incorrect (due to DMA copies)
                        boxLock![i] = true;
                        Utils.UpdateCollectionRange(data, pkmn, offset);
                    }
                    else boxLock![i] = false;
                }
            }
            if (CheckChannelUpdate(TEAM_IN))
            {
                // Team
                byte[] oldData = teamData.ToArray();
                for (int i = 0; i < teamLock!.Length; i++)
                {
                    int offset = i * Pokemon.TEAM_SIZE;
                    byte[] pkmn = ReadChannel(TEAM_IN, offset, Pokemon.TEAM_SIZE);
                    IEnumerable<byte> oldPkmn = new ArraySegment<byte>(oldData, offset, Pokemon.TEAM_SIZE);
                    if (!Enumerable.SequenceEqual(pkmn, oldPkmn))
                    {
                        // We do not allow modification of a slot that has just been modified by the game, because its data can be incorrect (due to DMA copies)
                        teamLock![i] = true;
                        Utils.UpdateCollectionRange(teamData, pkmn, offset);
                    }
                    else teamLock![i] = false;
                }
            }
        }
    }
}
