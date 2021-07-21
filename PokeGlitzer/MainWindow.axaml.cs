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
using MessageBox.Avalonia;

namespace PokeGlitzer
{
    // TODO: Possibility to edit the whole box data (big hex editor)
    // TODO: Possibility to edit the box names
    // TODO: Glitzer Popping operations (simulations, etc.)
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
        // TODO: Synchronize team data
        MainWindow mw;
        byte[] initialData;
        byte[] initialTeamData;
        byte[]? copiedData = null;
        RangeObservableCollection<byte> data;
        RangeObservableCollection<byte> teamData;
        const int BOX_PKMN_SIZE = 80;
        const int BOX_SIZE = 30;
        const int BOX_NUMBER = 14;
        const int TEAM_PKMN_SIZE = 100;
        const int TEAM_SIZE = 6;
        Save? save = null;
        MMFSync sync;
        public MainWindowViewModel(MainWindow mw)
        {
            this.mw = mw;
            data = Utils.ByteCollectionOfSize<byte>(BOX_PKMN_SIZE * BOX_SIZE * BOX_NUMBER);
            teamData = Utils.ByteCollectionOfSize<byte>(TEAM_PKMN_SIZE * TEAM_SIZE);
            initialData = new byte[data.Count];
            initialTeamData = new byte[teamData.Count];
            currentBox = Utils.ByteCollectionOfSize<PokemonExt?>(BOX_SIZE);
            LoadBox(0);
            team = Utils.ByteCollectionOfSize<PokemonExt?>(TEAM_SIZE);
            LoadTeam();
            sync = new MMFSync(data);
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
                Pokemon pkmn = new Pokemon(data, BOX_PKMN_SIZE * i + offset, BOX_PKMN_SIZE, false);
                pkmns[i] = new PokemonExt(pkmn, IsSelected(pkmn));
            }
                
            Utils.UpdateCollectionRange(CurrentBox, pkmns);
            CurrentBoxNumber = nb+1;
        }
        void LoadTeam()
        {
            for (int i = 0; i < Team.Count; i++)
            {
                if (Team[i] != null)
                    Team[i]!.pkmn.Dispose();
            }
            PokemonExt?[] pkmns = new PokemonExt?[TEAM_SIZE];
            for (int i = 0; i < pkmns.Length; i++)
            {
                Pokemon pkmn = new Pokemon(teamData, TEAM_PKMN_SIZE * i, TEAM_PKMN_SIZE, true);
                pkmns[i] = new PokemonExt(pkmn, IsSelected(pkmn));
            }

            Utils.UpdateCollectionRange(Team, pkmns);
        }
        bool IsSelected(Pokemon pkmn)
        {
            if (Selection == null) return false;
            if (pkmn.DataLocation.Intersect(Selection)) return true;
            return false;
        }
        public MMFSync Sync { get => sync; }
        public byte[]? CopiedData
        {
            get => copiedData;
            set { copiedData = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopiedData))); }
        }

        RangeObservableCollection<PokemonExt?> currentBox;
        RangeObservableCollection<PokemonExt?> team;
        int currentBoxNumber;
        DataLocation? selection;
        public int CurrentBoxNumber {
            get => currentBoxNumber + 1;
            set { currentBoxNumber = value - 1; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoxNumber))); }
        }
        public RangeObservableCollection<PokemonExt?> Team { get => team; }
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
                for (int i = 0; i < Team.Count; i++)
                {
                    if (Team[i] == null) continue;
                    PokemonExt pkmn = Team[i]!;
                    bool selected = IsSelected(pkmn.pkmn);
                    if (selected != pkmn.selected)
                        Team[i] = new PokemonExt(pkmn.pkmn, selected);
                }
            }
        }

        List<IEditorWindow> openedEditors = new List<IEditorWindow>();
        public void OpenInterpretedEditor(DataLocation dl)
        {
            ShowWindow(new InterpretedEditor(dl.inTeam ? teamData : data, dl.offset, dl.size, dl.inTeam));
        }
        public void OpenDataEditor(DataLocation dl)
        {
            if (dl.size == TEAM_PKMN_SIZE)
                ShowWindow(new PokemonViewWindow100(dl.inTeam ? teamData : data, dl.offset, dl.inTeam, this));
            else
                ShowWindow(new PokemonViewWindow(dl.inTeam ? teamData : data, dl.offset, dl.inTeam, this));
        }
        public void OpenRawEditor(DataLocation dl)
        {
            ShowWindow(new HexEditor(dl.inTeam ? teamData : data, dl.offset, dl.size, dl.inTeam));
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
                    Utils.UpdateCollectionRange(teamData, CurrentSave.RetrieveTeamData().pokemonList);
                    initialData = data.ToArray();
                    initialTeamData = teamData.ToArray();
                } catch {
                    await MessageBoxManager.GetMessageBoxStandardWindow("Error", "An error occured while loading the save.").ShowDialog(mw);
                }
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
                    TeamItemsData tid = CurrentSave.RetrieveTeamData();
                    tid.pokemonList = teamData.ToArray();
                    CurrentSave.SetTeamData(tid);
                    CurrentSave.SaveToFile();
                    initialData = pcd.pokemonList;
                    initialTeamData = tid.pokemonList;
                }
                catch {
                    MessageBoxManager.GetMessageBoxStandardWindow("Error", "An error occured while saving.").ShowDialog(mw);
                }
            }
        }

        public void RestoreInitialData(DataLocation dl)
        {
            ArraySegment<byte> initial = new ArraySegment<byte>(dl.inTeam ? initialTeamData : initialData, dl.offset, dl.size);
            Utils.UpdateCollectionRange(dl.inTeam ? teamData : data, initial, dl.offset);
        }
        public void Delete(DataLocation dl)
        {
            Utils.UpdateCollectionRange(dl.inTeam ? teamData : data, new byte[dl.size], dl.offset);
        }
        public void Copy(DataLocation dl)
        {
            CopiedData = Utils.ExtractCollectionRange(dl.inTeam ? teamData : data, dl.offset, dl.size);
        }
        public void Paste(DataLocation dl)
        {
            if (CopiedData != null)
                Utils.UpdateCollectionRange(dl.inTeam ? teamData : data, new ArraySegment<byte>(CopiedData, 0, Math.Min(dl.size, CopiedData.Length)), dl.offset);
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

        public void StartSync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Sorry, synchronization with Bizhawk is only supported on Windows.").ShowDialog(mw);
            else if (!sync.Start())
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Please run the synchronization LUA script in Bizhawk first.").ShowDialog(mw);
        }
        public void StopSync() { sync.Stop(); }
    }
}
