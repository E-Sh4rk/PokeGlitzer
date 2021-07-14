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
    // TODO: When an edition window is focused, highlight in yellow the corresponding pokemon on the main window
    // TODO: And the opposite way: when a pokemin is selected on the main window, focus on the corresponding editor (double click = open new editor)
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);

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
        MainWindow mw;
        RangeObservableCollection<byte> data;
        const int BOX_PKMN_SIZE = 80;
        const int BOX_SIZE = 30;
        const int BOX_NUMBER = 14;
        Save? save = null;
        public MainWindowViewModel(MainWindow mw)
        {
            this.mw = mw;
            data = Utils.ByteCollectionOfSize(BOX_PKMN_SIZE * BOX_SIZE * BOX_NUMBER);
            Pokemon?[] box1 = new Pokemon[BOX_SIZE];
            for (int i = 0; i < box1.Length; i++) box1[i] = new Pokemon(data, BOX_PKMN_SIZE * i);
            currentBox = new RangeObservableCollection<Pokemon?>(box1);
        }

        RangeObservableCollection<Pokemon?> currentBox;
        public RangeObservableCollection<Pokemon?> CurrentBox { get => currentBox; }
        public Save? CurrentSave
        {
            get => save;
            set { save = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Save))); }
        }

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
        public void Exit()
        {
            mw.Close();
        }
        public async void Open()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Save file", Extensions = { "sav" } });
            dialog.AllowMultiple = false;

            string[] result = await dialog.ShowAsync(mw);
            if (result != null && result.Length >= 1)
            {
                try {
                    CurrentSave = new Save(result[0]);
                    Utils.UpdateCollectionRange(data, CurrentSave.RetrievePCData().pokemonList);
                } catch { }
            }
        }
        public void Save()
        {
            // TODO
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
