using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace PokeGlitzer
{
    public partial class PokemonViewWindow100 : Window, IEditorWindow
    {
        Pokemon pkmn;
        public PokemonViewWindow100()
        {
            InitializeComponent();
            pkmn = new Pokemon(Utils.ByteCollectionOfSize<byte>(100), 0, 100, true);
            DataContext = new PokemonViewModel(pkmn, null);
        }
        public PokemonViewWindow100(RangeObservableCollection<byte> data, int offset, bool inTeam, MainWindowViewModel mw)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, offset, 100, inTeam);
            DataContext = new PokemonViewModel(pkmn, mw);

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
}
