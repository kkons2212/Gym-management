using System.IO;
using System.Net.Http;
using Windows.Storage.Streams;
using GymManagement.Desktop.Services.Api;
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
        string qrImageUrl,
        IApiClient? apiClient = null)
    {
        InitializeComponent();

        TxtAmount.Text = $"{amount:N0} đ";
        TxtBank.Text = bankName;
        TxtAccount.Text = accountNumber;
        TxtContent.Text = transferContent;

        IsPrimaryButtonEnabled = false;

        PrimaryButtonClick += (s, e) => { IsPaymentConfirmed = true; };

        LoadQrImage(qrImageUrl);
    }

    private async void LoadQrImage(string? qrImageUrl)
    {
        if (string.IsNullOrWhiteSpace(qrImageUrl))
        {
            QrLoadingRing.IsActive = false;
            QrLoadingRing.Visibility = Visibility.Collapsed;
            TxtQrError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            if (qrImageUrl.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                int commaIndex = qrImageUrl.IndexOf(",");
                string base64Data = qrImageUrl.Substring(commaIndex + 1);
                byte[] bytes = Convert.FromBase64String(base64Data);

                var stream = new InMemoryRandomAccessStream();
                using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(bytes);
                    await writer.StoreAsync();
                }

                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                QrImage.Source = bitmap;
            }
            else
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                var response = await httpClient.GetAsync(qrImageUrl);

                if (response.IsSuccessStatusCode)
                {
                    byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                    var stream = new InMemoryRandomAccessStream();
                    using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(imageBytes);
                        await writer.StoreAsync();
                    }

                    string mediaType = string.Empty;
                    if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType != null)
                    {
                        mediaType = response.Content.Headers.ContentType.MediaType;
                    }

                    if (mediaType.Contains("svg", StringComparison.OrdinalIgnoreCase) || qrImageUrl.Contains(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        var svgSource = new SvgImageSource();
                        svgSource.RasterizePixelWidth = 260;
                        svgSource.RasterizePixelHeight = 260;
                        await svgSource.SetSourceAsync(stream);
                        QrImage.Source = svgSource;
                    }
                    else
                    {
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);
                        QrImage.Source = bitmap;
                    }
                }
                else
                {
                    TxtQrError.Visibility = Visibility.Visible;
                }
            }
        }
        catch
        {
            TxtQrError.Visibility = Visibility.Visible;
        }
        finally
        {
            QrLoadingRing.IsActive = false;
            QrLoadingRing.Visibility = Visibility.Collapsed;
        }
    }

    private void BtnManualConfirm_Click(object sender, RoutedEventArgs e)
    {
        IsPaymentConfirmed = true;
        BtnManualConfirm.IsEnabled = false;
        BtnManualConfirm.Content = "✓ Đã ghi nhận thanh toán";
        TxtPaymentStatus.Text = "Hóa đơn đã thanh toán thành công. Sẵn sàng in phiếu thu!";

        var brush = Application.Current.Resources["StatusSuccessBrush"] as Brush;
        if (brush != null)
        {
            TxtPaymentStatus.Foreground = brush;
        }

        IsPrimaryButtonEnabled = true;
        this.Hide();
    }
}
