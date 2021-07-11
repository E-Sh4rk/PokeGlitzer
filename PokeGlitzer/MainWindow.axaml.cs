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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        RangeObservableCollection<byte> data;
        Pokemon pkmn;
        public MainWindowViewModel()
        {
            data = new RangeObservableCollection<byte>(new byte[80]);
            pkmn = new Pokemon(data, 0);
        }
        public void OpenPokemonData() => new PokemonViewWindow(pkmn).Show();
        public void OpenPokemonRaw() => new HexEditor(pkmn).Show();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
