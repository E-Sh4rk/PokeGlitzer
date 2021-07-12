using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer.Converters
{
    // ========== DATA EDITOR ==========
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

            string v = (string)value;
            if (v.Length != 2 || !Utils.HasOnlyHexDigits(v))
                return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException(null), Avalonia.Data.BindingErrorType.DataValidationError);
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
                case 0:
                    return Brushes.Orchid;
                case 1:
                    return Brushes.SlateBlue;
                case 2:
                    return Brushes.DarkOrchid;
                case 3:
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
                case 0:
                    return "Growth (Substructure 1)";
                case 1:
                    return "Attacks (Substructure 2)";
                case 2:
                    return "EVs & Condition (Substructure 3)";
                case 3:
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
