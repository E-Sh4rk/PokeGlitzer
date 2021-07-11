using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer.Converters
{
    public class ByteStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();
            if (!(value is byte)) throw new NotImplementedException();
            return ((byte)value).ToString("X").PadLeft(2, '0');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(byte)) return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
            if (!(value is string)) return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
            /*StringBuilder normalized = new StringBuilder();
            foreach (char c in ((string)value).ToUpperInvariant())
            {
                if (c >= 'A' && c <= 'F' ||
                    c >= '0' && c <= '9')
                {
                    normalized.Append(c);
                }
            }
            if (normalized.Length > 2) normalized.Remove(0, normalized.Length - 2);
            else if (normalized.Length == 0) normalized.Append('0');
            return byte.Parse(normalized.ToString(), NumberStyles.HexNumber);*/
            string v = (string)value;
            if (v.Length != 2) { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException(null), Avalonia.Data.BindingErrorType.DataValidationError); }
            try { return byte.Parse(v, NumberStyles.HexNumber); }
            catch { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException(null), Avalonia.Data.BindingErrorType.DataValidationError); }
        }
    }
    public class SubstructureToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(IBrush)) throw new NotImplementedException();
            if (!(value is int)) throw new NotImplementedException();
            switch (value)
            {
                case 1:
                    return Brushes.Orchid;
                case 2:
                    return Brushes.SlateBlue;
                case 3:
                    return Brushes.DarkOrchid;
                case 4:
                    return Brushes.DarkSlateBlue;
                default:
                    return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }

    public class SubstructureToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();
            if (!(value is int)) throw new NotImplementedException();
            switch (value)
            {
                case 1:
                    return "Growth (Substructure 1)";
                case 2:
                    return "Attacks (Substructure 2)";
                case 3:
                    return "EVs & Condition (Substructure 3)";
                case 4:
                    return "Miscellaneous (Substructure 4)";
                default:
                    return "Invalid";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }

    public class BoolToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(IBrush)) throw new NotImplementedException();
            if (!(value is bool)) throw new NotImplementedException();
            if ((bool)value) return Brushes.DarkGreen;
            else return Brushes.DarkRed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
}
