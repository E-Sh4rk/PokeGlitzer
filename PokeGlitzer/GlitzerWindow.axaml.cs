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
                Utils.CollectionOfSize<byte>(MainWindowViewModel.BOX_PKMN_SIZE * MainWindowViewModel.BOX_SIZE * MainWindowViewModel.BOX_NUMBER));
        }
        public GlitzerWindow(MainWindowViewModel mw, RangeObservableCollection<byte> data)
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(mw, this, data);

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
        const int SIZE = NB_SLOTS * MainWindowViewModel.TEAM_PKMN_SIZE;
        RangeObservableCollection<byte> data;
        public GlitzerWindowViewModel(MainWindowViewModel? mw, GlitzerWindow parent, RangeObservableCollection<byte> data)
        {
            this.mw = mw;
            this.parent = parent;
            this.data = data;
            UpdateDataLocation();
            glitzer = Utils.CollectionOfSize<PokemonExt?>(NB_SLOTS);
            ReloadGlitzer();
        }

        void UpdateDataLocation()
        {
            DataLocation = new DataLocation(CurrentOffset, Math.Min(SIZE, data.Count - CurrentOffset), false);
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
                Pokemon pkmn = new Pokemon(data, start + MainWindowViewModel.TEAM_PKMN_SIZE * i, MainWindowViewModel.TEAM_PKMN_SIZE, false);
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

        int currentOffset = 0;
        public int CurrentOffset
        {
            get => currentOffset;
            set
            {
                if (value < 0 || value + SIZE >= data.Count)
                    throw new Avalonia.Data.DataValidationException(null);
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

        public void Prev() { try { CurrentOffset -= 4; } catch { } }
        public void Next() { try { CurrentOffset += 4; } catch { } }

        public void Dispose()
        {
            DisposeGlitzer();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
