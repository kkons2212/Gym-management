using System.Net;
using System.Net.Mail;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.DTOs.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymManagement.Server.Data.Adapters;

/// <summary>
/// Hiện thực Cổng SMS Sending Gateway & Email Provider hỗ trợ thông báo tự động và chế độ Sandbox.
/// </summary>
public class NotificationAdapter : INotificationAdapter
{
    // ====================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // ====================================================================
    private const string EmailChannel = "Email";
    private const string SmsChannel = "SMS";

    // ====================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // ====================================================================
    private readonly NotificationOptions _options;
    private readonly ILogger<NotificationAdapter> _logger;

    // ====================================================================
    // 3. CONSTRUCTOR
    // ====================================================================
    public NotificationAdapter(
        IOptions<NotificationOptions> options,
        ILogger<NotificationAdapter> logger)
    {
        _options = options?.Value ?? new NotificationOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ====================================================================
    // 4. PUBLIC METHODS (Interface INotificationAdapter Implementation)
    // ====================================================================

    /// <inheritdoc />
    public async Task<NotificationResult> SendEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        cancellationToken.ThrowIfCancellationRequested();

        if (_options.IsSandbox)
        {
            _logger.LogInformation("[SANDBOX EMAIL] Gửi tới: {ToEmail} | Tiêu đề: {Subject}", message.ToEmail, message.Subject);
            return new NotificationResult(true, EmailChannel, $"SANDBOX-EMAIL-{DateTime.UtcNow.Ticks}");
        }

        try
        {
            using var mail = new MailMessage();
            mail.From = new MailAddress(_options.SenderEmail, _options.SenderDisplayName);
            mail.To.Add(message.ToEmail);
            mail.Subject = message.Subject;
            mail.Body = message.BodyHtml;
            mail.IsBodyHtml = true;

            if (message.Attachments != null)
            {
                foreach (var att in message.Attachments)
                {
                    var stream = new MemoryStream(att.ContentBytes);
                    mail.Attachments.Add(new Attachment(stream, att.FileName, att.ContentType));
                }
            }

            using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort);
            smtp.EnableSsl = _options.EnableSsl;
            if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
            {
                smtp.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);
            }

            await smtp.SendMailAsync(mail, cancellationToken);

            _logger.LogInformation("Đã gửi email thành công tới: {ToEmail}", message.ToEmail);
            return new NotificationResult(true, EmailChannel, Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email tới: {ToEmail}", message.ToEmail);
            return new NotificationResult(false, EmailChannel, null, ex.Message);
        }
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendSmsAsync(SmsMessageDto message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        cancellationToken.ThrowIfCancellationRequested();

        // Mặc định hoặc chế độ Sandbox ghi nhận vào log
        _logger.LogInformation("[SMS GATEWAY] SĐT: {Phone} | Nội dung: {Content}", message.PhoneNumber, message.MessageContent);

        var result = new NotificationResult(
            Success: true,
            Channel: SmsChannel,
            MessageId: $"SMS-{DateTime.UtcNow.Ticks}"
        );

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendWelcomeEmailAsync(string toEmail, string memberName, string qrCodeDataUrl, CancellationToken cancellationToken = default)
    {
        string subject = "Chào mừng bạn đến với Hệ thống Phòng tập Thể thao!";
        string body = BuildWelcomeHtml(memberName, qrCodeDataUrl);

        var message = new EmailMessageDto(toEmail, subject, body);
        return SendEmailAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendInvoiceReceiptAsync(string toEmail, string invoiceCode, decimal amount, string planName, DateTime paidAt, CancellationToken cancellationToken = default)
    {
        string subject = $"Biên lai thanh toán hóa đơn #{invoiceCode} - Phòng tập thể thao";
        string body = BuildInvoiceReceiptHtml(invoiceCode, amount, planName, paidAt);

        var message = new EmailMessageDto(toEmail, subject, body);
        return SendEmailAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public Task<NotificationResult> SendPtSessionReminderAsync(string toPhone, string memberName, string ptName, DateTime sessionTime, CancellationToken cancellationToken = default)
    {
        string content = $"[Gym System] Nhắc lịch tập: Hội viên {memberName} có ca tập với HLV {ptName} vào lúc {sessionTime:HH:mm dd/MM/yyyy}. Vui lòng đến đúng giờ!";
        var message = new SmsMessageDto(toPhone, content);

        return SendSmsAsync(message, cancellationToken);
    }

    // ====================================================================
    // 5. PRIVATE HELPER METHODS (Đặt ở cuối file)
    // ====================================================================

    private static string BuildWelcomeHtml(string memberName, string qrCodeDataUrl)
    {
        return $@"
        <div style='font-family: Arial, sans-serif; background: #0F172A; color: #F8FAFC; padding: 24px; border-radius: 12px;'>
            <h2 style='color: #0EA5E9;'>Chào mừng hội viên {WebUtility.HtmlEncode(memberName)}!</h2>
            <p>Tài khoản của bạn đã được kích hoạt thành công trên Hệ thống Quản lý Phòng tập.</p>
            <p>Dưới đây là liên kết thẻ tập và mã QR điểm danh của bạn:</p>
            <div style='text-align: center; margin: 20px 0;'>
                <img src='{WebUtility.HtmlEncode(qrCodeDataUrl)}' alt='QR Check-in' style='width: 220px; height: 220px; border-radius: 8px;' />
            </div>
            <p style='color: #94A3B8; font-size: 13px;'>Vui lòng xuất trình mã này tại quầy lễ tân để check-in vào phòng tập.</p>
        </div>";
    }

    private static string BuildInvoiceReceiptHtml(string invoiceCode, decimal amount, string planName, DateTime paidAt)
    {
        return $@"
        <div style='font-family: Arial, sans-serif; background: #0F172A; color: #F8FAFC; padding: 24px; border-radius: 12px;'>
            <h2 style='color: #16A34A;'>Biên Lai Thanh Toán Hóa Đơn #{WebUtility.HtmlEncode(invoiceCode)}</h2>
            <table style='width: 100%; border-collapse: collapse; margin-top: 16px; color: #F8FAFC;'>
                <tr style='border-bottom: 1px solid #334155;'>
                    <td style='padding: 8px 0; color: #94A3B8;'>Gói dịch vụ:</td>
                    <td style='padding: 8px 0; font-weight: bold;'>{WebUtility.HtmlEncode(planName)}</td>
                </tr>
                <tr style='border-bottom: 1px solid #334155;'>
                    <td style='padding: 8px 0; color: #94A3B8;'>Số tiền thanh toán:</td>
                    <td style='padding: 8px 0; font-weight: bold; color: #0EA5E9;'>{amount:N0} đ</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; color: #94A3B8;'>Thời gian giao dịch:</td>
                    <td style='padding: 8px 0;'>{paidAt:HH:mm dd/MM/yyyy}</td>
                </tr>
            </table>
            <p style='margin-top: 20px; font-size: 13px; color: #94A3B8;'>Cảm ơn quý khách đã đồng hành cùng phòng tập!</p>
        </div>";
    }
}

