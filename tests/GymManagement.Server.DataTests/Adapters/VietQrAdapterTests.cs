using FluentAssertions;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Data.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GymManagement.Server.DataTests.Adapters;

public class VietQrAdapterTests
{
    private readonly Mock<ILogger<VietQrAdapter>> _loggerMock;
    private readonly IOptions<VietQrOptions> _options;
    private readonly HttpClient _httpClient;
    private readonly VietQrAdapter _adapter;

    public VietQrAdapterTests()
    {
        _loggerMock = new Mock<ILogger<VietQrAdapter>>();
        _options = Options.Create(new VietQrOptions
        {
            BankBin = "970422",
            AccountNo = "0912345678",
            AccountName = "CONG TY TNHH PHONG TAP",
            IsSandbox = true
        });

        _httpClient = new HttpClient();
        _adapter = new VietQrAdapter(_httpClient, _options, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_WhenValidInput_ShouldReturnSuccessfulResponseWithQuickLink()
    {
        // Arrange
        var request = new VietQrGenerateRequest(
            InvoiceCode: "INV-20261108-001",
            Amount: 1200000m,
            Description: "Thanh toan goi tap 1 thang",
            CustomerName: "Nguyen Van A"
        );

        // Act
        var result = await _adapter.GenerateQrCodeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Amount.Should().Be(1200000m);
        result.TransactionReference.Should().Be("INV-20261108-001");
        result.QuickLink.Should().Contain("970422-0912345678-compact2.png");
        result.QuickLink.Should().Contain("amount=1200000");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateQrCodeAsync_WhenAmountIsZeroOrNegative_ShouldReturnFailure()
    {
        // Arrange
        var request = new VietQrGenerateRequest(
            InvoiceCode: "INV-INVALID-001",
            Amount: -50000m,
            Description: "So tien am",
            CustomerName: "Test User"
        );

        // Act
        var result = await _adapter.GenerateQrCodeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Số tiền thanh toán phải lớn hơn 0");
    }

    [Fact]
    public async Task SimulatePaymentSuccessAsync_InSandboxMode_ShouldReturnVerifiedPayment()
    {
        // Arrange
        string invoiceCode = "INV-SIMULATE-001";

        // Act
        var simulateResult = await _adapter.SimulatePaymentSuccessAsync(invoiceCode);
        var verifyResult = await _adapter.VerifyPaymentAsync(invoiceCode, 500000m);

        // Assert
        simulateResult.IsPaid.Should().BeTrue();
        simulateResult.TransactionReference.Should().Be(invoiceCode);
        simulateResult.BankTransactionId.Should().StartWith("SANDBOX-MB-");

        verifyResult.IsPaid.Should().BeTrue();
        verifyResult.TransactionReference.Should().Be(invoiceCode);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenNotPaidAndNotInSandboxCache_ShouldReturnPending()
    {
        // Arrange
        string invoiceCode = "INV-NOT-PAID-999";

        // Act
        var result = await _adapter.VerifyPaymentAsync(invoiceCode, 1000000m);

        // Assert
        result.IsPaid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("chưa được thanh toán");
    }
}

