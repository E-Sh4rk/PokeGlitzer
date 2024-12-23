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
            pkmn = new Pokemon(Utils.CollectionOfSize<byte>(100), DataLocation.DefaultTeam);
            DataContext = new PokemonViewModel(pkmn, null);
        }
        public PokemonViewWindow100(RangeObservableCollection<byte> data, int offset, Source src, MainWindowViewModel mw)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, new DataLocation(offset, 100, src));
            DataContext = new PokemonViewModel(pkmn, mw);
        }
        public DataLocation DataLocation => pkmn.DataLocation;

        protected override void OnClosed(EventArgs e)
        {
            pkmn.Dispose();
            base.OnClosed(e);
        }
    }
}
