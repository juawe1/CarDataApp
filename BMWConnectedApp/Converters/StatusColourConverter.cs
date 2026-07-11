using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace BMWConnectedApp.Converters;

public class StatusColourConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool connected = value is bool && (bool)value;
        return connected ? new SolidColorBrush(Microsoft.UI.Colors.LimeGreen) : new SolidColorBrush(Microsoft.UI.Colors.Gray); // Green for connected
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
