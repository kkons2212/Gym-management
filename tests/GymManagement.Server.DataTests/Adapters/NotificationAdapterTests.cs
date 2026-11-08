using FluentAssertions;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Data.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GymManagement.Server.DataTests.Adapters;

public class NotificationAdapterTests
{
    private readonly Mock<ILogger<NotificationAdapter>> _loggerMock;
    private readonly IOptions<NotificationOptions> _options;
    private readonly NotificationAdapter _adapter;

    public NotificationAdapterTests()
    {
        _loggerMock = new Mock<ILogger<NotificationAdapter>>();
        _options = Options.Create(new NotificationOptions
        {
            IsSandbox = true,
            SenderEmail = "noreply@gymmanagement.com",
            SenderDisplayName = "Gym Management Support"
        });

        _adapter = new NotificationAdapter(_options, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEmailAsync_InSandboxMode_ShouldSucceedWithoutCallingNetwork()
    {
        // Arrange
        var message = new EmailMessageDto(
            ToEmail: "member@example.com",
            Subject: "Thong bao kich hoat the tap",
            BodyHtml: "<p>The tap cua ban da duoc kich hoat</p>"
        );

        // Act
        var result = await _adapter.SendEmailAsync(message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Channel.Should().Be("Email");
        result.MessageId.Should().StartWith("SANDBOX-EMAIL-");
    }

    [Fact]
    public async Task SendSmsAsync_WhenCalled_ShouldLogAndReturnSuccess()
    {
        // Arrange
        var message = new SmsMessageDto(
            PhoneNumber: "0901234567",
            MessageContent: "[Gym] Nhac lich tap ca PT 15:00 ngay mai."
        );

        // Act
        var result = await _adapter.SendSmsAsync(message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Channel.Should().Be("SMS");
        result.MessageId.Should().StartWith("SMS-");
    }

    [Fact]
    public async Task SendInvoiceReceiptAsync_WhenValidParams_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = await _adapter.SendInvoiceReceiptAsync(
            toEmail: "customer@example.com",
            invoiceCode: "INV-20261108-002",
            amount: 800000m,
            planName: "Goi Tap 30 Ngay",
            paidAt: DateTime.UtcNow
        );

        // Assert
        result.Success.Should().BeTrue();
        result.Channel.Should().Be("Email");
    }

    [Fact]
    public async Task SendPtSessionReminderAsync_WhenValidParams_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = await _adapter.SendPtSessionReminderAsync(
            toPhone: "0988776655",
            memberName: "Tran Thi B",
            ptName: "HLV Minh Hoang",
            sessionTime: DateTime.UtcNow.AddHours(3)
        );

        // Assert
        result.Success.Should().BeTrue();
        result.Channel.Should().Be("SMS");
    }
}

