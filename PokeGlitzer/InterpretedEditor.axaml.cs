using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
#if DEBUG
            this.AttachDevTools();
#endif
        }
        public Pokemon Pokemon => pkmn;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

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

        public void RefreshControls()
        {
            InterpretedData d = view.Interpreted; // TODO
            PID = d.PID;
            OTID = d.OTID;
            Species = d.species;
            Egg = d.egg;
        }
        public void Save()
        {
            EVsIVs evs = view.Interpreted.EVs; // TODO
            EVsIVs ivs = view.Interpreted.IVs;
            Moves m = view.Interpreted.moves;
            Condition c = view.Interpreted.condition;
            InterpretedData d = new InterpretedData(PID, OTID, Species, Egg, m, evs, ivs, c);
            view.Interpreted = d;
        }
        public void SaveAndClose()
        {
            Save(); parent.Close();
        }

        uint pid;
        public uint PID
        {
            get => pid;
            set
            {
                pid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PID)));
            }
        }
        uint otid;
        public uint OTID
        {
            get => otid;
            set
            {
                otid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OTID)));
            }
        }
        ushort species;
        public ushort Species
        {
            get => species;
            set
            {
                species = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Species)));
            }
        }
        EggType egg;
        public EggType Egg
        {
            get => egg;
            set
            {
                egg = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Egg)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
