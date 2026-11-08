using GymManagement.Server.Core.DTOs.Adapters;

namespace GymManagement.Server.Core.Contracts.Adapters;

/// <summary>
/// Hợp đồng Adapter tích hợp cổng gửi Email và SMS tự động.
/// </summary>
public interface INotificationAdapter
{
    /// <summary>
    /// Gửi Email trực tiếp.
    /// </summary>
    Task<NotificationResult> SendEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi tin nhắn SMS trực tiếp.
    /// </summary>
    Task<NotificationResult> SendSmsAsync(SmsMessageDto message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi Email chào mừng hội viên kèm thông tin thẻ tập.
    /// </summary>
    Task<NotificationResult> SendWelcomeEmailAsync(string toEmail, string memberName, string qrCodeDataUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi Email biên lai hóa đơn điện tử sau khi thanh toán gói tập thành công.
    /// </summary>
    Task<NotificationResult> SendInvoiceReceiptAsync(string toEmail, string invoiceCode, decimal amount, string planName, DateTime paidAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi SMS nhắc lịch hẹn tập luyện cùng Huấn luyện viên cá nhân (PT).
    /// </summary>
    Task<NotificationResult> SendPtSessionReminderAsync(string toPhone, string memberName, string ptName, DateTime sessionTime, CancellationToken cancellationToken = default);
}

