using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PokeGlitzer
{
    public partial class HexEditor : Window
    {
        public HexEditor()
        {
            InitializeComponent();
            DataContext = new PokemonView(80);
        }
        public HexEditor(Pokemon pkmn)
        {
            InitializeComponent();
            DataContext = pkmn.View;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    /*public class PokemonHexEditorModel : INotifyPropertyChanged
    {
        Pokemon? pkmn = null;
        PokemonView view;

        public PokemonHexEditorModel()
        {
            view = new PokemonView(80);
        }
        public PokemonHexEditorModel(Pokemon pkmn)
        {
            this.pkmn = pkmn;
            view = pkmn.View;
        }

        public PokemonView View
        {
            get => view;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }*/
}
