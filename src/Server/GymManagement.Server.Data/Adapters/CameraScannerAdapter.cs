using System.Text;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.DTOs.Adapters;
using Microsoft.Extensions.Logging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace GymManagement.Server.Data.Adapters;

/// <summary>
/// Hiện thực Cổng Camera Scanner giải mã hình ảnh mã QR điểm danh và thẩm định tính hợp lệ của token động (60s).
/// </summary>
public class CameraScannerAdapter : ICameraScannerAdapter
{
    // ====================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // ====================================================================
    public const string TokenPrefix = "GYM-";
    private const int DefaultValidDurationSeconds = 60;

    // ====================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // ====================================================================
    private readonly ILogger<CameraScannerAdapter> _logger;

    // ====================================================================
    // 3. CONSTRUCTOR
    // ====================================================================
    public CameraScannerAdapter(ILogger<CameraScannerAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ====================================================================
    // 4. PUBLIC METHODS (Interface ICameraScannerAdapter Implementation)
    // ====================================================================

    /// <inheritdoc />
    public Task<QrDecodeResult> DecodeQrCodeFromBytesAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageBytes);

        if (imageBytes.Length == 0)
        {
            return Task.FromResult(new QrDecodeResult(false, null, "Dữ liệu hình ảnh trống."));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Trường hợp 1: Nếu chuỗi byte truyền vào là văn bản thô (ví dụ quét trực tiếp từ cổng serial USB / barcode reader)
            if (IsPlainTextPayload(imageBytes, out string? plainText))
            {
                return Task.FromResult(new QrDecodeResult(true, plainText));
            }

            // Trường hợp 2: Thử giải mã luminance từ mảng byte bitmap sử dụng ZXing
            var luminanceSource = CreateLuminanceSourceFromBytes(imageBytes);
            if (luminanceSource != null)
            {
                var binarizer = new HybridBinarizer(luminanceSource);
                var bitmap = new BinaryBitmap(binarizer);
                var reader = new QRCodeReader();
                var result = reader.decode(bitmap);

                if (result != null && !string.IsNullOrWhiteSpace(result.Text))
                {
                    _logger.LogInformation("Giải mã thành công mã QR: {Payload}", result.Text);
                    return Task.FromResult(new QrDecodeResult(true, result.Text));
                }
            }

            return Task.FromResult(new QrDecodeResult(false, null, "Không tìm thấy hoặc không thể giải mã mã QR trong hình ảnh."));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi trong quá trình giải mã mã QR từ ảnh.");
            return Task.FromResult(new QrDecodeResult(false, null, $"Lỗi giải mã: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<QrDecodeResult> DecodeQrCodeFromStreamAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        cancellationToken.ThrowIfCancellationRequested();

        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream, cancellationToken);
        byte[] bytes = memoryStream.ToArray();

        return await DecodeQrCodeFromBytesAsync(bytes, cancellationToken);
    }

    /// <inheritdoc />
    public QrTokenValidationResult ValidateCheckInToken(string qrPayload, int validDurationSeconds = DefaultValidDurationSeconds)
    {
        if (string.IsNullOrWhiteSpace(qrPayload))
        {
            return new QrTokenValidationResult(false, null, null, false, "Chuỗi mã QR trống.");
        }

        string trimmedPayload = qrPayload.Trim();

        if (!trimmedPayload.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return new QrTokenValidationResult(false, null, null, false, "Mã QR không đúng định dạng của phòng gym (thiếu tiền tố GYM-).");
        }

        // Cấu trúc chuẩn: GYM-{UserId}-{TimestampTicks}[-{Signature}]
        // UserId có thể là GUID hoặc mã hội viên có dấu gạch nối (ví dụ MEMBER-999).
        string[] parts = trimmedPayload.Split('-');
        if (parts.Length < 3)
        {
            return new QrTokenValidationResult(false, null, null, false, "Cấu trúc token mã QR không hợp lệ (tối thiểu 3 phần).");
        }

        // Tìm vị trí của TimestampTicks (giá trị DateTime.Ticks hiện đại luôn có 18 chữ số > 600.000.000.000.000.000)
        int ticksIndex = -1;
        long ticks = 0;

        for (int i = 1; i < parts.Length; i++)
        {
            if (long.TryParse(parts[i], out long candidateTicks) && candidateTicks > 600_000_000_000_000_000L)
            {
                ticksIndex = i;
                ticks = candidateTicks;
                break;
            }
        }

        if (ticksIndex <= 1)
        {
            return new QrTokenValidationResult(false, null, null, false, "Không tìm thấy dấu thời gian hợp lệ trong token.");
        }

        // Toàn bộ các phần trước ticksIndex (từ index 1) là UserId
        string userId = string.Join("-", parts[1..ticksIndex]);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new QrTokenValidationResult(false, null, null, false, "Mã hội viên trong token không hợp lệ.");
        }

        DateTime tokenCreatedAt;
        try
        {
            tokenCreatedAt = new DateTime(ticks, DateTimeKind.Utc);
        }
        catch
        {
            return new QrTokenValidationResult(false, userId, null, false, "Dấu thời gian không chuyển đổi được sang ngày giờ UTC.");
        }

        DateTime now = DateTime.UtcNow;
        TimeSpan elapsed = now - tokenCreatedAt;

        // Cho phép dung sai lệch đồng hồ 5 giây
        if (elapsed.TotalSeconds < -5)
        {
            return new QrTokenValidationResult(false, userId, tokenCreatedAt, false, "Thời gian của token vượt quá thời gian thực hiện tại.");
        }

        bool isExpired = elapsed.TotalSeconds > validDurationSeconds;
        if (isExpired)
        {
            _logger.LogWarning("Mã QR của hội viên {UserId} đã hết hạn ({Elapsed}s > {MaxDuration}s)", userId, (int)elapsed.TotalSeconds, validDurationSeconds);
            return new QrTokenValidationResult(false, userId, tokenCreatedAt, true, $"Mã QR đã hết hiệu lực (tự động làm mới mỗi {validDurationSeconds} giây).");
        }

        _logger.LogInformation("Mã QR của hội viên {UserId} hợp lệ. Thời gian tạo: {CreatedAt}", userId, tokenCreatedAt);
        return new QrTokenValidationResult(true, userId, tokenCreatedAt, false, null);
    }

    // ====================================================================
    // 5. PRIVATE HELPER METHODS (Đặt ở cuối file)
    // ====================================================================

    private static bool IsPlainTextPayload(byte[] bytes, out string? text)
    {
        text = null;
        try
        {
            string candidate = Encoding.UTF8.GetString(bytes).Trim();
            if (candidate.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                text = candidate;
                return true;
            }
        }
        catch
        {
            // Không phải UTF-8 hợp lệ
        }

        return false;
    }

    private static LuminanceSource? CreateLuminanceSourceFromBytes(byte[] imageBytes)
    {
        // Kiểm tra xem có đủ dữ liệu cho một ma trận điểm ảnh thô không
        // Với ma trận vuông NxN grayscale
        int length = imageBytes.Length;
        int dimension = (int)Math.Sqrt(length);

        if (dimension * dimension == length && dimension >= 20)
        {
            return new PlanarYUVLuminanceSource(imageBytes, dimension, dimension, 0, 0, dimension, dimension, false);
        }

        return null;
    }
}
