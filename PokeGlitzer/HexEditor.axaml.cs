using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace PokeGlitzer
{
    public partial class HexEditor : Window
    {
        Pokemon pkmn;
        public HexEditor()
        {
            InitializeComponent();
            pkmn = new Pokemon(Utils.ByteCollectionOfSize(80), 0);
            DataContext = new HexEditorModel(pkmn);
        }
        public HexEditor(RangeObservableCollection<byte> data, int offset)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, offset);
            DataContext = new HexEditorModel(pkmn);
#if DEBUG
            this.AttachDevTools();
#endif
        }

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
    public class HexEditorModel : INotifyPropertyChanged
    {
        PokemonView view;

        public HexEditorModel(Pokemon pkmn)
        {
            view = pkmn.View;
            //view.Data.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            Avalonia.Utilities.WeakEventHandlerManager.Subscribe<ObservableCollection<byte>, NotifyCollectionChangedEventArgs, HexEditorModel>(view.Data,
                nameof(ObservableCollection<byte>.CollectionChanged), (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))));
        }

        public string Text
        {
            get
            {
                StringBuilder res = new StringBuilder();
                RangeObservableCollection<byte> col = view.Data;
                int i = -1;
                foreach (byte b in col)
                {
                    i++;
                    int mod = i % 8;
                    if (mod == 0) { if (i != 0) res.Append(Environment.NewLine); }
                    else if (mod == 4) res.Append("   ");
                    else res.Append(" ");
                    res.Append(b.ToString("X").PadLeft(2, '0'));
                }
                return res.ToString();
            }
            set
            {
                try
                {
                    RangeObservableCollection<byte> col = view.Data;
                    byte[] res = new byte[col.Count];

                    int j = 0;
                    int i = 0;
                    while (i < value.Length)
                    {
                        char c = value[i++];
                        if (c == ' ' || c == '\t' || c == '\r' || c == '\n') continue;
                        char c2 = value[i++];
                        string str = string.Join("", new char[] { c, c2 });
                        if (!Utils.HasOnlyHexDigits(str)) throw new Exception();
                        byte b = byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        res[j++] = b;
                    }
                    if (j != res.Length) throw new Exception();

                    Utils.UpdateCollectionRange(col, res);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
                catch { throw new Avalonia.Data.DataValidationException(null); }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
