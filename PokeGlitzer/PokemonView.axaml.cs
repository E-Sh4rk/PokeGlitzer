using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace PokeGlitzer
{
    public partial class PokemonViewWindow : Window, IEditorWindow
    {
        Pokemon pkmn;
        public PokemonViewWindow()
        {
            InitializeComponent();
            pkmn = new Pokemon(Utils.CollectionOfSize<byte>(80), DataLocation.DefaultPC);
            DataContext = new PokemonViewModel(pkmn, null);
        }
        public PokemonViewWindow(RangeObservableCollection<byte> data, int offset, Source src, MainWindowViewModel mw)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, new DataLocation(offset, 80, src));
            DataContext = new PokemonViewModel(pkmn, mw);
        }
        public DataLocation DataLocation => pkmn.DataLocation;

        protected override void OnClosed(EventArgs e)
        {
            pkmn.Dispose();
            base.OnClosed(e);
        }
    }
    public class PokemonViewModel : INotifyPropertyChanged
    {
        Pokemon pkmn;
        PokemonView view;
        MainWindowViewModel? mw;

        public PokemonViewModel(Pokemon pkmn, MainWindowViewModel? mw)
        {
            this.mw = mw;
            this.pkmn = pkmn;
            view = pkmn.View;
            displayData = view.Data;
            decoded = false;
        }

        bool decoded;
        public bool Decoded
        {
            get => decoded;
            set
            {
                decoded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Decoded)));
                DisplayData = decoded ? view.DecodedData : view.Data;
            }
        }

        RangeObservableCollection<byte> displayData;
        public RangeObservableCollection<byte> DisplayData
        {
            get => displayData;
            set
            {
                displayData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayData)));
            }
        }

        public PokemonView View {
            get => view;
        }

        public void RestoreInitial() => mw?.RestoreInitialData(pkmn.DataLocation);
            
        public void FixChecksum() => pkmn.FixChecksum();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
