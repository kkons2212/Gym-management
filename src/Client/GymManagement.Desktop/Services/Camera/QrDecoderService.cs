using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace GymManagement.Desktop.Services.Camera;

/// <summary>
/// Dịch vụ phân tích hình ảnh và giải mã mã QR sử dụng thư viện ZXing.Net.
/// Tích hợp hàng rào chống quét trùng lặp (Debounce Cooldown 3 giây).
/// Tuân thủ quy chuẩn dev_rule.md (0 toán tử 3 ngôi, flat guard clauses).
/// </summary>
public sealed class QrDecoderService : IQrDecoderService
{
    private readonly TimeSpan _sameCodeCooldown = TimeSpan.FromSeconds(3);
    private readonly TimeSpan _differentCodeCooldown = TimeSpan.FromMilliseconds(1200);

    private string? _lastCode;
    private DateTime _lastScanTime = DateTime.MinValue;
    private readonly object _syncLock = new();

    /// <summary>
    /// Giải mã mã QR từ SoftwareBitmap của camera.
    /// </summary>
    public string? DecodeFromSoftwareBitmap(SoftwareBitmap bitmap)
    {
        if (bitmap == null)
        {
            return null;
        }

        try
        {
            SoftwareBitmap targetBitmap;
            bool isConverted;

            isConverted = false;
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                targetBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                isConverted = true;
            }
            else
            {
                targetBitmap = bitmap;
            }

            int width;
            int height;
            byte[] pixelBytes;

            width = targetBitmap.PixelWidth;
            height = targetBitmap.PixelHeight;
            pixelBytes = new byte[4 * width * height];

            targetBitmap.CopyToBuffer(pixelBytes.AsBuffer());

            if (isConverted)
            {
                targetBitmap.Dispose();
            }

            RGBLuminanceSource luminanceSource;
            luminanceSource = new RGBLuminanceSource(pixelBytes, width, height, RGBLuminanceSource.BitmapFormat.BGRA32);

            HybridBinarizer binarizer;
            binarizer = new HybridBinarizer(luminanceSource);

            BinaryBitmap binaryBitmap;
            binaryBitmap = new BinaryBitmap(binarizer);

            QRCodeReader qrReader;
            qrReader = new QRCodeReader();

            Result? result;
            result = qrReader.decode(binaryBitmap);

            if (result != null)
            {
                if (!string.IsNullOrWhiteSpace(result.Text))
                {
                    return result.Text;
                }
            }

            return null;
        }
        catch (Exception)
        {
            // Trả về null nếu frame không chứa mã QR hoặc hình ảnh bị nhòe
            return null;
        }
    }

    /// <summary>
    /// Giải mã mã QR từ file hình ảnh.
    /// </summary>
    public async Task<string?> DecodeFromFileAsync(StorageFile file)
    {
        if (file == null)
        {
            return null;
        }

        try
        {
            using IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder;
            decoder = await BitmapDecoder.CreateAsync(stream);

            using SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            return DecodeFromSoftwareBitmap(softwareBitmap);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Kiểm tra và chặn quét trùng lặp mã QR.
    /// </summary>
    public bool TryProcessScannedCode(string rawCode, out string? acceptedCode)
    {
        acceptedCode = null;

        if (string.IsNullOrWhiteSpace(rawCode))
        {
            return false;
        }

        string trimmed;
        trimmed = rawCode.Trim();

        lock (_syncLock)
        {
            DateTime now;
            now = DateTime.UtcNow;

            if (string.Equals(trimmed, _lastCode, StringComparison.OrdinalIgnoreCase))
            {
                TimeSpan elapsed;
                elapsed = now - _lastScanTime;
                if (elapsed < _sameCodeCooldown)
                {
                    return false;
                }
            }
            else
            {
                TimeSpan elapsed;
                elapsed = now - _lastScanTime;
                if (elapsed < _differentCodeCooldown)
                {
                    return false;
                }
            }

            _lastCode = trimmed;
            _lastScanTime = now;
            acceptedCode = trimmed;
            return true;
        }
    }

    /// <summary>
    /// Đặt lại bộ đệm thời gian chống quét trùng.
    /// </summary>
    public void ResetCooldown()
    {
        lock (_syncLock)
        {
            _lastCode = null;
            _lastScanTime = DateTime.MinValue;
        }
    }
}
