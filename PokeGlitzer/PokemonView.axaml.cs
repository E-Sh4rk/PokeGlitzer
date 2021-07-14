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
            pkmn = new Pokemon(Utils.ByteCollectionOfSize<byte>(80), 0);
            DataContext = new PokemonViewModel(pkmn);
        }
        public PokemonViewWindow(RangeObservableCollection<byte> data, int offset)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, offset);
            DataContext = new PokemonViewModel(pkmn);

#if DEBUG
            this.AttachDevTools();
#endif
        }
        public Pokemon Pokemon => pkmn;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

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

        public PokemonViewModel(Pokemon pkmn)
        {
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

        public void RestoreInitial() => Utils.UpdateCollectionRange(view.Data, new byte[view.Data.Count]);
        public void FixChecksum() => pkmn.FixChecksum();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
