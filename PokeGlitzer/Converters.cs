using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    // ========== INTERPRETED EDITOR ==========

    public class NumberToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();

            long nb = (long)System.Convert.ChangeType(value, typeof(Int64));
            string format = (string)parameter;
            if (format == "X" || format == "x")
                return "0x" + nb.ToString(format);
            else
                return nb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);

            string v = (string)value;
            try
            {
                checked
                {
                    return System.Convert.ChangeType(new Int64Converter().ConvertFromString(v), targetType);
                }
            }
            catch { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException(null), Avalonia.Data.BindingErrorType.DataValidationError); }
        }
    }

    public class EggTypeToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return (int)value; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return (EggType)value; }
    }

    // ========== MAIN WINDOW ==========
    public class SelectionToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Brushes.Yellow;
            else
                return Brushes.LightGray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
    public class PokemonToBitmap : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                InterpretedData data = (InterpretedData?)value!;
                switch (data.egg)
                {
                    case EggType.None:
                        return ResLoader.None;
                    case EggType.Invalid:
                        return ResLoader.Error;
                    case EggType.Egg:
                    case EggType.BadEgg:
                        return ResLoader.Egg;
                    case EggType.NotAnEgg:
                        if (data.species > ResLoader.MAX_SPECIES)
                            return ResLoader.Unknown;
                        if (data.IsShiny)
                            return ResLoader.ShinySpecies[data.species];
                        else
                            return ResLoader.Species[data.species];
                }
            }
            catch { }
            return ResLoader.None;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
}
