using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace PokeGlitzer
{
    // TODO: Wheb a pokemon is clicked on the main window, focus on the corresponding editor (or open a new one)
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
    public record PokemonExt(Pokemon pkmn, bool selected);
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
            currentBox = Utils.ByteCollectionOfSize<PokemonExt?>(BOX_SIZE);
            LoadBox(0);
        }

        void LoadBox(int nb)
        {
            int offset = BOX_PKMN_SIZE * BOX_SIZE * nb;
            for (int i = 0; i < CurrentBox.Count; i++)
            {
                if (CurrentBox[i] != null)
                    CurrentBox[i]!.pkmn.Dispose();
            }
            PokemonExt?[] pkmns = new PokemonExt?[BOX_SIZE];
            for (int i = 0; i < pkmns.Length; i++)
            {
                Pokemon pkmn = new Pokemon(data, BOX_PKMN_SIZE * i + offset);
                pkmns[i] = new PokemonExt(pkmn, IsSelected(pkmn));
            }
                
            Utils.UpdateCollectionRange(CurrentBox, pkmns);
            CurrentBoxNumber = nb+1;
        }
        bool IsSelected(Pokemon pkmn)
        {
            if (Selection == null) return false;
            if (pkmn.DataLocation.Intersect(Selection)) return true;
            return false;
        }

        RangeObservableCollection<PokemonExt?> currentBox;
        int currentBoxNumber;
        DataLocation? selection;
        public int CurrentBoxNumber {
            get => currentBoxNumber + 1;
            set { currentBoxNumber = value - 1; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoxNumber))); }
        }
        public RangeObservableCollection<PokemonExt?> CurrentBox { get => currentBox; }
        public Save? CurrentSave
        {
            get => save;
            set { save = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSave))); }
        }
        public DataLocation? Selection
        {
            get => selection;
            set
            {
                selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selection)));
                for (int i = 0; i < CurrentBox.Count; i++)
                {
                    if (CurrentBox[i] == null) continue;
                    PokemonExt pkmn = CurrentBox[i]!;
                    bool selected = IsSelected(pkmn.pkmn);
                    if (selected != pkmn.selected)
                        CurrentBox[i] = new PokemonExt(pkmn.pkmn, selected);
                }
            }
        }

        List<IEditorWindow> openedEditors = new List<IEditorWindow>();
        public void OpenInterpretedEditor(Pokemon arg)
        {
            ShowWindow(new InterpretedEditor(data, arg.DataLocation.offset));
        }
        public void OpenDataEditor(Pokemon arg)
        {
            ShowWindow(new PokemonViewWindow(data, arg.DataLocation.offset));
        }
        public void OpenRawEditor(Pokemon arg)
        {
            ShowWindow(new HexEditor(data, arg.DataLocation.offset));
        }
        public void ShowWindow(IEditorWindow w)
        {
            openedEditors.Add(w);
            w.Closed += (_, _) => { openedEditors.Remove(w); GC.Collect(); };
            w.Activated += (_, _) => { Selection = w.Pokemon.DataLocation; };
            w.Deactivated += (_, _) => { Selection = null; };
            w.Show(mw);
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
