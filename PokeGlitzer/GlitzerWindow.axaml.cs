using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PokeGlitzer
{
    public partial class GlitzerWindow : Window
    {
        public GlitzerWindow()
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(null, this,
                Utils.CollectionOfSize<byte>(Pokemon.PC_SIZE * MainWindowViewModel.BOX_SIZE * MainWindowViewModel.BOX_NUMBER), 0, GlitzerWindowViewModel.OffsetType.Start);
        }
        public GlitzerWindow(MainWindowViewModel mw, RangeObservableCollection<byte> data, int offset, GlitzerWindowViewModel.OffsetType type)
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(mw, this, data, offset, type);
        }

        protected override void OnClosed(EventArgs e)
        {
            ((GlitzerWindowViewModel)DataContext!).Dispose();
            base.OnClosed(e);
        }
    }
    public class GlitzerWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        MainWindowViewModel? mw;
        GlitzerWindow parent;
        const int NB_SLOTS = 12;
        public const int SIZE = NB_SLOTS * Pokemon.TEAM_SIZE;
        RangeObservableCollection<byte> data;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public enum OffsetType { Start, End, ASLR }
        public GlitzerWindowViewModel(MainWindowViewModel? mw, GlitzerWindow parent, RangeObservableCollection<byte> data, int offset, OffsetType t)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.mw = mw;
            this.parent = parent;
            this.data = data;
            
            glitzer = Utils.CollectionOfSize<PokemonExt?>(NB_SLOTS);
            try {
                if (t == OffsetType.Start)
                    CurrentOffset = offset;
                else if (t == OffsetType.End)
                    CurrentEndOffset = offset;
                else
                    CurrentASLR = offset;
            } catch { CurrentOffset = 0; }

            UpdateDataLocation();
        }

        void UpdateDataLocation()
        {
            int start = CurrentOffset;
            int end = start + SIZE;
            start = Math.Min(Math.Max(0, start), data.Count);
            end = Math.Min(Math.Max(0, end), data.Count);
            DataLocation = new DataLocation(start, end-start, Source.PC);
        }
        public void UpdateSelection()
        {
            for (int i = 0; i < Glitzer.Count; i++)
            {
                if (Glitzer[i] == null) continue;
                PokemonExt pkmn = Glitzer[i]!;
                bool selected = IsSelected(pkmn.pkmn);
                if (selected != pkmn.selected)
                    Glitzer[i] = new PokemonExt(pkmn.pkmn, selected);
            }
        }

        DataLocation dataLocation;
        public DataLocation DataLocation
        {
            get => dataLocation;
            private set
            {
                dataLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataLocation)));
            }
        }

        void DisposeGlitzer()
        {
            for (int i = 0; i < Glitzer.Count; i++)
            {
                if (Glitzer[i] != null)
                {
                    Glitzer[i]!.pkmn.Dispose();
                    Glitzer[i] = null;
                }
            }
        }
        void ReloadGlitzer()
        {
            DisposeGlitzer();
            int start = CurrentOffset;
            PokemonExt?[] pkmns = new PokemonExt?[NB_SLOTS];
            for (int i = 0; i < pkmns.Length; i++)
            {
                int offset = start + Pokemon.TEAM_SIZE * i;
                if (offset < 0) continue;
                if (offset + Pokemon.TEAM_SIZE > data.Count) continue;
                Pokemon pkmn = new Pokemon(data, new DataLocation(offset, Pokemon.TEAM_SIZE, Source.PC));
                pkmns[i] = new PokemonExt(pkmn, IsSelected(pkmn));
            }

            Utils.UpdateCollectionRange(Glitzer, pkmns);
        }

        bool IsSelected(Pokemon pkmn)
        {
            if (mw != null) return mw.IsSelected(pkmn);
            return false;
        }

        RangeObservableCollection<PokemonExt?> glitzer;
        public RangeObservableCollection<PokemonExt?> Glitzer { get => glitzer; }

        int currentOffset;
        public int CurrentOffset
        {
            get => currentOffset;
            set
            {
                /*if (value < 0 || value + SIZE > data.Count)
                    throw new Avalonia.Data.DataValidationException("Offset out of bounds.");*/
                currentOffset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOffset)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentEndOffset)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentASLR)));
                UpdateDataLocation();
                ReloadGlitzer();
            }
        }
        public int CurrentEndOffset
        {
            get => CurrentOffset + SIZE;
            set => CurrentOffset = value - SIZE;
        }
        public int CurrentASLR
        {
            get => GlitzerSimulation.EndOffset - CurrentEndOffset;
            set => CurrentEndOffset = GlitzerSimulation.EndOffset - value;
        }

        public void OpenInterpretedEditor(object dl) { mw?.OpenInterpretedEditor((DataLocation)dl, parent); }
        public void OpenDataEditor(object dl) { mw?.OpenDataEditor((DataLocation)dl, parent); }
        public void OpenRawEditor(object dl) { mw?.OpenRawEditor((DataLocation)dl, parent); }

        public void SelectSlot(object dl) { mw?.SelectSlot((DataLocation)dl, parent); }

        public void Delete(object dl) { mw?.Delete(dl); }
        public void Copy(object dl) { mw?.Copy(dl); }
        public void Cut(object dl) { mw?.Cut(dl); }
        public void Paste(object dl) { mw?.Paste(dl); }

        //public void ImportPk3Ek3(object dlo) { mw?.ImportPk3Ek3(dlo); }
        //public void ExportPk3Ek3(object dlo) { mw?.ExportPk3Ek3(dlo); }

        public void Prev() { try { CurrentOffset -= 4; } catch { } }
        public void Next() { try { CurrentOffset += 4; } catch { } }

        public void FlagBadEggs()
        {
            if (PreviousData != null && previousOffset == CurrentOffset) return;
            PreviousData = Utils.ExtractCollectionRange(data, DataLocation.offset, DataLocation.size);
            previousDataLocation = DataLocation;
            previousOffset = CurrentOffset;
            foreach (PokemonExt? p in Glitzer)
                if (p != null) p.pkmn.FlagAsBaddEggIfInvalid();
        }

        byte[]? previousData = null;
        DataLocation? previousDataLocation = null;
        int previousOffset = 0;
        public byte[]? PreviousData
        {
            get => previousData;
            private set
            {
                previousData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviousData)));
            }
        }
        public void Revert()
        {
            if (PreviousData == null) return;
            Utils.UpdateCollectionRange(data, PreviousData!, previousDataLocation!.offset);
            PreviousData = null;
        }

        public void Dispose()
        {
            DisposeGlitzer();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
