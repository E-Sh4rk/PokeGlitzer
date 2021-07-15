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

namespace PokeGlitzer
{
    // TODO: Copy and Paste
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
        byte[] initialData;
        RangeObservableCollection<byte> data;
        const int BOX_PKMN_SIZE = 80;
        const int BOX_SIZE = 30;
        const int BOX_NUMBER = 14;
        Save? save = null;
        public MainWindowViewModel(MainWindow mw)
        {
            this.mw = mw;
            data = Utils.ByteCollectionOfSize<byte>(BOX_PKMN_SIZE * BOX_SIZE * BOX_NUMBER);
            initialData = new byte[data.Count];
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
        public void OpenInterpretedEditor(DataLocation dl)
        {
            ShowWindow(new InterpretedEditor(data, dl.offset));
        }
        public void OpenDataEditor(DataLocation dl)
        {
            ShowWindow(new PokemonViewWindow(data, dl.offset, this));
        }
        public void OpenRawEditor(DataLocation dl)
        {
            ShowWindow(new HexEditor(data, dl.offset));
        }
        public void ShowWindow(IEditorWindow w)
        {
            openedEditors.Add(w);
            w.Closed += (_, _) => { openedEditors.Remove(w); GC.Collect(); };
            w.Activated += (_, _) => { Selection = w.Pokemon.DataLocation; };
            w.Deactivated += (_, _) => { Selection = null; };
            w.Show(mw);
        }
        static readonly ISolidColorBrush HIGHLIGHT_BRUSH = new SolidColorBrush(Colors.Yellow, 150);
        public void SelectSlot(DataLocation dl)
        {
            bool openNew = true;
            foreach (IEditorWindow w in openedEditors)
            {
                if (w.Pokemon.DataLocation.Intersect(dl))
                {
                    openNew = false;
                    w.Activate();
                    if (w.Background != HIGHLIGHT_BRUSH)
                    {
                        IBrush bg = w.Background;
                        w.Background = HIGHLIGHT_BRUSH;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromMilliseconds(250);
                        timer.Tick += new EventHandler((_, _) => { try { w.Background = bg; } catch { } timer.Stop(); });
                        timer.Start();
                    }
                }
            }
            if (openNew)
                OpenInterpretedEditor(dl);
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
                    initialData = data.ToArray();
                } catch { }
            }
        }
        public void Save()
        {
            if (CurrentSave != null)
            {
                try
                {
                    PCData pcd = CurrentSave.RetrievePCData();
                    pcd.pokemonList = data.ToArray();
                    CurrentSave.SetPCData(pcd);
                    CurrentSave.SaveToFile();
                    initialData = pcd.pokemonList;
                }
                catch { }
            }
        }

        public void RestoreInitialData(DataLocation dl)
        {
            Utils.UpdateCollectionRange(data, new ArraySegment<byte>(initialData, dl.offset, dl.size), dl.offset);
        }
        public void Delete(DataLocation dl)
        {
            Utils.UpdateCollectionRange(data, new byte[dl.size], dl.offset);
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
