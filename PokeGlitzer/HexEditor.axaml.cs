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
    public partial class HexEditor : Window, IEditorWindow
    {
        Pokemon? pkmn;
        DataLocation dataLocation;
        public HexEditor()
        {
            InitializeComponent();
            pkmn = new Pokemon(Utils.CollectionOfSize<byte>(Pokemon.PC_SIZE), DataLocation.DefaultPC);
            dataLocation = pkmn.DataLocation;
            DataContext = new HexEditorModel(pkmn.View.Data, false);
        }
        public HexEditor(RangeObservableCollection<byte> data, DataLocation dl)
        {
            InitializeComponent();
            pkmn = new Pokemon(data, dl);
            dataLocation = dl;
            DataContext = new HexEditorModel(pkmn.View.Data, false);
        }
        public HexEditor(RangeObservableCollection<byte> data, Source src)
        {
            InitializeComponent();
            pkmn = null;
            dataLocation = new DataLocation(0, data.Count, src);
            int lineSize = 0;
            if (src == Source.Team) lineSize = Pokemon.TEAM_SIZE;
            if (src == Source.PC) lineSize = Pokemon.PC_SIZE;
            if (src == Source.BoxNames) lineSize = BoxNames.BOX_NAME_BYTE_SIZE;
            DataContext = new HexEditorModel(data, true, lineSize);
        }
        public DataLocation DataLocation => dataLocation;

        protected override void OnClosed(EventArgs e)
        {
            if (pkmn != null) pkmn.Dispose();
            base.OnClosed(e);
        }
    }
    public class HexEditorModel : INotifyPropertyChanged
    {
        RangeObservableCollection<byte> data;
        bool custom;
        int lineSize;

        public HexEditorModel(RangeObservableCollection<byte> data, bool custom, int lineSize = 0)
        {
            this.data = data;
            this.custom = custom;
            this.lineSize = lineSize;
            //view.Data.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            Avalonia.Utilities.WeakEventHandlerManager.Subscribe<ObservableCollection<byte>, NotifyCollectionChangedEventArgs, HexEditorModel>(data,
                nameof(ObservableCollection<byte>.CollectionChanged), (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))));
        }

        public string Text
        {
            get
            {
                StringBuilder res = new StringBuilder();
                int i = -1;
                foreach (byte b in data)
                {
                    i++;
                    if (custom)
                    {
                        bool startOfLine = i % lineSize == 0;
                        if (startOfLine) { if (i != 0) res.Append(Environment.NewLine); }
                        else res.Append(" ");
                        res.Append(b.ToString("X").PadLeft(2, '0'));
                    }
                    else
                    {
                        int mod = i % 8;
                        if (mod == 0) { if (i != 0) res.Append(Environment.NewLine); }
                        else if (mod == 4) res.Append("   ");
                        else res.Append(" ");
                        res.Append(b.ToString("X").PadLeft(2, '0'));
                    }
                }
                res.AppendLine(Environment.NewLine);
                return res.ToString();
            }
            set
            {
                try
                {
                    byte[] res = new byte[data.Count];
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

                    Utils.UpdateCollectionRange(data, res);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
                catch { throw new Avalonia.Data.DataValidationException("Invalid hexadecimal data."); }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
