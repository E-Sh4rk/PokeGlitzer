using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PokeGlitzer
{
    public partial class InterpretedEditor : Window, IEditorWindow
    {
        Pokemon pkmn;
        public InterpretedEditor()
        {
            InitializeComponent();
            pkmn = new Pokemon(Utils.CollectionOfSize<byte>(Pokemon.PC_SIZE), DataLocation.DefaultPC);
            DataContext = new InterpretedEditorModel(pkmn, this);
        }

        public InterpretedEditor(RangeObservableCollection<byte> data, DataLocation dl)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, dl);
            DataContext = new InterpretedEditorModel(pkmn, this);
        }
        public DataLocation DataLocation => pkmn.DataLocation;

        protected override void OnClosed(EventArgs e)
        {
            pkmn.Dispose();
            base.OnClosed(e);
        }
    }
    public class InterpretedEditorModel : INotifyPropertyChanged
    {
        PokemonView view;
        Window parent;

        public InterpretedEditorModel(Pokemon pkmn, Window parent)
        {
            this.parent = parent;
            view = pkmn.View;
            //view.PropertyChanged += ViewInterpretedChanged;
            Avalonia.Utilities.WeakEventHandlerManager.Subscribe<PokemonView, PropertyChangedEventArgs, InterpretedEditorModel>(view,
                nameof(PokemonView.PropertyChanged), ViewInterpretedChanged);
            RefreshControls();
        }

        void ViewInterpretedChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(PokemonView.Interpreted)) return;
            RefreshControls();
        }

        Converters.BallToStringConverter bts = new Converters.BallToStringConverter();
        Converters.GameToStringConverter gats = new Converters.GameToStringConverter();
        Converters.GenderToStringConverter gets = new Converters.GenderToStringConverter();
        Converters.LanguageToStringConverter lats = new Converters.LanguageToStringConverter();
        Converters.PokemonToStringConverter pts = new Converters.PokemonToStringConverter();
        Converters.LocationToStringConverter lots = new Converters.LocationToStringConverter();
        Converters.ItemToStringConverter its = new Converters.ItemToStringConverter();
        Converters.MoveToStringConverter mts = new Converters.MoveToStringConverter();
        public void RefreshControls()
        {
            InterpretedData d = view.Interpreted;
            pid = d.PID;
            otid = d.OTID;
            species = (string)pts.Convert(d.species, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            UpdatePidFields();
            HasSpecies = d.hasSpecies;
            Egg = d.egg;

            Language = (string)lats.Convert(d.identity.lang, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            SetNormalizedNickname(d.identity.nickname); SetNormalizedOTName(d.identity.otName);
            MetLocation = (string)lots.Convert(d.identity.metLocation, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            LevelMet = d.identity.levelMet;
            GameOfOrigin = (string)gats.Convert(d.identity.gameOfOrigin, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            Ball = (string)bts.Convert(d.identity.ball, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            OTGender = (string)gets.Convert(d.identity.otGender, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);

            Move1 = (string)mts.Convert(d.moves.m1, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            Move2 = (string)mts.Convert(d.moves.m2, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture); ;
            Move3 = (string)mts.Convert(d.moves.m3, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture); ;
            Move4 = (string)mts.Convert(d.moves.m4, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture); ;
            PP1 = d.moves.pp1; PP2 = d.moves.pp2; PP3 = d.moves.pp3; PP4 = d.moves.pp4;
            PPb1 = d.moves.ppb1; PPb2 = d.moves.ppb2; PPb3 = d.moves.ppb3; PPb4 = d.moves.ppb4;

            HpEV = d.EVs.hp; AtkEV = d.EVs.atk; DefEV = d.EVs.def;
            SpeedEV = d.EVs.speed; SpeAtkEV = d.EVs.spe_atk; SpeDefEV = d.EVs.spe_def;

            HpIV = d.IVs.hp; AtkIV = d.IVs.atk; DefIV = d.IVs.def;
            SpeedIV = d.IVs.speed; SpeAtkIV = d.IVs.spe_atk; SpeDefIV = d.IVs.spe_def;

            Coolness = d.condition.coolness; Beauty = d.condition.beauty; Cuteness = d.condition.cuteness;
            Smartness = d.condition.smartness; Toughness = d.condition.toughness; Feel = d.condition.feel;

            Item = (string)its.Convert(d.battle.item, typeof(String), "x", System.Globalization.CultureInfo.CurrentCulture);
            Ability = d.battle.ability ? (byte)1 : (byte)0; Experience = d.battle.experience; Friendship = d.battle.friendship;

            PokerusDays = d.misc.pokerus_days; PokerusStrain = d.misc.pokerus_strain; Ribbons = d.misc.ribbons; Obedient = d.misc.obedient;
        }
        public void Save()
        {
            object language = lats.ConvertBack(Language, typeof(byte), "x", System.Globalization.CultureInfo.CurrentCulture);
            object location = lots.ConvertBack(MetLocation, typeof(byte), "x", System.Globalization.CultureInfo.CurrentCulture);
            object gender = gets.ConvertBack(OTGender, typeof(byte), "x", System.Globalization.CultureInfo.CurrentCulture);
            object game = gats.ConvertBack(GameOfOrigin, typeof(byte), "x", System.Globalization.CultureInfo.CurrentCulture);
            object ball = bts.ConvertBack(Ball, typeof(byte), "x", System.Globalization.CultureInfo.CurrentCulture);
            Identity id = new Identity(language is byte ? (byte)language : (byte)0, nickname, gender is byte ? (byte)gender : (byte)0, otName,
                location is byte ? (byte)location : (byte)0, LevelMet, game is byte ? (byte)game : (byte)0, ball is byte ? (byte)ball : (byte)0);
            object m1 = mts.ConvertBack(Move1, typeof(ushort), "x", System.Globalization.CultureInfo.CurrentCulture);
            object m2 = mts.ConvertBack(Move2, typeof(ushort), "x", System.Globalization.CultureInfo.CurrentCulture);
            object m3 = mts.ConvertBack(Move3, typeof(ushort), "x", System.Globalization.CultureInfo.CurrentCulture);
            object m4 = mts.ConvertBack(Move4, typeof(ushort), "x", System.Globalization.CultureInfo.CurrentCulture);
            Moves m = new Moves(m1 is ushort ? (ushort)m1 : (ushort)0, PP1, PPb1, m2 is ushort ? (ushort)m2 : (ushort)0, PP2, PPb2,
                m3 is ushort ? (ushort)m3 : (ushort)0, PP3, PPb3, m4 is ushort ? (ushort)m4 : (ushort)0, PP4, PPb4);
            EVsIVs evs = new EVsIVs(HpEV, AtkEV, DefEV, SpeedEV, SpeAtkEV, SpeDefEV);
            EVsIVs ivs = new EVsIVs(HpIV, AtkIV, DefIV, SpeedIV, SpeAtkIV, SpeDefIV);
            Condition c = new Condition(Coolness, Beauty, Cuteness, Smartness, Toughness, Feel);
            object item = its.ConvertBack(Item, typeof(ushort), "x", System.Globalization.CultureInfo.CurrentCulture);
            Battle b = new Battle(item is ushort ? (ushort)item : (ushort)0, Ability != 0, Experience, Friendship);
            Misc misc = new Misc(PokerusDays, PokerusStrain, Ribbons, Obedient);
            InterpretedData d = new InterpretedData(PID, OTID, SpeciesU16(), HasSpecies, Egg, id, b, m, evs, ivs, c, misc);
            view.Interpreted = d;
        }
        public void SaveAndClose()
        {
            Save(); parent.Close();
        }

        // ========== General ==========
        uint pid;
        public uint PID
        {
            get => pid;
            set { pid = value; UpdatePidFields(); }
        }
        uint otid;
        public uint OTID
        {
            get => otid;
            set { otid = value; UpdatePidFields(); }
        }
        string species;
        public string Species
        {
            get => species;
            set { species = value; UpdatePidFields(); UpdateLvlRemExpFields(); }
        }
        public ushort SpeciesU16()
        {
            object species = pts.ConvertBack(Species, typeof(ushort), "x", System.Globalization.CultureInfo.CurrentCulture);
            return species is ushort ? (ushort)species : (ushort)0;
        }
        bool hasSpecies;
        public bool HasSpecies
        {
            get => hasSpecies;
            set { hasSpecies = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSpecies))); }
        }
        EggType egg;
        public EggType Egg
        {
            get => egg;
            set { egg = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Egg))); }
        }
        // ========== Identity ==========
        string language;
        Language GetInterpretedLanguage() {
            object l = lats.ConvertBack(language, typeof(byte), "x", System.Globalization.CultureInfo.CurrentCulture);
            byte lb = l is byte ? (byte)l : (byte)0;
            return Identity.LanguageFromByte(lb);
        }
        public string Language
        {
            get => language;
            set
            {
                language = value;
                byte[] data = view.DecodedData.ToArray();
                bool useJP = Pokemon.UseJP(GetInterpretedLanguage());
                SetNormalizedNickname(StringConverter.GetString3(data,
                    PokemonStruct.NICKNAME_OFFSET, PokemonStruct.NICKNAME_LEN, useJP));
                SetNormalizedOTName(StringConverter.GetString3(data,
                    PokemonStruct.OTNAME_OFFSET, PokemonStruct.OTNAME_LEN, useJP));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            }
        }
        string nickname;
        public string Nickname
        {
            get => BoxNames.MakeNameLookBetter(nickname, GetInterpretedLanguage());
            set
            {
                string v = BoxNames.NormalizeName(value, GetInterpretedLanguage());
                if (BoxNames.IsValidName(v, PokemonStruct.NICKNAME_LEN, GetInterpretedLanguage()))
                {
                    nickname = v;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nickname)));
                }
                else throw new Avalonia.Data.DataValidationException("Invalid nickname.");
            }
        }
        void SetNormalizedNickname(string v)
        {
            nickname = v;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nickname)));
        }
        string otGender;
        public string OTGender
        {
            get => otGender;
            set { otGender = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OTGender))); }
        }
        string otName;
        public string OTName
        {
            get => BoxNames.MakeNameLookBetter(otName, GetInterpretedLanguage());
            set
            {
                string v = BoxNames.NormalizeName(value, GetInterpretedLanguage());
                if (BoxNames.IsValidName(v, PokemonStruct.OTNAME_LEN, GetInterpretedLanguage()))
                {
                    otName = v;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OTName)));
                }
                else throw new Avalonia.Data.DataValidationException("Invalid trainer name.");
            }
        }
        void SetNormalizedOTName(string v)
        {
            otName = v;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OTName)));
        }
        string metLocation;
        public string MetLocation
        {
            get => metLocation;
            set { metLocation = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MetLocation))); }
        }
        byte levelMet;
        public byte LevelMet
        {
            get => levelMet;
            set { levelMet = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LevelMet))); }
        }
        string gameOfOrigin;
        public string GameOfOrigin
        {
            get => gameOfOrigin;
            set { gameOfOrigin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameOfOrigin))); }
        }
        string ball;
        public string Ball
        {
            get => ball;
            set { ball = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ball))); }
        }
        // ========== Battle ==========
        string item;
        public string Item
        {
            get => item;
            set { item = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Item))); }
        }
        byte ability;
        public byte Ability
        {
            get => ability;
            set { ability = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ability))); }
        }
        uint experience;
        public uint Experience
        {
            get => experience;
            set { experience = value; UpdateLvlRemExpFields(); }
        }
        uint remainingExperience;
        public uint RemainingExperience
        {
            get => remainingExperience;
            set { remainingExperience = value; UpdateExpFromLvlRemExpFields(); }
        }
        byte level;
        public byte Level
        {
            get => level;
            set { level = value; UpdateExpFromLvlField(); }
        }
        void RefreshExpFields()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingExperience)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Experience)));
        }
        void UpdateLvlRemExpFields()
        {
            ushort species = SpeciesU16();
            uint exp = Experience;
            int s = SpeciesConverter.SetG3Species(species);
            if (s == 0)
            {
                level = 0; remainingExperience = exp;
            }
            else
            {
                byte growth = PersonalInfo.Table[s].EXPGrowth;
                byte lvl = ExperienceConverter.GetLevel(exp, growth);
                uint remExp = exp - ExperienceConverter.GetEXP(lvl, growth);
                level = lvl; remainingExperience = remExp;
            }
            RefreshExpFields();
        }
        void UpdateExpFromLvlRemExpFields()
        {
            ushort species = SpeciesU16();
            uint expRem = RemainingExperience; byte lvl = Level;
            int s = SpeciesConverter.SetG3Species(species);
            uint exp = expRem;
            if (s != 0)
            {
                byte growth = PersonalInfo.Table[s].EXPGrowth;
                exp += ExperienceConverter.GetEXP(lvl, growth);
            }
            experience = exp;
            RefreshExpFields();
        }
        void UpdateExpFromLvlField()
        {
            ushort species = SpeciesU16();
            byte lvl = Level;
            int s = SpeciesConverter.SetG3Species(species);
            uint exp = 0;
            if (s != 0)
            {
                byte growth = PersonalInfo.Table[s].EXPGrowth;
                exp += ExperienceConverter.GetEXP(lvl, growth);
            }
            experience = exp;
            RefreshExpFields();
        }
        byte friendship;
        public byte Friendship
        {
            get => friendship;
            set { friendship = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Friendship))); }
        }
        // ========== Moves ==========
        string move1;
        public string Move1
        {
            get => move1;
            set { move1 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Move1))); }
        }
        string move2;
        public string Move2
        {
            get => move2;
            set { move2 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Move2))); }
        }
        string move3;
        public string Move3
        {
            get => move3;
            set { move3 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Move3))); }
        }
        string move4;
        public string Move4
        {
            get => move4;
            set { move4 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Move4))); }
        }
        byte pp1;
        public byte PP1
        {
            get => pp1;
            set { pp1 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PP1))); }
        }
        byte pp2;
        public byte PP2
        {
            get => pp2;
            set { pp2 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PP2))); }
        }
        byte pp3;
        public byte PP3
        {
            get => pp3;
            set { pp3 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PP3))); }
        }
        byte pp4;
        public byte PP4
        {
            get => pp4;
            set { pp4 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PP4))); }
        }
        byte ppb1;
        public byte PPb1
        {
            get => ppb1;
            set { ppb1 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PPb1))); }
        }
        byte ppb2;
        public byte PPb2
        {
            get => ppb2;
            set { ppb2 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PPb2))); }
        }
        byte ppb3;
        public byte PPb3
        {
            get => ppb3;
            set { ppb3 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PPb3))); }
        }
        byte ppb4;
        public byte PPb4
        {
            get => ppb4;
            set { ppb4 = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PPb4))); }
        }
        // ========== EVs ==========
        byte hpEV;
        public byte HpEV
        {
            get => hpEV;
            set { hpEV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpEV))); }
        }
        byte atkEV;
        public byte AtkEV
        {
            get => atkEV;
            set { atkEV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AtkEV))); }
        }
        byte defEV;
        public byte DefEV
        {
            get => defEV;
            set { defEV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefEV))); }
        }
        byte speedEV;
        public byte SpeedEV
        {
            get => speedEV;
            set { speedEV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeedEV))); }
        }
        byte speAtkEV;
        public byte SpeAtkEV
        {
            get => speAtkEV;
            set { speAtkEV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeAtkEV))); }
        }
        byte speDefEV;
        public byte SpeDefEV
        {
            get => speDefEV;
            set { speDefEV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeDefEV))); }
        }
        // ========== IVs ==========
        byte hpIV;
        public byte HpIV
        {
            get => hpIV;
            set { hpIV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpIV))); }
        }
        byte atkIV;
        public byte AtkIV
        {
            get => atkIV;
            set { atkIV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AtkIV))); }
        }
        byte defIV;
        public byte DefIV
        {
            get => defIV;
            set { defIV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DefIV))); }
        }
        byte speedIV;
        public byte SpeedIV
        {
            get => speedIV;
            set { speedIV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeedIV))); }
        }
        byte speAtkIV;
        public byte SpeAtkIV
        {
            get => speAtkIV;
            set { speAtkIV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeAtkIV))); }
        }
        byte speDefIV;
        public byte SpeDefIV
        {
            get => speDefIV;
            set { speDefIV = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeDefIV))); }
        }
        // ========== Condition ==========
        byte coolness;
        public byte Coolness
        {
            get => coolness;
            set { coolness = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Coolness))); }
        }
        byte beauty;
        public byte Beauty
        {
            get => beauty;
            set { beauty = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Beauty))); }
        }
        byte cuteness;
        public byte Cuteness
        {
            get => cuteness;
            set { cuteness = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cuteness))); }
        }
        byte smartness;
        public byte Smartness
        {
            get => smartness;
            set { smartness = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Smartness))); }
        }
        byte toughness;
        public byte Toughness
        {
            get => toughness;
            set { toughness = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Toughness))); }
        }
        byte feel;
        public byte Feel
        {
            get => feel;
            set { feel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Feel))); }
        }
        // ========== Misc ==========
        byte pokerusDays;
        public byte PokerusDays
        {
            get => pokerusDays;
            set { pokerusDays = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PokerusDays))); }
        }
        byte pokerusStrain;
        public byte PokerusStrain
        {
            get => pokerusStrain;
            set { pokerusStrain = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PokerusStrain))); }
        }
        uint ribbons;
        public uint Ribbons
        {
            get => ribbons;
            set { ribbons = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ribbons))); }
        }
        bool obedient;
        public bool Obedient
        {
            get => obedient;
            set { obedient = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Obedient))); }
        }

        // PID fields
        bool pidShiny;
        public bool PidShiny
        {
            get => pidShiny;
            set { pidShiny = value; UpdatePidFromPidFields(); }
        }
        byte pidNature;
        public byte PidNature
        {
            get => pidNature;
            set { pidNature = value; UpdatePidFromPidFields(); }
        }
        byte pidAbility;
        public byte PidAbility
        {
            get => pidAbility;
            set { pidAbility = value; UpdatePidFromPidFields(); }
        }
        byte pidGender;
        public byte PidGender
        {
            get => pidGender;
            set { pidGender = value; UpdatePidFromPidFields(); }
        }
        string[] abilities = { "-" };
        public string[] Abilities
        {
            get => abilities;
        }
        bool pausePIDChanges = false;
        void RefreshPidFields()
        {
            pausePIDChanges = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Abilities)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidShiny)));
            // Ugly hack because SelectedIndex is only invalidated if the value changes...
            byte pidAbility_ = pidAbility; byte pidGender_ = pidGender; byte pidNature_ = pidNature;
            pidAbility = 0xFF; pidGender = 0xFF; pidNature = 0xFF;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidAbility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidGender)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidNature)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PID)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OTID)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Species)));
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                pidAbility = pidAbility_;
                pidGender = pidGender_;
                pidNature = pidNature_;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidAbility)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidGender)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PidNature)));
                pausePIDChanges = false;
            });
        }
        void UpdatePidFields()
        {
            ushort species = SpeciesU16();
            uint otid = OTID; uint pid = PID;
            // Load abilities names
            int s = SpeciesConverter.SetG3Species(species);
            if (s == 0) abilities = new string[]{ "-" };
            else
            {
                string a1 = PersonalInfo.ABILITIES[PersonalInfo.Table[s].Ability1];
                string a2 = PersonalInfo.ABILITIES[PersonalInfo.Table[s].Ability2];
                if (PersonalInfo.Table[s].HasSecondAbility) abilities = new string[] { "1. " + a1, "2. " + a2 };
                else abilities = new string[] { "1. " + a1 };
            }
            // Load values
            pidShiny = PidCalculator.ShinyOfPID(pid, otid);
            pidAbility = PidCalculator.AbilityOfPID(pid, species);
            pidGender = (byte)PidCalculator.GenderOfPID(pid, species);
            pidNature = PidCalculator.NatureOfPID(pid);
            RefreshPidFields();
        }
        void UpdatePidFromPidFields()
        {
            if (pausePIDChanges) return;
            PidCalculator.PkmnGender g = PidCalculator.PkmnGender.Unknown;
            try { g = (PidCalculator.PkmnGender)PidGender; } catch { }
            uint? npid = PidCalculator.GenerateNewPID(OTID, SpeciesU16(), PidShiny, g, PidNature, PidAbility);
            if (npid.HasValue) { pid = npid.Value; UpdatePidFields(); }
            else { UpdatePidFields(); throw new Avalonia.Data.DataValidationException("No suitable PID."); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
