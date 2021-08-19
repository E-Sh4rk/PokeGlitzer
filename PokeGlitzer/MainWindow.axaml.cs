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
    // TODO: Possibility to edit the box names
    // TODO: Add more interpreted data

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, null);
        }
        public MainWindow(string[] args)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, args);

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
        byte[] initialTeamData;
        byte[]? copiedData = null;
        RangeObservableCollection<byte> data;
        RangeObservableCollection<byte> teamData;
        public const int BOX_SIZE = 30;
        public const int BOX_NUMBER = 14;
        public const int TEAM_SIZE = 6;
        Save? save = null;
        MMFSync sync;
        public MainWindowViewModel(MainWindow mw, string[]? args)
        {
            this.mw = mw;
            data = Utils.CollectionOfSize<byte>(Pokemon.PC_SIZE * BOX_SIZE * BOX_NUMBER);
            teamData = Utils.CollectionOfSize<byte>(Pokemon.TEAM_SIZE * TEAM_SIZE);
            initialData = new byte[data.Count];
            initialTeamData = new byte[teamData.Count];
            currentBox = Utils.CollectionOfSize<PokemonExt?>(BOX_SIZE);
            LoadBox(0);
            team = Utils.CollectionOfSize<PokemonExt?>(TEAM_SIZE);
            LoadTeam();
            sync = new MMFSync(data, teamData);

            if (args != null && args.Length == 1)
                LoadSave(args[0]);
        }

        void LoadBox(int nb)
        {
            int offset = Pokemon.PC_SIZE * BOX_SIZE * nb;
            for (int i = 0; i < CurrentBox.Count; i++)
            {
                if (CurrentBox[i] != null)
                    CurrentBox[i]!.pkmn.Dispose();
            }
            PokemonExt?[] pkmns = new PokemonExt?[BOX_SIZE];
            for (int i = 0; i < pkmns.Length; i++)
            {
                Pokemon pkmn = new Pokemon(data, new DataLocation(Pokemon.PC_SIZE * i + offset, Pokemon.PC_SIZE, false));
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
                Pokemon pkmn = new Pokemon(teamData, new DataLocation(Pokemon.TEAM_SIZE * i, Pokemon.TEAM_SIZE, true));
                pkmns[i] = new PokemonExt(pkmn, IsSelected(pkmn));
            }

            Utils.UpdateCollectionRange(Team, pkmns);
        }
        public bool IsSelected(Pokemon pkmn)
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
                foreach (GlitzerWindow gw in openedGPs)
                {
                    ((GlitzerWindowViewModel)gw.DataContext!).UpdateSelection();
                }
            }
        }

        List<IEditorWindow> openedEditors = new List<IEditorWindow>();
        public void OpenInterpretedEditor(DataLocation dl) { OpenInterpretedEditor(dl, null); }
        public void OpenInterpretedEditor(DataLocation dl, Window? parent)
        {
            ShowWindow(new InterpretedEditor(dl.inTeam ? teamData : data, dl), parent);
        }
        public void OpenDataEditor(DataLocation dl) { OpenDataEditor(dl, null); }
        public void OpenDataEditor(DataLocation dl, Window? parent)
        {
            if (dl.size == Pokemon.TEAM_SIZE)
                ShowWindow(new PokemonViewWindow100(dl.inTeam ? teamData : data, dl.offset, dl.inTeam, this), parent);
            else
                ShowWindow(new PokemonViewWindow(dl.inTeam ? teamData : data, dl.offset, dl.inTeam, this), parent);
        }
        public void OpenRawEditor(DataLocation dl) { OpenRawEditor(dl, null); }
        public void OpenRawEditor(DataLocation dl, Window? parent)
        {
            ShowWindow(new HexEditor(dl.inTeam ? teamData : data, dl), parent);
        }
        public void EditFullBoxes()
        {
            ShowWindow(new HexEditor(data, false), null);
        }
        public void EditFullParty()
        {
            ShowWindow(new HexEditor(teamData, true), null);
        }
        public void ShowWindow(IEditorWindow w, Window? parent)
        {
            openedEditors.Add(w);
            w.Closed += (_, _) => { openedEditors.Remove(w); GC.Collect(); };
            w.Activated += (_, _) => { Selection = w.DataLocation; };
            w.Deactivated += (_, _) => { Selection = null; };
            w.Show(parent == null ? mw : parent);
        }
        static readonly ISolidColorBrush HIGHLIGHT_BRUSH = new SolidColorBrush(Colors.Yellow, 150);
        void HighlightWindow(Window w)
        {
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
        public void SelectSlot(DataLocation dl) { SelectSlot(dl, null); }
        public void SelectSlot(DataLocation dl, Window? parent)
        {
            bool openNew = true;
            foreach (IEditorWindow w in openedEditors)
            {
                if (w.DataLocation.Intersect(dl))
                {
                    openNew = false;
                    HighlightWindow((Window)w);
                }
            }
            foreach (GlitzerWindow gw in openedGPs)
            {
                if (gw == parent) continue;
                if (((GlitzerWindowViewModel)gw.DataContext!).DataLocation.Intersect(dl))
                {
                    openNew = false;
                    HighlightWindow(gw);
                }
            }
            if (openNew)
                OpenInterpretedEditor(dl, parent);
        }

        List<GlitzerWindow> openedGPs = new List<GlitzerWindow>();
        public void openGP(int offset)
        {
            GlitzerWindow gw = new GlitzerWindow(this, data, offset);
            openedGPs.Add(gw);
            GlitzerWindowViewModel gwvm = (GlitzerWindowViewModel)gw.DataContext!;
            gw.Closed += (_, _) => { openedGPs.Remove(gw); GC.Collect(); };
            gw.Activated += (_, _) => { Selection = gwvm.DataLocation; };
            gw.Deactivated += (_, _) => { Selection = null; };
            gwvm.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(GlitzerWindowViewModel.DataLocation))
                    Selection = gwvm.DataLocation;
            };
            gw.Show(mw);
        }
        public void OpenGP() { openGP(0); }
        public void OpenGPBefore(DataLocation dl) { openGP(dl.offset + dl.size - GlitzerWindowViewModel.SIZE); }
        public void OpenGPAfter(DataLocation dl) { openGP(dl.offset); }

        public void FlagBadEggs()
        {
            foreach (PokemonExt? p in CurrentBox)
                if (p != null) p.pkmn.FlagAsBaddEggIfInvalid();
            foreach (PokemonExt? p in Team)
                if (p != null) p.pkmn.FlagAsBaddEggIfInvalid();
        }

        public void Exit()
        {
            mw.Close();
        }
        void LoadSave(string path)
        {
            try
            {
                CurrentSave = new Save(path);
                Utils.UpdateCollectionRange(data, CurrentSave.RetrievePCData().pokemonList);
                Utils.UpdateCollectionRange(teamData, CurrentSave.RetrieveTeamData().pokemonList);
                initialData = data.ToArray();
                initialTeamData = teamData.ToArray();
            }
            catch
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Unable to load this save.").ShowDialog(mw);
            }
        }
        public async void Open()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Save file", Extensions = { "sav" } });
            dialog.AllowMultiple = false;

            string[] result = await dialog.ShowAsync(mw);
            if (result != null && result.Length >= 1)
                LoadSave(result[0]);
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
        public async void SaveAs()
        {
            if (CurrentSave != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filters.Add(new FileDialogFilter() { Name = "Save file", Extensions = { "sav" } });

                string result = await dialog.ShowAsync(mw);
                if (result != null)
                {
                    CurrentSave.SetSavePath(result);
                    Save();
                }
            }
        }

        public void RunGPSimulation()
        {
            LogWindow log = new LogWindow();
            log.Show(mw);
            GlitzerSimulation sim = new GlitzerSimulation(data);
            GlitzerSimulation.SimulationResult res = sim.Simulate();
            StringBuilder str = new StringBuilder();
            str.AppendLine("Below are the species which can be obtained by corruption.");
            str.AppendLine("Pokemons with an invalid checksum are not shown.");
            str.AppendLine();
            List<GlitzerSimulation.SimulationEntry> entries = res.obtained.Keys.ToList();
            entries.Sort(GlitzerSimulation.CompareEntries);
            foreach (GlitzerSimulation.SimulationEntry e in entries)
            {
                List<GlitzerSimulation.OffsetASLR> o = res.obtained[e];
                string type = e.type switch
                {
                    GlitzerSimulation.CorruptionType.PID => "PID",
                    GlitzerSimulation.CorruptionType.TID => "TID",
                    GlitzerSimulation.CorruptionType.Other => "Misc.",
                    GlitzerSimulation.CorruptionType.None => "None",
                    _ => throw new NotImplementedException(),
                };
                string egg = e.egg switch
                {
                    EggType.Egg => "egg",
                    EggType.Invalid => "inconsistent",
                    EggType.None => "empty slot",
                    EggType.NotAnEgg => "not an egg",
                    EggType.BadEgg => "bad egg",
                    _ => throw new NotImplementedException(),
                };
                str.AppendLine($"Species 0x{e.species:X} ({egg}) by {type} corruption:");
                foreach (GlitzerSimulation.OffsetASLR oa in o)
                    str.AppendLine($"    By corrupting from 0x{oa.startOffset:X} to 0x{oa.endOffset:X} (ASLR: +{oa.aslr})");
                str.AppendLine($"    Total probability: {o.Count}/{res.nbTries}");
                str.AppendLine();
            }
            log.Text = str.ToString();
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
        public void Cut(DataLocation dl)
        {
            Copy(dl); Delete(dl);
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
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Synchronization with Bizhawk is only supported on Windows.").ShowDialog(mw);
            else if (!sync.Start())
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Please run the synchronization LUA script in Bizhawk first.").ShowDialog(mw);
        }
        public void StopSync() { sync.Stop(); }
    }
}
