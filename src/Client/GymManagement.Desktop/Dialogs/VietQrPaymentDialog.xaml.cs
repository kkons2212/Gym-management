using GymManagement.Desktop.Services.Api;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class VietQrPaymentDialog : ContentDialog
{
    private readonly decimal _amount;
    private readonly string _accountNumber;
    private readonly string _transferContent;
    private readonly IApiClient? _apiClient;

    public bool IsPaymentConfirmed { get; private set; }

    public VietQrPaymentDialog(
        decimal amount,
        string bankName,
        string accountNumber,
        string transferContent,
        string qrImageUrl,
        IApiClient? apiClient = null)
    {
        InitializeComponent();

        _amount = amount;
        _accountNumber = accountNumber;
        _transferContent = transferContent;
        _apiClient = apiClient;

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

    private async void BtnSimulatePayment_Click(object sender, RoutedEventArgs e)
    {
        BtnSimulatePayment.IsEnabled = false;
        BtnManualConfirm.IsEnabled = false;
        LoadingRing.Visibility = Visibility.Visible;
        LoadingRing.IsActive = true;
        TxtPaymentStatus.Text = "Đang gửi tín hiệu Test Callback tới VietQR Sandbox...";

        if (_apiClient != null)
        {
            string bankCode = Environment.GetEnvironmentVariable("VIETQR_BANK_CODE") ?? "MB";
            await _apiClient.SimulateVietQrCallbackAsync(_accountNumber, _transferContent, Convert.ToInt64(_amount), bankCode);
        }

        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;

        IsPaymentConfirmed = true;
        BtnSimulatePayment.Content = "✓ Đã xác nhận thanh toán (Sandbox VietQR)";
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
        BtnSimulatePayment.IsEnabled = false;
        BtnManualConfirm.IsEnabled = false;
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;

        IsPaymentConfirmed = true;
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
