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
            pkmn = new Pokemon(Utils.ByteCollectionOfSize<byte>(80), 0);
            DataContext = new InterpretedEditorModel(pkmn);
        }

        public InterpretedEditor(RangeObservableCollection<byte> data, int offset)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, offset);
            DataContext = new InterpretedEditorModel(pkmn);
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

        public InterpretedEditorModel(Pokemon pkmn)
        {
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
            if (view.Interpreted == null) return;
            InterpretedData d = view.Interpreted!;
            PID = d.PID;
            OTID = d.OTID;
            Species = d.species;
            Egg = d.egg;
        }
        public void Save()
        {
            InterpretedData d = new InterpretedData(PID, OTID, Species, Egg);
            view.Interpreted = d;
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
