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
    // ========== GENRAL PURPOSE ==========
    public class NumberToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();
            long nb = (long)System.Convert.ChangeType(value, typeof(Int64));
            return Utils.NumberToString(nb, (string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
            try { return Utils.ToNumber((string)value, targetType); }
            catch { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException("Invalid number."), Avalonia.Data.BindingErrorType.DataValidationError); }
        }
    }
    public class BoolToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return (bool)value ? 1 : 0; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return (int)value == 0 ? false : true; }
    }

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
                return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException("Invalid byte."), Avalonia.Data.BindingErrorType.DataValidationError);
            try { return byte.Parse(v, NumberStyles.HexNumber); }
            catch { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException("Invalid byte."), Avalonia.Data.BindingErrorType.DataValidationError); }
        }
    }
    public class SubstructureToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(IBrush)) throw new NotImplementedException();
            if (!(value is int)) throw new NotImplementedException();
            return value switch
            {
                0 => Brushes.Orchid,
                1 => Brushes.SlateBlue,
                2 => Brushes.DarkOrchid,
                3 => Brushes.DarkSlateBlue,
                _ => Brushes.Black
            };
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
            return value switch
            {
                0 => "Growth (Substructure 1)",
                1 => "Attacks (Substructure 2)",
                2 => "EVs & Condition (Substructure 3)",
                3 => "Miscellaneous (Substructure 4)",
                _ => "Invalid"
            };
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
    public class EggTypeToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return (int)value; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return (EggType)value; }
    }
    public class PkmnGenderTypeToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return (int)value; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return (PidCalculator.PkmnGender)value; }
    }
    public class PokemonToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();

            long nb = (long)System.Convert.ChangeType(value, typeof(Int64));
            int species = SpeciesConverter.SetG3Species(nb);
            if (species == 0)
                return Utils.NumberToString(nb, (string)parameter);
            else
                return SpeciesConverter.SPECIES[species];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
            try {
                string v = (string)value;
                int i = Array.IndexOf(SpeciesConverter.SPECIES_LOWERCASE, v.ToLowerInvariant());
                if (i >= 0)
                {
                    int species = SpeciesConverter.GetG3Species(i);
                    if (species != 0)
                        return System.Convert.ChangeType(species, targetType);
                }
                return Utils.ToNumber(v, targetType);
            }
            catch { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException("Invalid species."), Avalonia.Data.BindingErrorType.DataValidationError); }
        }
    }
    public abstract class TextDataToStringConverter : IValueConverter
    {
        protected abstract string[] Data();
        protected abstract string[] LowercaseData();
        protected abstract int NormalizeID(long _);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();

            long nb = (long)System.Convert.ChangeType(value, typeof(Int64));
            int id = NormalizeID(nb);
            if (id < 0)
                return Utils.NumberToString(nb, (string)parameter);
            else
                return Data()[id];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string)) return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
            try {
                string v = (string)value;
                int i = Array.IndexOf(LowercaseData(), v.ToLowerInvariant());
                if (i >= 0)
                {
                    int id = NormalizeID(i);
                    if (id >= 0)
                        return System.Convert.ChangeType(id, targetType);
                }
                return Utils.ToNumber(v, targetType);
            }
            catch { return new Avalonia.Data.BindingNotification(new Avalonia.Data.DataValidationException("Invalid data."), Avalonia.Data.BindingErrorType.DataValidationError); }
        }
    }
    public class BallToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.BALLS; }
        protected override string[] LowercaseData() { return TextData.BALLS_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeBall(id); }
    }
    public class GameToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.GAMES; }
        protected override string[] LowercaseData() { return TextData.GAMES_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeGame(id); }
    }
    public class GenderToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.GENDERS; }
        protected override string[] LowercaseData() { return TextData.GENDERS_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeGender(id); }
    }
    public class LanguageToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.LANGUAGES; }
        protected override string[] LowercaseData() { return TextData.LANGUAGES_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeLanguage(id); }
    }
    public class ItemToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.ITEMS; }
        protected override string[] LowercaseData() { return TextData.ITEMS_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeItem(id); }
    }
    public class NatureToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.NATURES; }
        protected override string[] LowercaseData() { return TextData.NATURES_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeNature(id); }
    }
    public class LocationToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.LOCATIONS; }
        protected override string[] LowercaseData() { return TextData.LOCATIONS_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeLocation(id); }
    }
    public class MoveToStringConverter : TextDataToStringConverter
    {
        protected override string[] Data() { return TextData.MOVES; }
        protected override string[] LowercaseData() { return TextData.MOVES_LOWERCASE; }
        protected override int NormalizeID(long id) { return TextData.NormalizeMove(id); }
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
                if (data.species == 0) return ResLoader.None;
                switch (data.egg)
                {
                    case EggType.Invalid:
                        return ResLoader.Error;
                    case EggType.Egg:
                        return ResLoader.Egg;
                    case EggType.BadEgg:
                        return ResLoader.BadEgg;
                    case EggType.NotAnEgg:
                        int species = SpeciesConverter.SetG3Species(data.species);
                        if (data.IsShiny)
                            return ResLoader.ShinySpecies(species);
                        else
                            return ResLoader.Species(species);
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
    public class PokemonToShortLabel : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                InterpretedData data = (InterpretedData?)value!;
                if (data.hasSpecies) return data.species.ToString("x");
                else return "(" + data.species.ToString("x") + ")";
            }
            catch { }
            return "ERR";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
    public class ItemToBitmap : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                InterpretedData data = (InterpretedData?)value!;
                if (data.battle.item == 0) return ResLoader.NoneItem;
                if (ItemConverter.IsItemTM(data.battle.item)) return ResLoader.TM;
                if (ItemConverter.IsItemHM(data.battle.item)) return ResLoader.HM;
                if (ItemConverter.IsItemMail(data.battle.item))
                    return ResLoader.Mails(ItemConverter.GetMailID(data.battle.item));
                int id = ItemConverter.GetItemFuture3(data.battle.item);
                return ResLoader.Items(id);
            }
            catch { }
            return ResLoader.NoneItem;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotImplementedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
}
