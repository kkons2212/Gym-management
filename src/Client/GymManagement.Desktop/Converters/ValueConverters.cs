using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace GymManagement.Desktop.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string status = string.Empty;
        if (value != null)
        {
            status = value.ToString()?.ToUpperInvariant() ?? string.Empty;
        }

        if (status == "ACTIVE" || status == "PAID" || status == "SUCCESS")
        {
            return new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74));   // Green 600
        }

        if (status == "FROZEN" || status == "WARNING")
        {
            return new SolidColorBrush(ColorHelper.FromArgb(255, 217, 119, 6)); // Amber 600
        }

        if (status == "EXPIRED" || status == "CANCELLED" || status == "DANGER" || status == "FAILED")
        {
            return new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38)); // Red 600
        }

        return new SolidColorBrush(ColorHelper.FromArgb(255, 148, 163, 184)); // Slate 400
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}

public class CurrencyFormatterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return $"{d:N0} đ";
        }

        if (value is int i)
        {
            return $"{i:N0} đ";
        }

        return "0 đ";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}

public class BmiEvaluationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal bmi)
        {
            if (bmi < 18.5m)
            {
                return "Thiếu cân (Gầy)";
            }

            if (bmi < 25.0m)
            {
                return "Bình thường (Chuẩn)";
            }

            if (bmi < 30.0m)
            {
                return "Thừa cân";
            }

            return "Béo phì";
        }

        return "Chưa xác định";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}

public class InvertedBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
        {
            return Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}

public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (parameter is string format)
        {
            if (value is DateTime dt)
            {
                return dt.ToString(format);
            }

            if (value is TimeSpan ts)
            {
                return ts.ToString(format);
            }

            return string.Format(format, value);
        }

        if (value is DateTime d)
        {
            return d.ToString("dd/MM/yyyy");
        }

        if (value is TimeSpan t)
        {
            return t.ToString(@"hh\:mm");
        }

        string? str = value.ToString();
        if (str != null)
        {
            return str;
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}

public class TimeOnlyFormatterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            return dt.ToString("HH:mm:ss");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
