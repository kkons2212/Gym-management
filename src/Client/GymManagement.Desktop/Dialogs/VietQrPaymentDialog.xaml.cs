using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using GymManagement.Desktop.Services.Hardware;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class VietQrPaymentDialog : ContentDialog
{
    private readonly IAudioFeedbackService _audioService;
    public bool IsPaymentConfirmed { get; private set; }

    public VietQrPaymentDialog(
        decimal amount,
        string bankName,
        string accountNumber,
        string transferContent,
        string qrImageUrl,
        IAudioFeedbackService audioService)
    {
        InitializeComponent();
        _audioService = audioService;

        TxtAmount.Text = $"{amount:N0} đ";
        TxtBank.Text = bankName;
        TxtAccount.Text = accountNumber;
        TxtContent.Text = transferContent;

        if (!string.IsNullOrEmpty(qrImageUrl))
        {
            QrImage.Source = new BitmapImage(new Uri(qrImageUrl));
        }

        IsPrimaryButtonEnabled = false;
    }

    private void BtnSimulatePayment_Click(object sender, RoutedEventArgs e)
    {
        IsPaymentConfirmed = true;
        BtnSimulatePayment.IsEnabled = false;
        BtnSimulatePayment.Content = "✓ Đã thanh toán thành công";

        TxtPaymentStatus.Text = "Thanh toán mô phỏng thành công! Hóa đơn đã sẵn sàng.";
        TxtPaymentStatus.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["StatusSuccessBrush"];

        _audioService.PlaySuccessBeep();
        IsPrimaryButtonEnabled = true;
    }
}

