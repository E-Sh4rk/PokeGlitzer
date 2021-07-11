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
    public partial class PokemonViewWindow : Window
    {
        public PokemonViewWindow()
        {
            InitializeComponent();
            DataContext = new PokemonViewModel();
        }
        public PokemonViewWindow(PokemonView view)
        {
            InitializeComponent();
            DataContext = new PokemonViewModel(view);

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class PokemonViewModel : INotifyPropertyChanged
    {
        PokemonView view;

        public PokemonViewModel()
        {
            view = new PokemonView(80);
            displayData = view.Data;
            decoded = false;
        }
        public PokemonViewModel(PokemonView view)
        {
            this.view = view;
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
                DisplayData = decoded ? view.DecodedData : view.Data;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Decoded)));
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

        public void RestoreInitial() => Utils.UpdateCollectionRange(view.Data, new byte[80]);

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
