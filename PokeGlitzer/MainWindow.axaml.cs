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
using System.Xml.Linq;
using System.IO;

namespace PokeGlitzer
{
    // TODO: Improve ribbons field.
    // TODO: Add item sprites for "unholdable" objects.
    // TODO: Nickname field: button to set to default (when available)
    // TODO: Allow to view and edit other save data (https://github.com/pret/pokeemerald/blob/master/src/save.c : gRamSaveSectorLocations)

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
        RangeObservableCollection<byte> boxNamesData;
        public const int BOX_SIZE = 30;
        public const int BOX_NUMBER = 14;
        public const int TEAM_SIZE = 6;
        Save? save = null;
        MMFSync sync;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MainWindowViewModel(MainWindow mw, string[]? args)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.mw = mw;
            data = Utils.CollectionOfSize<byte>(Pokemon.PC_SIZE * BOX_SIZE * BOX_NUMBER);
            teamData = Utils.CollectionOfSize<byte>(Pokemon.TEAM_SIZE * TEAM_SIZE);
            boxNamesData = Utils.CollectionOfSize<byte>(BoxNames.BOX_NAME_BYTE_SIZE * BOX_NUMBER);
            initialData = new byte[data.Count];
            initialTeamData = new byte[teamData.Count];
            currentBox = Utils.CollectionOfSize<PokemonExt?>(BOX_SIZE);
            boxNames = new BoxNames(boxNamesData);
            LoadBox(0);
            team = Utils.CollectionOfSize<PokemonExt?>(TEAM_SIZE);
            LoadTeam();
            sync = new MMFSync(data, teamData, boxNamesData);
            BoxNames.Names.CollectionChanged += (_, _) => SetNormalizedCurrentBoxName(BoxNames.Names[CurrentBoxNumber-1]); 

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
                Pokemon pkmn = new Pokemon(data, new DataLocation(Pokemon.PC_SIZE * i + offset, Pokemon.PC_SIZE, Source.PC));
                pkmns[i] = new PokemonExt(pkmn, IsSelected(pkmn));
            }
                
            Utils.UpdateCollectionRange(CurrentBox, pkmns);
            CurrentBoxNumber = nb+1;
            SetNormalizedCurrentBoxName(BoxNames.Names[nb]);
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
                Pokemon pkmn = new Pokemon(teamData, new DataLocation(Pokemon.TEAM_SIZE * i, Pokemon.TEAM_SIZE, Source.Team));
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

        BoxNames boxNames;
        public BoxNames BoxNames { get => boxNames; }
        public string CurrentBoxName
        {
            get => BoxNames.MakeNameLookBetter(BoxNames.Names[CurrentBoxNumber - 1]);
            set
            {
                try
                {
                    string str = BoxNames.NormalizeName(value);
                    if (!BoxNames.IsValidName(str)) throw new Exception();
                    BoxNames.Names[CurrentBoxNumber - 1] = str;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoxName)));
                }
                catch
                {
                    throw new Avalonia.Data.DataValidationException(null);
                }
            }
        }
        void SetNormalizedCurrentBoxName(string v)
        {
            BoxNames.Names[CurrentBoxNumber - 1] = v;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoxName)));
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
            ShowWindow(new InterpretedEditor(dl.src == Source.Team ? teamData : data, dl), parent);
        }
        public void OpenDataEditor(DataLocation dl) { OpenDataEditor(dl, null); }
        public void OpenDataEditor(DataLocation dl, Window? parent)
        {
            if (dl.size == Pokemon.TEAM_SIZE)
                ShowWindow(new PokemonViewWindow100(dl.src == Source.Team ? teamData : data, dl.offset, dl.src, this), parent);
            else
                ShowWindow(new PokemonViewWindow(dl.src == Source.Team ? teamData : data, dl.offset, dl.src, this), parent);
        }
        public void OpenRawEditor(DataLocation dl) { OpenRawEditor(dl, null); }
        public void OpenRawEditor(DataLocation dl, Window? parent)
        {
            ShowWindow(new HexEditor(dl.src == Source.Team ? teamData : data, dl), parent);
        }
        public void EditFullBoxes()
        {
            ShowWindow(new HexEditor(data, Source.PC), null);
        }
        public void EditFullParty()
        {
            ShowWindow(new HexEditor(teamData, Source.Team), null);
        }
        public void EditFullBoxNames()
        {
            ShowWindow(new HexEditor(boxNamesData, Source.BoxNames), null);
        }
        public void ShowWindow(IEditorWindow w, Window? parent)
        {
            openedEditors.Add(w);
            w.Closed += (_, _) => { Selection = null; openedEditors.Remove(w); GC.Collect(); };
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
                timer.Interval = TimeSpan.FromMilliseconds(500);
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
        public void openGP(int offset, GlitzerWindowViewModel.OffsetType t)
        {
            GlitzerWindow gw = new GlitzerWindow(this, data, offset, t);
            openedGPs.Add(gw);
            GlitzerWindowViewModel gwvm = (GlitzerWindowViewModel)gw.DataContext!;
            gw.Closed += (_, _) => { Selection = null; openedGPs.Remove(gw); GC.Collect(); };
            gw.Activated += (_, _) => { Selection = gwvm.DataLocation; };
            gw.Deactivated += (_, _) => { Selection = null; };
            gwvm.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(GlitzerWindowViewModel.DataLocation))
                    Selection = gwvm.DataLocation;
            };
            gw.Show(mw);
        }
        public void OpenGP() { openGP(0, GlitzerWindowViewModel.OffsetType.ASLR); }
        public void OpenGPBefore(DataLocation dl) { openGP(dl.offset + dl.size, GlitzerWindowViewModel.OffsetType.End); }
        public void OpenGPAfter(DataLocation dl) { openGP(dl.offset, GlitzerWindowViewModel.OffsetType.Start); }

        public async void ImportPk3Ek3(DataLocation dl) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Pk3 file", Extensions = { "pk3" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Ek3 file", Extensions = { "ek3" } });
            dialog.AllowMultiple = false;

            string[] result = await dialog.ShowAsync(mw);
            if (result != null && result.Length >= 1) {
                try {
                    string path = result[0];
                    byte[] fileData = File.ReadAllBytes(path);
                    RangeObservableCollection<byte> dest = dl.src == Source.Team ? teamData : data;
                    ArraySegment<byte> src = new ArraySegment<byte>(fileData, 0, Math.Min(dl.size, fileData.Length));
                    if (Path.GetExtension(path).ToLowerInvariant() == ".ek3") {
                        Utils.UpdateCollectionRange(dest, src, dl.offset);
                    }
                    else {
                        Pokemon p = new Pokemon(dest, dl);
                        Utils.UpdateCollectionRange(p.View.Pk3Data, src);
                    }
                }
                catch {
                    MessageBoxManager.GetMessageBoxStandardWindow("Error", "An error occured while importing the file.").ShowDialog(mw);
                }
            }
        }
        public async void ExportPk3Ek3(DataLocation dl) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Pk3 file", Extensions = { "pk3" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Ek3 file", Extensions = { "ek3" } });

            string result = await dialog.ShowAsync(mw);
            if (result != null) {
                try {
                    RangeObservableCollection<byte> src = dl.src == Source.Team ? teamData : data;
                    byte[] srcData;
                    if (Path.GetExtension(result).ToLowerInvariant() == ".ek3") {
                        srcData = Utils.ExtractCollectionRange(src, dl.offset, dl.size);
                    }
                    else {
                        Pokemon p = new Pokemon(src, dl);
                        srcData = Utils.ExtractCollectionRange(p.View.Pk3Data, 0, dl.size);
                    }
                    File.WriteAllBytes(result, srcData);
                }
                catch {
                    MessageBoxManager.GetMessageBoxStandardWindow("Error", "An error occured while exporting the file.").ShowDialog(mw);
                }
            }
        }

        public void Exit()
        {
            mw.Close();
        }
        async void LoadSave(string path)
        {
            try
            {
                CurrentSave = new Save(path);
                PCData pcd = CurrentSave.RetrievePCData();
                Utils.UpdateCollectionRange(data, pcd.pokemonList);
                Utils.UpdateCollectionRange(boxNamesData, pcd.boxNames);
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
            dialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { "*" } });
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
                    pcd.boxNames = boxNamesData.ToArray();
                    CurrentSave.SetPCData(pcd);
                    TeamItemsData tid = CurrentSave.RetrieveTeamData();
                    tid.pokemonList = teamData.ToArray();
                    tid.teamSize = CalculatePlayerPartyCount();
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
        uint CalculatePlayerPartyCount()
        {
            uint i = 0;
            while (i < Team.Count && Team[(int)i] != null && Team[(int)i]!.pkmn.View.Interpreted.species != 0) i++;
            return i;
        }
        public async void SaveAs()
        {
            if (CurrentSave != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filters.Add(new FileDialogFilter() { Name = "Save file", Extensions = { "sav" } });
                dialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { "*" } });

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
            Converters.PokemonToStringConverter pts = new Converters.PokemonToStringConverter();
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
                    EggType.NotAnEgg => "not an egg",
                    EggType.BadEgg => "bad egg",
                    _ => throw new NotImplementedException(),
                };
                string species = (string)pts.Convert(e.species, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
                str.AppendLine($"Species {/*e.species:X*/species} ({egg}) by {type} corruption:");
                foreach (GlitzerSimulation.OffsetASLR oa in o)
                    str.AppendLine($"    By corrupting from 0x{oa.startOffset:X} to 0x{oa.endOffset:X} (ASLR: +{oa.aslr})");
                str.AppendLine($"    Total probability: {o.Count}/{res.nbTries}");
                str.AppendLine();
            }
            log.Text = str.ToString();
        }

        public void RestoreInitialData(DataLocation dl)
        {
            ArraySegment<byte> initial = new ArraySegment<byte>(dl.src == Source.Team ? initialTeamData : initialData, dl.offset, dl.size);
            Utils.UpdateCollectionRange(dl.src == Source.Team ? teamData : data, initial, dl.offset);
        }
        public void Delete(DataLocation dl)
        {
            Utils.UpdateCollectionRange(dl.src == Source.Team ? teamData : data, new byte[dl.size], dl.offset);
        }
        public void ClearCurrentBox()
        {
            foreach (PokemonExt? p in CurrentBox)
                if (p != null) Delete(p!.pkmn.DataLocation);
        }
        public void FlagBadEggs()
        {
            foreach (PokemonExt? p in CurrentBox)
                if (p != null) p.pkmn.FlagAsBaddEggIfInvalid();
        }
        public void RemoveBadEggs()
        {
            foreach (PokemonExt? p in CurrentBox)
                if (p != null && p.pkmn.View.Interpreted.egg == EggType.BadEgg)
                    Delete(p!.pkmn.DataLocation);
        }
        public void FixChecksums()
        {
            foreach (PokemonExt? p in CurrentBox)
                if (p != null) p!.pkmn.FixChecksum();
        }
        void ChangeEggTypes(EggType from, EggType to)
        {
            foreach (PokemonExt? p in CurrentBox)
                if (p != null && p.pkmn.View.Interpreted.egg == from)
                    p!.pkmn.View.Interpreted = p!.pkmn.View.Interpreted with { egg = to };

        }
        public void InconsistentToBadEgg()
        {
            ChangeEggTypes(EggType.Invalid, EggType.BadEgg);
        }
        public void InconsistentToEgg()
        {
            ChangeEggTypes(EggType.Invalid, EggType.Egg);
        }
        public void InconsistentToHatched()
        {
            ChangeEggTypes(EggType.Invalid, EggType.NotAnEgg);
        }
        public void BadEggToEgg()
        {
            ChangeEggTypes(EggType.BadEgg, EggType.Egg);
        }
        public void BadEggToHatched()
        {
            ChangeEggTypes(EggType.BadEgg, EggType.NotAnEgg);
        }
        public void EggToHatched()
        {
            ChangeEggTypes(EggType.Egg, EggType.NotAnEgg);
        }
        public void Copy(DataLocation dl)
        {
            CopiedData = Utils.ExtractCollectionRange(dl.src == Source.Team ? teamData : data, dl.offset, dl.size);
        }
        public void Cut(DataLocation dl)
        {
            Copy(dl); Delete(dl);
        }
        public void Paste(DataLocation dl)
        {
            if (CopiedData != null)
                Utils.UpdateCollectionRange(dl.src == Source.Team ? teamData : data,
                    new ArraySegment<byte>(CopiedData, 0, Math.Min(dl.size, CopiedData.Length)), dl.offset);
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

        public bool EnglishLang
        {
            get => Settings.Text_lang == Settings.Lang.ENG && Settings.Game_lang == Settings.GameLang.ENG;
            set { SetLang(Settings.GameLang.ENG, Settings.Lang.ENG); }
        }
        public void SetEnglishLang() { EnglishLang = true; }
        public bool JapaneseLang
        {
            get => Settings.Text_lang == Settings.Lang.JAP && Settings.Game_lang == Settings.GameLang.JAP;
            set { SetLang(Settings.GameLang.JAP, Settings.Lang.JAP); }
        }
        public void SetJapaneseLang() { JapaneseLang = true; }
        public bool FrenchLang
        {
            get => Settings.Text_lang == Settings.Lang.FRA && Settings.Game_lang == Settings.GameLang.FRA;
            set { SetLang(Settings.GameLang.FRA, Settings.Lang.FRA); }
        }
        public void SetFrenchLang() { FrenchLang = true; }
        public bool GermanLang
        {
            get => Settings.Text_lang == Settings.Lang.GER && Settings.Game_lang == Settings.GameLang.GER;
            set { SetLang(Settings.GameLang.GER, Settings.Lang.GER); }
        }
        public void SetGermanLang() { GermanLang = true; }
        public bool ItalianLang
        {
            get => Settings.Text_lang == Settings.Lang.ITA && Settings.Game_lang == Settings.GameLang.ITA;
            set { SetLang(Settings.GameLang.ITA, Settings.Lang.ITA); }
        }
        public void SetItalianLang() { ItalianLang = true; }
        public bool SpanishLang
        {
            get => Settings.Text_lang == Settings.Lang.SPA && Settings.Game_lang == Settings.GameLang.SPA;
            set { SetLang(Settings.GameLang.SPA, Settings.Lang.SPA); }
        }
        public void SetSpanishLang() { SpanishLang = true; }
        void SetLang (Settings.GameLang gl, Settings.Lang l)
        {
            Settings.SetLang(gl, l);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnglishLang)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JapaneseLang)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrenchLang)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GermanLang)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItalianLang)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpanishLang)));
            boxNames.Refresh();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoxName)));
        }

        public bool EmeraldGame
        {
            get => Settings.Game_version == Settings.GameVersion.Emerald;
            set { SetVersion(Settings.GameVersion.Emerald); }
        }
        public void SetEmeraldGame() { EmeraldGame = true; }
        public bool FireRedGame
        {
            get => Settings.Game_version == Settings.GameVersion.FireRed;
            set { SetVersion(Settings.GameVersion.FireRed); }
        }
        public void SetFireRedGame() { FireRedGame = true; }
        public bool LeafGreenGame
        {
            get => Settings.Game_version == Settings.GameVersion.LeafGreen;
            set { SetVersion(Settings.GameVersion.LeafGreen); }
        }
        public void SetLeafGreenGame() { LeafGreenGame = true; }
        public bool RubyGame
        {
            get => Settings.Game_version == Settings.GameVersion.Ruby;
            set { SetVersion(Settings.GameVersion.Ruby); }
        }
        public void SetRubyGame() { RubyGame = true; }
        public bool SapphireGame
        {
            get => Settings.Game_version == Settings.GameVersion.Sapphire;
            set { SetVersion(Settings.GameVersion.Sapphire); }
        }
        public void SetSapphireGame() { SapphireGame = true; }
        void SetVersion(Settings.GameVersion v)
        {
            Settings.SetVersion(v);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmeraldGame)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FireRedGame)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeafGreenGame)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RubyGame)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SapphireGame)));
        }
    }
}
