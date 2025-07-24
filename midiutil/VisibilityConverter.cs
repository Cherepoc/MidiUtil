using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MidiUtil;

public sealed class VisibilityConverter : IValueConverter
{
    private static readonly object VisibleObject = Visibility.Visible;
    private static readonly object NonVisibleObject = Visibility.Collapsed;

    public bool IsInverted { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isVisible = value switch
        {
            bool booleanValue => booleanValue,
            null => false,
            _ => true
        };

        return isVisible ^ IsInverted
            ? VisibleObject
            : NonVisibleObject;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
