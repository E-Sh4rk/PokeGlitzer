using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        const int BOX_PKMN_SIZE = 80;
        const int BOX_SIZE = 30;
        public MainWindowViewModel()
        {
            data = Utils.ByteCollectionOfSize(BOX_PKMN_SIZE * BOX_SIZE);
            Pokemon?[] box1 = new Pokemon[BOX_SIZE];
            for (int i = 0; i < box1.Length; i++) box1[i] = new Pokemon(data, BOX_PKMN_SIZE * i);
            currentBox = new RangeObservableCollection<Pokemon?>(box1);
        }

        RangeObservableCollection<Pokemon?> currentBox;
        public RangeObservableCollection<Pokemon?> CurrentBox { get => currentBox; }

        public void OpenInterpretedEditor(Pokemon arg)
        {
            new InterpretedEditor(data, arg.DataOffset).Show();
        }
        public void OpenDataEditor(Pokemon arg)
        {
            new PokemonViewWindow(data, arg.DataOffset).Show();
        }
        public void OpenRawEditor(Pokemon arg)
        {
            new HexEditor(data, arg.DataOffset).Show();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
