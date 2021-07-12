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
        // TODO: Investigate the memory leak when opening/closing multiple editors
        RangeObservableCollection<byte> data;
        public MainWindowViewModel()
        {
            data = Utils.ByteCollectionOfSize(80);
        }
        public void OpenPokemonInterpreted() => new InterpretedEditor(data, 0).Show();
        public void OpenPokemonData() => new PokemonViewWindow(data, 0).Show();
        public void OpenPokemonRaw() => new HexEditor(data, 0).Show();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
