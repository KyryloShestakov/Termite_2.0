using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TermiteUI_2;

public class BoolToButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Stop node" : "Start node";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString() == "Stop node";
    }
}