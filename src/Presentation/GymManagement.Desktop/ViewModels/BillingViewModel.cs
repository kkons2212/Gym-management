using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;
using GymManagement.Core.Enums;

namespace GymManagement.Desktop.ViewModels;

public partial class BillingViewModel : ObservableObject
{
    private readonly IBillingService _billingService;

    [ObservableProperty]
    private string _memberCodeInput = "MB-0248";

    [ObservableProperty]
    private string _selectedServiceSummary = "Gói tập Premium 12 tháng";

    [ObservableProperty]
    private double _totalAmount = 6900000;

    [ObservableProperty]
    private double _discountAmount = 500000;

    public double FinalAmount => TotalAmount - DiscountAmount;

    [ObservableProperty]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.VietQR;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<InvoiceDto> InvoicesList { get; } = [];

    public BillingViewModel(IBillingService billingService)
    {
        _billingService = billingService;
        _ = LoadInvoicesAsync();
    }

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        var list = await _billingService.GetInvoicesAsync();
        InvoicesList.Clear();
        foreach (var inv in list)
        {
            InvoicesList.Add(inv);
        }
    }

    [RelayCommand]
    public async Task CreateInvoiceAsync()
    {
        var inv = await _billingService.CreateInvoiceAsync(MemberCodeInput, SelectedServiceSummary, (decimal)TotalAmount, (decimal)DiscountAmount, SelectedPaymentMethod);
        InvoicesList.Insert(0, inv);
        StatusMessage = $"Đã lập hóa đơn {inv.InvoiceCode} thành công!";
    }

    [RelayCommand]
    public Task OpenRefundDialogAsync(InvoiceDto invoice)
    {
        StatusMessage = $"Mở form xử lý hoàn tiền cho hóa đơn: {invoice.InvoiceCode} (UC05.3)...";
        return Task.CompletedTask;
    }
}
