using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class VietQrPaymentDialog : ContentDialog
{
    public bool IsPaymentConfirmed { get; private set; }

    public VietQrPaymentDialog(
        decimal amount,
        string bankName,
        string accountNumber,
        string transferContent,
        string qrImageUrl)
    {
        InitializeComponent();

        TxtAmount.Text = $"{amount:N0} đ";
        TxtBank.Text = bankName;
        TxtAccount.Text = accountNumber;
        TxtContent.Text = transferContent;

        if (!string.IsNullOrEmpty(qrImageUrl))
        {
            QrImage.Source = new BitmapImage(new Uri(qrImageUrl));
        }

        IsPrimaryButtonEnabled = false;

        // Ràng buộc hiển thị nút giả lập thanh toán theo biến môi trường (ui_rules.md 2.2)
        string? simEnv = Environment.GetEnvironmentVariable("VIETQR_ENABLE_SIMULATION");
        string? aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        bool isSimulationEnabled = false;

        if (string.Equals(simEnv, "true", StringComparison.OrdinalIgnoreCase))
        {
            isSimulationEnabled = true;
        }
        else if (string.Equals(aspnetEnv, "Development", StringComparison.OrdinalIgnoreCase))
        {
            isSimulationEnabled = true;
        }

        if (!isSimulationEnabled)
        {
            BtnSimulatePayment.Visibility = Visibility.Collapsed;
            SandboxBanner.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnSimulatePayment_Click(object sender, RoutedEventArgs e)
    {
        BtnSimulatePayment.IsEnabled = false;
        BtnSimulatePayment.Content = "Đang xử lý mô phỏng...";

        IsPaymentConfirmed = true;
        BtnSimulatePayment.Content = "✓ Đã xác nhận thanh toán (Mô phỏng)";
        TxtPaymentStatus.Text = "Kích hoạt gói tập thành công! Hóa đơn đã sẵn sàng in.";

        var brush = Application.Current.Resources["StatusSuccessBrush"] as Brush;
        if (brush != null)
        {
            TxtPaymentStatus.Foreground = brush;
        }

        IsPrimaryButtonEnabled = true;
    }

    private void BtnManualConfirm_Click(object sender, RoutedEventArgs e)
    {
        IsPaymentConfirmed = true;
        BtnManualConfirm.IsEnabled = false;
        BtnManualConfirm.Content = "✓ Đã xác nhận thủ công";
        TxtPaymentStatus.Text = "Thu ngân đã xác nhận thanh toán thành công.";

        var brush = Application.Current.Resources["StatusSuccessBrush"] as Brush;
        if (brush != null)
        {
            TxtPaymentStatus.Foreground = brush;
        }

        IsPrimaryButtonEnabled = true;
    }
}
