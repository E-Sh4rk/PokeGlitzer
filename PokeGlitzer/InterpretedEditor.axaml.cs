using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace PokeGlitzer
{
    public partial class InterpretedEditor : Window
    {
        public InterpretedEditor()
        {
            InitializeComponent();
            DataContext = new InterpretedEditorModel();
        }

        public InterpretedEditor(Pokemon pkmn)
        {
            InitializeComponent();
            DataContext = new InterpretedEditorModel(pkmn);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class InterpretedEditorModel : INotifyPropertyChanged
    {
        PokemonView view;

        public InterpretedEditorModel()
        {
            view = new PokemonView(80);
            RefreshControls();
        }
        public InterpretedEditorModel(Pokemon pkmn)
        {
            view = pkmn.View;
            view.PropertyChanged += ViewInterpretedChanged;
            RefreshControls();
        }

        void ViewInterpretedChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (sender == this) return;
            if (args.PropertyName != nameof(PokemonView.Intepreted)) return;
            RefreshControls();
        }

        public void RefreshControls()
        {
            if (view.Intepreted == null) return;
            InterpretedData d = view.Intepreted!;
            PID = d.PID;
            OTID = d.OTID;
            Egg = d.egg;
        }
        public void Save()
        {
            InterpretedData d = new InterpretedData(PID, OTID, Egg);
            view.Intepreted = d;
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
