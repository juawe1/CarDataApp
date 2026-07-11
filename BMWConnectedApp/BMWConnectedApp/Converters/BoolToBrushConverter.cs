using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace BMWConnectedApp.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isConnected = false;
        if (value is bool b) isConnected = b;
        if (isConnected)
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Green);
        }
        else
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Red);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new System.NotImplementedException();
    }
}
