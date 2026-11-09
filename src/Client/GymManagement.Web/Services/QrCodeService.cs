using QRCoder;

namespace GymManagement.Web.Services;

/// <summary>
/// Giao diện dịch vụ sinh mã QR chuẩn quốc tế.
/// </summary>
public interface IQrCodeService
{
    string GeneratePngBase64(string payload, int pixelsPerModule = 10);
    string GenerateSvg(string payload, int pixelsPerModule = 10);
}

/// <summary>
/// Dịch vụ sinh mã QR chuẩn quốc tế cho hội viên bằng thư viện QRCoder.
/// Tạo mã tĩnh dạng Base64 PNG hoặc SVG hỗ trợ tải về máy 1 lần.
/// </summary>
public sealed class QrCodeService : IQrCodeService
{
    string IQrCodeService.GeneratePngBase64(string payload, int pixelsPerModule) => GeneratePngBase64(payload, pixelsPerModule);
    string IQrCodeService.GenerateSvg(string payload, int pixelsPerModule) => GenerateSvg(payload, pixelsPerModule);

    /// <summary>
    /// Sinh chuỗi Data URL ảnh PNG (base64) từ mã định danh hội viên.
    /// Cho phép hiển thị trực tiếp trong thẻ &lt;img&gt; và tải về máy.
    /// Hỗ trợ gọi tĩnh trực tiếp hoặc thông qua DI IQrCodeService.
    /// </summary>
    public static string GeneratePngBase64(string payload, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }

    /// <summary>
    /// Sinh mã vector SVG từ mã định danh hội viên.
    /// </summary>
    public static string GenerateSvg(string payload, int pixelsPerModule = 10)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new SvgQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule);
    }
}

