using System.Linq.Expressions;
using FluentAssertions;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.UnitTests.Services;

public sealed class BillingServiceTests
{
    private readonly List<Invoice> _invoices = [];
    private readonly List<MemberSubscription> _subscriptions = [];
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IVietQrAdapter> _vietQr = new();
    private readonly Mock<INotificationAdapter> _notification = new();
    private readonly Mock<ILogger<BillingService>> _logger = new();
    private readonly User _user = new()
    {
        UserName = "MEMBER01",
        RoleId = "MEMBER",
        FullName = "Test Member",
        Email = "member@example.test",
        Status = "ACTIVE"
    };
    private readonly PackagePlan _plan = new()
    {
        PlanId = "PLAN000001",
        Name = "Monthly",
        DurationDays = 30,
        Price = 1000,
        MaxCheckInSessions = 20,
        TotalPtSessions = 2,
        IsActive = 1
    };

    private readonly BillingService _sut;

    public BillingServiceTests()
    {
        SetupRepositories();
        _unitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<InvoiceDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<Task<InvoiceDto>> action, CancellationToken _) => action());
        _unitOfWork
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<Task> action, CancellationToken _) => action());
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _notification
            .Setup(n => n.SendInvoiceReceiptAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult(true, "Email"));

        _sut = new BillingService(
            _unitOfWork.Object,
            _vietQr.Object,
            _notification.Object,
            _logger.Object);
    }

    [Fact]
    public async Task CreateInvoiceAsync_WhenValidRequest_CreatesPendingInvoiceAndSubscription()
    {
        var result = await _sut.CreateInvoiceAsync(CreateRequest());

        result.Status.Should().Be("PENDING");
        result.TotalAmount.Should().Be(1000);
        _invoices.Should().ContainSingle();
        _invoices[0].UserName.Should().Be(_user.UserName);
        _subscriptions.Should().ContainSingle();
        _subscriptions[0].Status.Should().Be("PENDING");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GeneratePaymentQrAsync_WhenInvoiceIsPending_ReturnsGatewayDetails()
    {
        await _sut.CreateInvoiceAsync(CreateRequest(paymentMethod: "VIETQR"));
        var invoice = _invoices.Single();
        _vietQr
            .Setup(a => a.GenerateQrCodeAsync(
                It.IsAny<VietQrGenerateRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VietQrGenerateResponse(
                true,
                "https://example.test/qr",
                "https://example.test/qr",
                invoice.InvoiceCode,
                "GYM MEMBER01 PLAN000001",
                1000,
                DateTime.UtcNow.AddMinutes(10),
                BankAccount: "123456",
                BankName: "Example Bank"));

        var result = await _sut.GeneratePaymentQrAsync(invoice.InvoiceCode);

        result.BankAccount.Should().Be("123456");
        result.BankName.Should().Be("Example Bank");
        result.Amount.Should().Be(1000);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenCashPayment_ActivatesSubscriptionAndSendsReceipt()
    {
        await _sut.CreateInvoiceAsync(CreateRequest(paymentMethod: "CASH"));
        var invoice = _invoices.Single();

        var result = await _sut.ConfirmPaymentAsync(invoice.InvoiceCode);

        result.Status.Should().Be("PAID");
        invoice.Status.Should().Be("PAID");
        invoice.PaidAt.Should().NotBeNull();
        _subscriptions.Single().Status.Should().Be("ACTIVE");
        _notification.Verify(n => n.SendInvoiceReceiptAsync(
            _user.Email!,
            invoice.InvoiceCode,
            1000,
            _plan.Name!,
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_WhenVietQrAmountDoesNotMatch_RejectsPayment()
    {
        await _sut.CreateInvoiceAsync(CreateRequest(paymentMethod: "VIETQR"));
        var invoice = _invoices.Single();
        _vietQr
            .Setup(a => a.VerifyPaymentAsync(invoice.InvoiceCode, 1000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentVerificationResult(
                true,
                invoice.InvoiceCode,
                999,
                DateTime.UtcNow));

        Func<Task> act = () => _sut.ConfirmPaymentAsync(invoice.InvoiceCode);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*không khớp*");
        invoice.Status.Should().Be("PENDING");
        _subscriptions.Single().Status.Should().Be("PENDING");
    }

    [Fact]
    public async Task RefundInvoiceAsync_WhenPaid_RefundsInternallyAndCancelsSubscription()
    {
        await _sut.CreateInvoiceAsync(CreateRequest(paymentMethod: "CASH"));
        var invoice = _invoices.Single();
        await _sut.ConfirmPaymentAsync(invoice.InvoiceCode);

        var result = await _sut.RefundInvoiceAsync(invoice.InvoiceCode, "Customer request");

        result.Status.Should().Be("REFUNDED");
        result.RefundAmount.Should().Be(1000);
        invoice.RefundReason.Should().Be("Customer request");
        _subscriptions.Single().Status.Should().Be("CANCELLED");
        _vietQr.Verify(
            a => a.VerifyPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessVietQrCallbackAsync_WhenValidPayment_ActivatesSubscriptionAndMarksPaid()
    {
        var created = await _sut.CreateInvoiceAsync(CreateRequest("VIETQR"));
        var callbackReq = new VietQrTransactionSyncRequest(
            BankAccount: "0912345678",
            Amount: 1000,
            TransType: "C",
            Content: created.InvoiceCode
        );

        var result = await _sut.ProcessVietQrCallbackAsync(callbackReq);

        result.Status.Should().Be("PAID");
        result.PaidAt.Should().NotBeNull();
        _invoices.Single().Status.Should().Be("PAID");
        _subscriptions.Single().Status.Should().Be("ACTIVE");
    }

    [Fact]
    public async Task ProcessVietQrCallbackAsync_WhenDebitTransaction_ThrowsBusinessRuleException()
    {
        var created = await _sut.CreateInvoiceAsync(CreateRequest("VIETQR"));
        var callbackReq = new VietQrTransactionSyncRequest(
            BankAccount: "0912345678",
            Amount: 1000,
            TransType: "D",
            Content: created.InvoiceCode
        );

        var act = () => _sut.ProcessVietQrCallbackAsync(callbackReq);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task ProcessVietQrCallbackAsync_WhenUnderpaid_ThrowsBusinessRuleException()
    {
        var created = await _sut.CreateInvoiceAsync(CreateRequest("VIETQR"));
        var callbackReq = new VietQrTransactionSyncRequest(
            BankAccount: "0912345678",
            Amount: 500,
            TransType: "C",
            Content: created.InvoiceCode
        );

        var act = () => _sut.ProcessVietQrCallbackAsync(callbackReq);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    private CreateInvoiceRequest CreateRequest(string paymentMethod = "CASH")
        => new(_user.UserName, _plan.PlanId, paymentMethod, "SUBSCRIPTION_PURCHASE");

    private void SetupRepositories()
    {
        var invoiceRepository = new Mock<IGenericRepository<Invoice>>();
        invoiceRepository
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<Invoice, bool>> predicate, CancellationToken _) =>
                Task.FromResult(_invoices.SingleOrDefault(predicate.Compile())));
        invoiceRepository
            .Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns((Invoice invoice, CancellationToken _) =>
            {
                _invoices.Add(invoice);
                return Task.CompletedTask;
            });

        var subscriptionRepository = new Mock<IGenericRepository<MemberSubscription>>();
        subscriptionRepository
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<MemberSubscription, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<MemberSubscription, bool>> predicate, CancellationToken _) =>
                Task.FromResult(_subscriptions.SingleOrDefault(predicate.Compile())));
        subscriptionRepository
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<MemberSubscription, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<MemberSubscription, bool>> predicate, CancellationToken _) =>
                Task.FromResult<IReadOnlyList<MemberSubscription>>(
                    _subscriptions.Where(predicate.Compile()).ToList()));
        subscriptionRepository
            .Setup(r => r.AddAsync(It.IsAny<MemberSubscription>(), It.IsAny<CancellationToken>()))
            .Returns((MemberSubscription subscription, CancellationToken _) =>
            {
                _subscriptions.Add(subscription);
                return Task.CompletedTask;
            });

        var userRepository = new Mock<IGenericRepository<User>>();
        userRepository
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<User, bool>> predicate, CancellationToken _) =>
                Task.FromResult(predicate.Compile()(_user) ? _user : null));

        var planRepository = new Mock<IGenericRepository<PackagePlan>>();
        planRepository
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<PackagePlan, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<PackagePlan, bool>> predicate, CancellationToken _) =>
                Task.FromResult(predicate.Compile()(_plan) ? _plan : null));

        _unitOfWork.Setup(u => u.Repository<Invoice>()).Returns(invoiceRepository.Object);
        _unitOfWork.Setup(u => u.Repository<MemberSubscription>()).Returns(subscriptionRepository.Object);
        _unitOfWork.Setup(u => u.Repository<User>()).Returns(userRepository.Object);
        _unitOfWork.Setup(u => u.Repository<PackagePlan>()).Returns(planRepository.Object);
    }
}
