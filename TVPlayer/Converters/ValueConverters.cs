using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TVPlayer.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value is bool bv && bv;
            if (Invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is Visibility v && v == Visibility.Visible;
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class PlayPauseIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b && b ? "⏸" : "▶";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class MuteIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is bool b && b ? "🔇" : "🔊";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    [ValueConversion(typeof(float), typeof(double))]
    public class FloatToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is float f ? (double)f : 0d;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double d ? (float)d : 0f;
    }

    [ValueConversion(typeof(long), typeof(double))]
    public class LongToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is long l ? (double)l : 0d;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double d ? (long)d : 0L;
    }

    /// <summary>Converts a null value to Visibility.</summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isNull = value is null || (value is string s && string.IsNullOrWhiteSpace(s));
            if (Invert) isNull = !isNull;
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    /// <summary>Formats the window title: "TV Player" when empty, "TV Player — {name}" when playing.</summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class ChannelTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string s && !string.IsNullOrWhiteSpace(s)
                ? $"TV Player | {s}"
                : "TV Player";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
