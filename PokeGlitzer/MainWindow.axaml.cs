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
            data = Utils.ByteCollectionOfSize<byte>(BOX_PKMN_SIZE * BOX_SIZE * BOX_NUMBER);
            currentBox = Utils.ByteCollectionOfSize<Pokemon?>(BOX_SIZE);
            LoadBox(0);
        }

        void LoadBox(int nb)
        {
            int offset = BOX_PKMN_SIZE * BOX_SIZE * nb;
            for (int i = 0; i < CurrentBox.Count; i++)
            {
                if (CurrentBox[i] != null)
                    CurrentBox[i]!.Dispose();
            }
            Pokemon?[] pkmns = new Pokemon?[BOX_SIZE];
            for (int i = 0; i < pkmns.Length; i++)
                pkmns[i] = new Pokemon(data, BOX_PKMN_SIZE * i + offset);
            Utils.UpdateCollectionRange(CurrentBox, pkmns);
            CurrentBoxNumber = nb+1;
        }

        RangeObservableCollection<Pokemon?> currentBox;
        int currentBoxNumber;
        public int CurrentBoxNumber {
            get => currentBoxNumber + 1;
            set { currentBoxNumber = value - 1; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoxNumber))); }
        }
        public RangeObservableCollection<Pokemon?> CurrentBox { get => currentBox; }
        public Save? CurrentSave
        {
            get => save;
            set { save = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSave))); }
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
            if (CurrentSave != null)
            {
                PCData pcd = CurrentSave.RetrievePCData();
                pcd.pokemonList = Utils.ExtractCollectionRange(data, 0, data.Count);
                CurrentSave.SetPCData(pcd);
                CurrentSave.SaveToFile();
            }
        }
        public void NextBox()
        {
            LoadBox((currentBoxNumber+1) % BOX_NUMBER);
        }
        public void PrevBox()
        {
            LoadBox((currentBoxNumber+BOX_NUMBER-1) % BOX_NUMBER);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
