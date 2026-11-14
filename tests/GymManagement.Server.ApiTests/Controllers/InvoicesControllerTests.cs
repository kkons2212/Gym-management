using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class InvoicesControllerTests
{
    private readonly Mock<IBillingService> _billingServiceMock = new();
    private readonly Mock<ILogger<InvoicesController>> _loggerMock = new();
    private readonly InvoicesController _sut;

    public InvoicesControllerTests()
    {
        _sut = new InvoicesController(_billingServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateInvoice_WhenValid_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateInvoiceRequest("MEM01", "PLAN01", "VIETQR", "SUBSCRIPTION_PURCHASE");
        var invoiceDto = new InvoiceDto(
            InvoiceId: "INV01",
            InvoiceCode: "INV-202611-0001",
            SubtotalAmount: 1000000m,
            TotalAmount: 1000000m,
            PaymentMethod: "VIETQR",
            TransactionType: "SUBSCRIPTION_PURCHASE",
            PaidAt: null,
            MemberName: "Nguyen Van A",
            PlanName: "Gói 1 Tháng",
            CreatedByAdminName: "ADMIN01",
            Status: "PENDING"
        );

        _billingServiceMock.Setup(s => s.CreateInvoiceAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceDto);

        // Act
        var result = await _sut.CreateInvoice(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<InvoiceDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.InvoiceCode.Should().Be("INV-202611-0001");
        response.Data.Status.Should().Be("PENDING");
    }

    [Fact]
    public async Task ConfirmPayment_WhenValid_ReturnsOkResponse()
    {
        // Arrange
        var invoiceDto = new InvoiceDto(
            InvoiceId: "INV01",
            InvoiceCode: "INV-202611-0001",
            SubtotalAmount: 1000000m,
            TotalAmount: 1000000m,
            PaymentMethod: "VIETQR",
            TransactionType: "SUBSCRIPTION_PURCHASE",
            PaidAt: DateTime.UtcNow,
            MemberName: "Nguyen Van A",
            PlanName: "Gói 1 Tháng",
            CreatedByAdminName: "ADMIN01",
            Status: "PAID"
        );

        _billingServiceMock.Setup(s => s.ConfirmPaymentAsync("INV-202611-0001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceDto);

        // Act
        var result = await _sut.ConfirmPayment("INV-202611-0001", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<InvoiceDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Status.Should().Be("PAID");
    }

    [Fact]
    public async Task GetInvoices_WhenCalled_ReturnsOkResponseWithList()
    {
        // Arrange
        var invoices = new List<InvoiceDto>
        {
            new(
                InvoiceId: "INV01",
                InvoiceCode: "INV-202611-0001",
                SubtotalAmount: 1000000m,
                TotalAmount: 1000000m,
                PaymentMethod: "VIETQR",
                TransactionType: "SUBSCRIPTION_PURCHASE",
                PaidAt: DateTime.UtcNow,
                MemberName: "Nguyen Van A",
                PlanName: "Gói 1 Tháng",
                CreatedByAdminName: "ADMIN01",
                Status: "PAID"
            )
        };

        _billingServiceMock.Setup(s => s.GetInvoicesAsync("INV", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);

        // Act
        var result = await _sut.GetInvoices("INV", null, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<InvoiceDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Count.Should().Be(1);
        response.Data[0].InvoiceCode.Should().Be("INV-202611-0001");
    }
}
