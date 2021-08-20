using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using MessageBox.Avalonia;

namespace PokeGlitzer
{
    public partial class GlitzerWindow : Window
    {
        public GlitzerWindow()
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(null, this,
                Utils.CollectionOfSize<byte>(Pokemon.PC_SIZE * MainWindowViewModel.BOX_SIZE * MainWindowViewModel.BOX_NUMBER), 0);
        }
        public GlitzerWindow(MainWindowViewModel mw, RangeObservableCollection<byte> data, int offset)
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(mw, this, data, offset);

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
        public GlitzerWindowViewModel(MainWindowViewModel? mw, GlitzerWindow parent, RangeObservableCollection<byte> data, int offset)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.mw = mw;
            this.parent = parent;
            this.data = data;
            
            glitzer = Utils.CollectionOfSize<PokemonExt?>(NB_SLOTS);
            try { CurrentOffset = offset; } catch { CurrentOffset = 0; }

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
                    throw new Avalonia.Data.DataValidationException(null);*/
                currentOffset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOffset)));
                UpdateDataLocation();
                ReloadGlitzer();
            }
        }

        public void OpenInterpretedEditor(DataLocation dl) { mw?.OpenInterpretedEditor(dl, parent); }
        public void OpenDataEditor(DataLocation dl) { mw?.OpenDataEditor(dl, parent); }
        public void OpenRawEditor(DataLocation dl) { mw?.OpenRawEditor(dl, parent); }

        public void SelectSlot(DataLocation dl) { mw?.SelectSlot(dl, parent); }

        public void Delete(DataLocation dl) { mw?.Delete(dl); }
        public void Copy(DataLocation dl) { mw?.Copy(dl); }
        public void Cut(DataLocation dl) { mw?.Cut(dl); }
        public void Paste(DataLocation dl) { mw?.Paste(dl); }

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
