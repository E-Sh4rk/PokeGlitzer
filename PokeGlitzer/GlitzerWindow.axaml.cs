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

namespace PokeGlitzer
{
    public partial class GlitzerWindow : Window
    {
        public GlitzerWindow()
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(null, this,
                Utils.CollectionOfSize<byte>(MainWindowViewModel.BOX_PKMN_SIZE * MainWindowViewModel.BOX_SIZE * MainWindowViewModel.BOX_NUMBER));
        }
        public GlitzerWindow(MainWindowViewModel mw, RangeObservableCollection<byte> data)
        {
            InitializeComponent();
            DataContext = new GlitzerWindowViewModel(mw, this, data);

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class GlitzerWindowViewModel : INotifyPropertyChanged
    {
        MainWindowViewModel? mw;
        GlitzerWindow parent;
        const int SIZE = 12 * MainWindowViewModel.TEAM_PKMN_SIZE;
        RangeObservableCollection<byte> data;
        public GlitzerWindowViewModel(MainWindowViewModel? mw, GlitzerWindow parent, RangeObservableCollection<byte> data)
        {
            this.mw = mw;
            this.parent = parent;
            this.data = data;
            parent.Activated += (_, _) => { UpdateCurrentSelection(); };
            parent.Deactivated += (_, _) => { CurrentSelection = null; };
        }

        void UpdateCurrentSelection()
        {
            CurrentSelection = new DataLocation(CurrentOffset, Math.Min(SIZE, data.Count - CurrentOffset), false);
        }

        DataLocation? currentSelection = null;
        public DataLocation? CurrentSelection
        {
            get => currentSelection;
            private set
            {
                currentSelection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSelection)));
            }
        }

        int currentOffset = 0;
        public int CurrentOffset
        {
            get => currentOffset;
            set
            {
                if (value < 0 || value + SIZE >= data.Count)
                    throw new Avalonia.Data.DataValidationException(null);
                currentOffset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOffset)));
                UpdateCurrentSelection();
            }
        }

        public void Prev() { try { CurrentOffset -= 4; } catch { } }
        public void Next() { try { CurrentOffset += 4; } catch { } }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
