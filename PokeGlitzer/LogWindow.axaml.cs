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
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
            DataContext = new LogWindowModel();
        }

        public string Text
        {
            get => ((LogWindowModel)DataContext!).Text;
            set => ((LogWindowModel)DataContext!).Text = value;
        }
    }
    public class LogWindowModel : INotifyPropertyChanged
    {
        public LogWindowModel() { }

        string text = "";
        public string Text
        {
            get => text;
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
