namespace GymManagement.Server.Core.DTOs.Adapters;

/// <summary>
/// DTO chứa thông tin nội dung gửi Email.
/// </summary>
public record EmailMessageDto(
    string ToEmail,
    string Subject,
    string BodyHtml,
    string? PlainTextFallback = null,
    IReadOnlyList<EmailAttachmentDto>? Attachments = null
);

/// <summary>
/// Tệp đính kèm trong Email (ví dụ hóa đơn PDF).
/// </summary>
public record EmailAttachmentDto(
    string FileName,
    byte[] ContentBytes,
    string ContentType = "application/pdf"
);

/// <summary>
/// DTO chứa thông tin nội dung gửi SMS.
/// </summary>
public record SmsMessageDto(
    string PhoneNumber,
    string MessageContent
);

/// <summary>
/// Kết quả phản hồi sau khi gửi Email hoặc SMS.
/// </summary>
public record NotificationResult(
    bool Success,
    string Channel,
    string? MessageId = null,
    string? ErrorMessage = null
);

/// <summary>
/// Cấu hình thông số cổng gửi Email & SMS.
/// </summary>
public record NotificationOptions
{
    public const string SectionName = "Notification";

    public string SmtpHost { get; init; } = "smtp.gymmanagement.local";
    public int SmtpPort { get; init; } = 587;
    public string SmtpUsername { get; init; } = "noreply@gymmanagement.local";
    public string SmtpPassword { get; init; } = "";
    public string SenderEmail { get; init; } = "support@gymmanagement.com";
    public string SenderDisplayName { get; init; } = "Gym Management System";
    public bool EnableSsl { get; init; } = true;

    public string SmsGatewayUrl { get; init; } = "https://sms.gymmanagement.local/api";
    public string SmsApiKey { get; init; } = "";
    public string SmsSenderId { get; init; } = "GYM_NOTIFY";

    public bool IsSandbox { get; init; } = true;
}

