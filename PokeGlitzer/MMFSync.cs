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
        // TODO: Add control mmf for bizhawk_down? (so that changes are not rolled-back when the game is paused)
        RangeObservableCollection<byte> data;
        DispatcherTimer? timer;

        public MMFSync(RangeObservableCollection<byte> data)
        {
            this.data = data;
        }

        MemoryMappedFile? inData;
        MemoryMappedViewAccessor? inAcc;
        MemoryMappedFile? outData;
        MemoryMappedViewAccessor? outAcc;
        MemoryMappedFile? cOutData;
        MemoryMappedViewAccessor? cOutAcc;
        byte cOut;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "42")]
        public bool Start()
        {
            if (IsRunning) return false;
            try
            {
                timer = new DispatcherTimer(DispatcherPriority.Input);
                timer.Interval = TimeSpan.FromMilliseconds(500);

                inData = MemoryMappedFile.OpenExisting("bizhawk_down", MemoryMappedFileRights.Read);
                inAcc = inData.CreateViewAccessor(0, data.Count, MemoryMappedFileAccess.Read);
                outData = MemoryMappedFile.OpenExisting("bizhawk_up", MemoryMappedFileRights.Write);
                outAcc = outData.CreateViewAccessor(0, data.Count, MemoryMappedFileAccess.Write);
                cOutData = MemoryMappedFile.OpenExisting("bizhawk_upc", MemoryMappedFileRights.ReadWrite);
                cOutAcc = cOutData.CreateViewAccessor(0, 0x1, MemoryMappedFileAccess.ReadWrite);
                cOut = cOutAcc.ReadByte(0);

                timer.Tick += Refresh;
                timer.Start();
                data.CollectionChanged += DataChanged;
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
            if (IsRunning)
            {
                timer!.Stop();
                timer!.Tick -= Refresh;
                timer = null;
            }

            cOutAcc?.Dispose(); cOutAcc = null;
            cOutData?.Dispose(); cOutData = null;
            outAcc?.Dispose(); outAcc = null;
            outData?.Dispose(); outData = null;
            inAcc?.Dispose(); inAcc = null;
            inData?.Dispose(); inData = null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }

        private void DataChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsRunning) return;
            int i; int c;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    i = e.OldStartingIndex; c = e.NewItems!.Count;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    i = 0; c = data.Count;
                    break;
                default:
                    throw new NotImplementedException();
            }
            byte[] emu_data = new byte[c];
            inAcc!.ReadArray(i, emu_data, 0, c);
            byte[] new_data = Utils.ExtractCollectionRange(data, i, c);

            if (!Enumerable.SequenceEqual(new_data, emu_data))
            {
                cOut++;
                byte[] dataArr = data.ToArray();
                outAcc!.WriteArray(0, dataArr, 0, dataArr.Length);
                cOutAcc!.Write(0, cOut);
            }
        }

        const int PKMN_SIZE = 80;
        const int PKMN_NB = 30*14;

        private void Refresh(object? sender, EventArgs e)
        {
            if (!IsRunning) return;
            byte[] oldData = data.ToArray();

            for (int i = 0; i < PKMN_NB; i++)
            {
                int offset = i * PKMN_SIZE;
                byte[] pkmn = new byte[PKMN_SIZE];
                inAcc!.ReadArray(offset, pkmn, 0, PKMN_SIZE);
                IEnumerable<byte> oldPkmn = new ArraySegment<byte>(oldData, offset, PKMN_SIZE);
                if (!Enumerable.SequenceEqual(pkmn, oldPkmn))
                    Utils.UpdateCollectionRange(data, pkmn, offset);
            }
        }
    }
}
