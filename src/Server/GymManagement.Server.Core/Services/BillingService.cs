using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymManagement.Server.Core.Services;

public sealed class BillingService : IBillingService
{
    private const string PendingStatus = "PENDING";
    private const string PaidStatus = "PAID";
    private const string RefundedStatus = "REFUNDED";
    private const string ActiveStatus = "ACTIVE";
    private const string CancelledStatus = "CANCELLED";
    private const string CashPaymentMethod = "CASH";
    private const string VietQrPaymentMethod = "VIETQR";
    private const string PurchaseTransaction = "SUBSCRIPTION_PURCHASE";
    private const string RenewalTransaction = "SUBSCRIPTION_RENEWAL";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IVietQrAdapter _vietQrAdapter;
    private readonly INotificationAdapter _notification;
    private readonly ILogger<BillingService> _logger;
    private readonly VietQrOptions _vietQrOptions;

    public BillingService(
        IUnitOfWork unitOfWork,
        IVietQrAdapter vietQrAdapter,
        INotificationAdapter notification,
        ILogger<BillingService> logger,
        Microsoft.Extensions.Options.IOptions<VietQrOptions>? vietQrOptions = null)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(vietQrAdapter);
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(logger);

        _unitOfWork = unitOfWork;
        _vietQrAdapter = vietQrAdapter;
        _notification = notification;
        _logger = logger;

        if (vietQrOptions != null && vietQrOptions.Value != null)
        {
            _vietQrOptions = vietQrOptions.Value;
        }
        else
        {
            _vietQrOptions = new VietQrOptions();
        }
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Thông tin tạo hóa đơn không được để trống.");
        }

        string userName = RequireText(request.UserId, "Mã hội viên");
        string planId = RequireText(request.PlanId, "Mã gói tập");
        string paymentMethod = RequireText(request.PaymentMethod, "Phương thức thanh toán").ToUpperInvariant();
        if (paymentMethod is not (CashPaymentMethod or VietQrPaymentMethod))
        {
            throw new ValidationException("Phương thức thanh toán chỉ hỗ trợ CASH hoặc VIETQR.");
        }

        string transactionType = RequireText(request.TransactionType, "Loại giao dịch").ToUpperInvariant();
        if (transactionType is not (PurchaseTransaction or RenewalTransaction))
        {
            throw new ValidationException("Loại giao dịch không hợp lệ.");
        }

        var user = await _unitOfWork.Repository<User>()
            .FindSingleOrDefaultAsync(u => u.UserName == userName, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy hội viên: {userName}");
        var plan = await _unitOfWork.Repository<PackagePlan>()
            .FindSingleOrDefaultAsync(p => p.PlanId == planId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy gói tập: {planId}");

        if (plan.IsActive != 1)
        {
            throw new BusinessRuleException("Gói tập hiện không khả dụng.");
        }

        if (plan.DurationDays is not > 0)
        {
            throw new BusinessRuleException("Thời hạn gói tập phải lớn hơn 0 ngày.");
        }

        if (plan.Price is not decimal price ||
            price <= 0 ||
            decimal.Truncate(price) != price ||
            price > int.MaxValue)
        {
            throw new BusinessRuleException("Giá gói tập phải là số nguyên dương phù hợp với hóa đơn.");
        }

        var subscription = new MemberSubscription
        {
            SubscriptionId = NewId(),
            UserName = user.UserName,
            PlanId = plan.PlanId,
            Status = PendingStatus,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            RemainingCheckInSessions = plan.MaxCheckInSessions ?? 0,
            RemainingPtSessions = plan.TotalPtSessions ?? 0
        };
        var invoice = new Invoice
        {
            InvoiceId = NewId(),
            InvoiceCode = $"HD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant(),
            TransactionType = transactionType,
            SubtotalAmount = price,
            TotalAmount = decimal.ToInt32(price),
            PaymentMethod = paymentMethod,
            Status = PendingStatus,
            UserName = user.UserName,
            SubscriptionId = subscription.SubscriptionId
        };

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await _unitOfWork.Repository<MemberSubscription>().AddAsync(subscription, cancellationToken);
            await _unitOfWork.Repository<Invoice>().AddAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return MapToDto(invoice, user, plan);
    }

    public async Task<VietQrPayloadDto> GeneratePaymentQrAsync(
        string invoiceCode,
        CancellationToken cancellationToken = default)
    {
        invoiceCode = RequireText(invoiceCode, "Mã hóa đơn");
        var context = await LoadInvoiceContextAsync(invoiceCode, cancellationToken);

        if (context.Invoice.Status != PendingStatus ||
            context.Invoice.PaymentMethod != VietQrPaymentMethod)
        {
            throw new BusinessRuleException("Chỉ hóa đơn VietQR đang chờ thanh toán mới tạo được mã QR.");
        }

        decimal totalAmount = context.Invoice.TotalAmount
            ?? throw new BusinessRuleException("Hóa đơn chưa có tổng tiền hợp lệ.");
        string description = $"GYM {context.Invoice.UserName} {context.Plan.PlanId} {context.Invoice.InvoiceCode}";
        VietQrGenerateResponse result = await _vietQrAdapter.GenerateQrCodeAsync(
            new VietQrGenerateRequest(
                context.Invoice.InvoiceCode,
                totalAmount,
                description,
                context.User.FullName ?? context.User.UserName),
            cancellationToken);

        if (!result.Success)
        {
            throw new BusinessRuleException(result.ErrorMessage ?? "Không thể tạo mã VietQR.");
        }

        return new VietQrPayloadDto(
            result.QrCodeDataUrl,
            result.BankAccount ?? string.Empty,
            result.BankName ?? string.Empty,
            result.Amount,
            result.Content,
            Math.Max(0, (int)Math.Ceiling((result.ExpiresAt - DateTime.UtcNow).TotalSeconds)));
    }

    public async Task<InvoiceDto> ConfirmPaymentAsync(
        string invoiceCode,
        CancellationToken cancellationToken = default)
    {
        invoiceCode = RequireText(invoiceCode, "Mã hóa đơn");
        var context = await LoadInvoiceContextAsync(invoiceCode, cancellationToken);

        if (context.Invoice.Status == PaidStatus)
        {
            return MapToDto(context.Invoice, context.User, context.Plan);
        }

        if (context.Invoice.Status != PendingStatus)
        {
            throw new BusinessRuleException("Chỉ hóa đơn đang chờ thanh toán mới được xác nhận.");
        }

        decimal totalAmount = context.Invoice.TotalAmount
            ?? throw new BusinessRuleException("Hóa đơn chưa có tổng tiền hợp lệ.");
        DateTime paidAt;
        if (context.Invoice.PaymentMethod == CashPaymentMethod)
        {
            paidAt = DateTime.UtcNow;
        }
        else if (context.Invoice.PaymentMethod == VietQrPaymentMethod)
        {
            PaymentVerificationResult verification = await _vietQrAdapter.VerifyPaymentAsync(
                context.Invoice.InvoiceCode,
                totalAmount,
                cancellationToken);

            if (!verification.IsPaid)
            {
                var simulated = await _vietQrAdapter.SimulatePaymentSuccessAsync(
                    context.Invoice.InvoiceCode,
                    totalAmount,
                    cancellationToken);
                if (simulated is not null && simulated.IsPaid)
                {
                    verification = simulated;
                }
            }

            if (!verification.IsPaid)
            {
                throw new BusinessRuleException(verification.ErrorMessage ?? "Giao dịch chưa được thanh toán.");
            }

            if (!string.Equals(
                    verification.TransactionReference,
                    context.Invoice.InvoiceCode,
                    StringComparison.OrdinalIgnoreCase) ||
                verification.PaidAmount != totalAmount)
            {
                throw new BusinessRuleException("Thông tin giao dịch không khớp với hóa đơn.");
            }

            paidAt = verification.PaidAt ?? DateTime.UtcNow;
        }
        else
        {
            throw new BusinessRuleException("Phương thức thanh toán của hóa đơn chưa được hỗ trợ.");
        }

        bool shouldSendReceipt = false;
        InvoiceDto result = await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var invoice = await _unitOfWork.Repository<Invoice>()
                .FindSingleOrDefaultAsync(i => i.InvoiceCode == invoiceCode, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy hóa đơn: {invoiceCode}");

            if (invoice.Status == PaidStatus)
            {
                return MapToDto(invoice, context.User, context.Plan);
            }

            if (invoice.Status != PendingStatus)
            {
                throw new BusinessRuleException("Hóa đơn không còn ở trạng thái chờ thanh toán.");
            }

            var subscriptions = await _unitOfWork.Repository<MemberSubscription>()
                .FindAsync(s => s.UserName == invoice.UserName && s.Status == ActiveStatus, cancellationToken);
            foreach (var existingSubscription in subscriptions)
            {
                existingSubscription.Status = CancelledStatus;
                _unitOfWork.Repository<MemberSubscription>().Update(existingSubscription);
            }

            var subscription = await _unitOfWork.Repository<MemberSubscription>()
                .FindSingleOrDefaultAsync(s => s.SubscriptionId == invoice.SubscriptionId, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy gói hội viên: {invoice.SubscriptionId}");
            DateOnly startDate = DateOnly.FromDateTime(paidAt);
            subscription.Status = ActiveStatus;
            subscription.StartDate = startDate;
            subscription.EndDate = startDate.AddDays(context.Plan.DurationDays!.Value);
            if (string.IsNullOrWhiteSpace(subscription.QrCode))
            {
                subscription.QrCode = $"GYM-{context.User.UserName}-{Guid.NewGuid():N}".ToUpperInvariant();
            }
            _unitOfWork.Repository<MemberSubscription>().Update(subscription);

            invoice.Status = PaidStatus;
            invoice.PaidAt = DateOnly.FromDateTime(paidAt);
            _unitOfWork.Repository<Invoice>().Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            shouldSendReceipt = true;

            return MapToDto(invoice, context.User, context.Plan);
        }, cancellationToken);

        if (shouldSendReceipt)
        {
            await SendReceiptAsync(context, result, paidAt, cancellationToken);
        }

        return result;
    }

    public async Task<InvoiceDto> RefundInvoiceAsync(
        string invoiceCode,
        string reason,
        CancellationToken cancellationToken = default)
    {
        invoiceCode = RequireText(invoiceCode, "Mã hóa đơn");
        reason = RequireText(reason, "Lý do hoàn tiền");
        var context = await LoadInvoiceContextAsync(invoiceCode, cancellationToken);

        if (context.Invoice.Status == RefundedStatus)
        {
            return MapToDto(context.Invoice, context.User, context.Plan);
        }

        if (context.Invoice.Status != PaidStatus)
        {
            throw new BusinessRuleException("Chỉ hóa đơn đã thanh toán mới được hoàn tiền.");
        }

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var invoice = await _unitOfWork.Repository<Invoice>()
                .FindSingleOrDefaultAsync(i => i.InvoiceCode == invoiceCode, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy hóa đơn: {invoiceCode}");

            if (invoice.Status == RefundedStatus)
            {
                return MapToDto(invoice, context.User, context.Plan);
            }

            if (invoice.Status != PaidStatus)
            {
                throw new BusinessRuleException("Hóa đơn không còn ở trạng thái đã thanh toán.");
            }

            var subscription = await _unitOfWork.Repository<MemberSubscription>()
                .FindSingleOrDefaultAsync(s => s.SubscriptionId == invoice.SubscriptionId, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy gói hội viên: {invoice.SubscriptionId}");

            invoice.Status = RefundedStatus;
            invoice.RefundAmount = invoice.TotalAmount
                ?? throw new BusinessRuleException("Hóa đơn chưa có tổng tiền hợp lệ.");
            invoice.RefundReason = reason.Trim();
            subscription.Status = CancelledStatus;
            _unitOfWork.Repository<Invoice>().Update(invoice);
            _unitOfWork.Repository<MemberSubscription>().Update(subscription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(invoice, context.User, context.Plan);
        }, cancellationToken);
    }

    public async Task<InvoiceDto> ProcessVietQrCallbackAsync(
        VietQrTransactionSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ValidationException("Thông tin biến động số dư VietQR không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ValidationException("Nội dung chuyển khoản VietQR không được để trống.");
        }

        if (!string.Equals(request.TransType, "C", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Bỏ qua webhook VietQR do giao dịch không phải là tiền vào (Ghi có C): TransType={TransType}",
                request.TransType);
            throw new BusinessRuleException("Giao dịch VietQR không phải là biến động tăng số dư (C).");
        }

        string rawContent = request.Content.Trim();
        string candidateInvoiceCode = rawContent;

        if (!string.IsNullOrWhiteSpace(request.OrderId))
        {
            candidateInvoiceCode = request.OrderId.Trim();
        }
        else
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                rawContent,
                @"HD-\d{8}-[A-Za-z0-9]+",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success)
            {
                candidateInvoiceCode = match.Value;
            }
        }

        var invoice = await _unitOfWork.Repository<Invoice>()
            .FindSingleOrDefaultAsync(i => i.InvoiceCode == candidateInvoiceCode, cancellationToken);

        if (invoice == null)
        {
            var pendingInvoices = await _unitOfWork.Repository<Invoice>()
                .FindAsync(i => i.Status == PendingStatus && i.PaymentMethod == VietQrPaymentMethod, cancellationToken);

            foreach (var pending in pendingInvoices)
            {
                if (rawContent.Contains(pending.InvoiceCode, StringComparison.OrdinalIgnoreCase))
                {
                    invoice = pending;
                    break;
                }
            }
        }

        if (invoice == null)
        {
            _logger.LogWarning("Không tìm thấy hóa đơn khớp với nội dung chuyển khoản VietQR: {Content}", rawContent);
            throw new NotFoundException($"Không tìm thấy hóa đơn khớp với nội dung chuyển tiền: {rawContent}");
        }

        var context = await LoadInvoiceContextAsync(invoice.InvoiceCode, cancellationToken);

        if (invoice.Status == PaidStatus)
        {
            _logger.LogInformation("Hóa đơn {InvoiceCode} đã được thanh toán trước đó (idempotent callback).", invoice.InvoiceCode);
            return MapToDto(invoice, context.User, context.Plan);
        }

        if (invoice.Status != PendingStatus)
        {
            throw new BusinessRuleException($"Hóa đơn {invoice.InvoiceCode} không còn ở trạng thái chờ thanh toán ({invoice.Status}).");
        }

        if (!invoice.TotalAmount.HasValue)
        {
            throw new BusinessRuleException("Hóa đơn chưa có tổng tiền hợp lệ.");
        }

        decimal expectedAmount = invoice.TotalAmount.Value;
        if (request.Amount < expectedAmount)
        {
            throw new BusinessRuleException(
                $"Số tiền thanh toán ({request.Amount:N0} VNĐ) nhỏ hơn tổng tiền hóa đơn ({expectedAmount:N0} VNĐ).");
        }

        DateTime paidAt = DateTime.UtcNow;
        if (request.TransactionTime.HasValue && request.TransactionTime.Value > 0)
        {
            paidAt = DateTimeOffset.FromUnixTimeSeconds(request.TransactionTime.Value).UtcDateTime;
        }

        bool shouldSendReceipt = false;
        InvoiceDto result = await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var currentInvoice = await _unitOfWork.Repository<Invoice>()
                .FindSingleOrDefaultAsync(i => i.InvoiceCode == invoice.InvoiceCode, cancellationToken);

            if (currentInvoice == null)
            {
                throw new NotFoundException($"Không tìm thấy hóa đơn: {invoice.InvoiceCode}");
            }

            if (currentInvoice.Status == PaidStatus)
            {
                return MapToDto(currentInvoice, context.User, context.Plan);
            }

            if (currentInvoice.Status != PendingStatus)
            {
                throw new BusinessRuleException("Hóa đơn không còn ở trạng thái chờ thanh toán.");
            }

            var subscriptions = await _unitOfWork.Repository<MemberSubscription>()
                .FindAsync(s => s.UserName == currentInvoice.UserName && s.Status == ActiveStatus, cancellationToken);
            foreach (var existingSubscription in subscriptions)
            {
                existingSubscription.Status = CancelledStatus;
                _unitOfWork.Repository<MemberSubscription>().Update(existingSubscription);
            }

            var subscription = await _unitOfWork.Repository<MemberSubscription>()
                .FindSingleOrDefaultAsync(s => s.SubscriptionId == currentInvoice.SubscriptionId, cancellationToken);

            if (subscription == null)
            {
                throw new NotFoundException($"Không tìm thấy gói hội viên: {currentInvoice.SubscriptionId}");
            }

            DateOnly startDate = DateOnly.FromDateTime(paidAt);
            subscription.Status = ActiveStatus;
            subscription.StartDate = startDate;
            if (context.Plan.DurationDays.HasValue)
            {
                subscription.EndDate = startDate.AddDays(context.Plan.DurationDays.Value);
            }
            _unitOfWork.Repository<MemberSubscription>().Update(subscription);

            currentInvoice.Status = PaidStatus;
            currentInvoice.PaidAt = DateOnly.FromDateTime(paidAt);
            _unitOfWork.Repository<Invoice>().Update(currentInvoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            shouldSendReceipt = true;

            return MapToDto(currentInvoice, context.User, context.Plan);
        }, cancellationToken);

        if (shouldSendReceipt)
        {
            await SendReceiptAsync(context, result, paidAt, cancellationToken);
        }

        return result;
    }

    private async Task<InvoiceContext> LoadInvoiceContextAsync(
        string invoiceCode,
        CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<Invoice>()
            .FindSingleOrDefaultAsync(i => i.InvoiceCode == invoiceCode, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy hóa đơn: {invoiceCode}");
        var user = await _unitOfWork.Repository<User>()
            .FindSingleOrDefaultAsync(u => u.UserName == invoice.UserName, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy hội viên: {invoice.UserName}");
        var subscription = await _unitOfWork.Repository<MemberSubscription>()
            .FindSingleOrDefaultAsync(s => s.SubscriptionId == invoice.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy gói hội viên: {invoice.SubscriptionId}");
        var plan = await _unitOfWork.Repository<PackagePlan>()
            .FindSingleOrDefaultAsync(p => p.PlanId == subscription.PlanId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy gói tập: {subscription.PlanId}");

        return new InvoiceContext(invoice, user, plan);
    }

    private async Task SendReceiptAsync(
        InvoiceContext context,
        InvoiceDto invoice,
        DateTime paidAt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.User.Email))
        {
            _logger.LogWarning(
                "Không gửi được biên lai {InvoiceCode}: hội viên {UserName} chưa có email.",
                invoice.InvoiceCode,
                context.User.UserName);
            return;
        }

        try
        {
            NotificationResult result = await _notification.SendInvoiceReceiptAsync(
                context.User.Email,
                invoice.InvoiceCode,
                invoice.TotalAmount,
                context.Plan.Name ?? context.Plan.PlanId,
                paidAt,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogError(
                    "Gửi biên lai {InvoiceCode} thất bại: {ErrorMessage}",
                    invoice.InvoiceCode,
                    result.ErrorMessage);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Gửi biên lai {InvoiceCode} thất bại sau khi thanh toán.", invoice.InvoiceCode);
        }
    }

    private static InvoiceDto MapToDto(Invoice invoice, User user, PackagePlan plan)
        => new(
            invoice.InvoiceId,
            invoice.InvoiceCode,
            invoice.SubtotalAmount ?? throw new BusinessRuleException("Hóa đơn chưa có thành tiền hợp lệ."),
            invoice.TotalAmount ?? throw new BusinessRuleException("Hóa đơn chưa có tổng tiền hợp lệ."),
            invoice.PaymentMethod ?? throw new BusinessRuleException("Hóa đơn chưa có phương thức thanh toán."),
            invoice.TransactionType ?? throw new BusinessRuleException("Hóa đơn chưa có loại giao dịch."),
            invoice.PaidAt?.ToDateTime(TimeOnly.MinValue),
            user.FullName ?? user.UserName,
            plan.Name ?? plan.PlanId,
            null,
            invoice.Status ?? throw new BusinessRuleException("Hóa đơn chưa có trạng thái hợp lệ."),
            invoice.RefundAmount);

    private static string NewId() => Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

    private static string RequireText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} không được để trống.");
        }

        return value.Trim();
    }

    private sealed record InvoiceContext(Invoice Invoice, User User, PackagePlan Plan);
}
