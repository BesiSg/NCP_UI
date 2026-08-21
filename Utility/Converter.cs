using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Utility
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateToString : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return ((DateTime)value).ToString("yyyy-MM-dd");
        }
        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            var Date = new DateTime();
            if (DateTime.TryParseExact((string)value, "dd-MM-yyyy", new CultureInfo("en-US"),
                                 DateTimeStyles.None, out Date))
                return Date;
            return DateTime.Now;
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString().Equals(parameter.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool isChecked) || !isChecked)
                return Binding.DoNothing;

            return Enum.Parse(targetType, parameter.ToString());
        }
    }

    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is bool isDirectory)
                return isDirectory ? "📁" : "📄";

            return "📄";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooltoVis : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null) return Visibility.Collapsed;
            return ((bool)value) == true ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }
    }
    [ValueConversion(typeof(string), typeof(bool))]
    public class stringtoBool : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null || (string)value == string.Empty) return false;
            return ((string)value) == "True";
        }
        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "True" : "False";

        }
    }
    [ValueConversion(typeof(SearchResult), typeof(Brushes))]
    public class SearchResultToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SearchResult result)
                return Brushes.Transparent;

            return result switch
            {
                SearchResult.NotFinding => Brushes.Transparent,
                SearchResult.Finding => Brushes.Orange,
                SearchResult.Found => Brushes.Lime,
                SearchResult.NotFound => Brushes.Red,
                _ => Brushes.Transparent
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
    }
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooltoBrowseCommits : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "browse" : "commits";

        }
        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null || (string)value == string.Empty) return false;
            return ((string)value) == "browse";
        }

    }
}
