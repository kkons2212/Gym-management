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
        if (targetType == typeof(bool))
        {
            if (value is true)
            {
                return false;
            }
            return true;
        }

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

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (b)
            {
                return false;
            }
            return true;
        }

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (b)
            {
                return false;
            }
            return true;
        }

        return true;
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

public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return (double)d;
        }
        if (value is int i)
        {
            return (double)i;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return (decimal)d;
        }
        return 0m;
    }
}
public class StatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return string.Empty;
        string status = value.ToString()?.ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "ACTIVE" => "Đang Hoạt Động",
            "PAID" => "Đã Thanh Toán",
            "PENDING" => "Chờ Xử Lý",
            "REFUNDED" => "Đã Hoàn Tiền",
            "FROZEN" => "Đang Bảo Lưu",
            "EXPIRED" => "Hết Hạn",
            "CANCELLED" => "Đã Hủy",
            "SUCCESS" => "Thành Công",
            "FAILED" => "Thất Bại",
            "DENIED" => "Từ Chối",
            _ => status
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public class PaymentMethodConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return string.Empty;
        string method = value.ToString()?.ToUpperInvariant() ?? string.Empty;
        return method switch
        {
            "CASH" => "Tiền Mặt",
            "VIETQR" => "VIETQR",
            "BANK_TRANSFER" => "Chuyển khoản",
            "CREDIT_CARD" => "Thẻ tín dụng",
            _ => method
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}
public class LockStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLocked)
        {
            return isLocked ? "Đang bị khóa" : "Đang hoạt động";
        }
        return "Đang hoạt động";
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public class LockButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLocked)
        {
            return isLocked ? "Mở khóa" : "Khóa";
        }
        return "Khóa";
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public class LockButtonBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLocked)
        {
            if (isLocked)
            {
                // Red for locked status
                return new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38));
            }
            // Green for active status
            return new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74));
        }
        return new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74));
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}
public class LockButtonActionBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isLocked)
        {
            if (isLocked)
            {
                // Green for unlock button (opposite of status)
                return new SolidColorBrush(ColorHelper.FromArgb(255, 22, 163, 74));
            }
            // Red for lock button (opposite of status)
            return new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38));
        }
        return new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38));
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}

public class CanManageUserConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string role)
        {
            bool isTargetSuperAdmin = string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
            if (isTargetSuperAdmin)
            {
                var sessionService = App.GetService<Services.Session.IAdminSessionService>();
                bool isCallerSuperAdmin = sessionService?.CurrentUser != null &&
                    string.Equals(sessionService.CurrentUser.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
                return isCallerSuperAdmin;
            }
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}
