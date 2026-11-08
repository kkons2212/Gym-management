using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;

namespace GymManagement.Desktop.ViewModels;

public partial class PosBillingViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public ObservableCollection<PackagePlanDto> Packages { get; } = [];
    public ObservableCollection<MemberDto> Members { get; } = [];

    [ObservableProperty]
    private PackagePlanDto? _selectedPackage;

    public string SelectedPackageName => SelectedPackage?.Name ?? "Chưa chọn gói";
    public string SelectedPackageDescription => SelectedPackage?.Description ?? string.Empty;
    public int SelectedPackageDurationDays => SelectedPackage?.DurationDays ?? 0;
    public int SelectedPackageMaxCheckInSessions => SelectedPackage?.MaxCheckInSessions ?? 0;
    public int SelectedPackageTotalPtSessions => SelectedPackage?.TotalPtSessions ?? 0;

    partial void OnSelectedPackageChanged(PackagePlanDto? value)
    {
        OnPropertyChanged(nameof(SelectedPackageName));
        OnPropertyChanged(nameof(SelectedPackageDescription));
        OnPropertyChanged(nameof(SelectedPackageDurationDays));
        OnPropertyChanged(nameof(SelectedPackageMaxCheckInSessions));
        OnPropertyChanged(nameof(SelectedPackageTotalPtSessions));
    }

    [ObservableProperty]
    private MemberDto? _selectedMember;

    [ObservableProperty]
    private string _memberSearchText = string.Empty;

    [ObservableProperty]
    private decimal _subtotalAmount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private string _selectedPaymentMethod = "VIETQR"; // CASH, VIETQR, CREDIT_CARD

    [ObservableProperty]
    private string _transactionType = "SUBSCRIPTION_PURCHASE"; // SUBSCRIPTION_PURCHASE, SUBSCRIPTION_RENEWAL

    [ObservableProperty]
    private bool _isVietQrModalOpen;

    [ObservableProperty]
    private VietQrPayloadDto? _vietQrPayload;

    [ObservableProperty]
    private InvoiceDto? _createdInvoice;

    [ObservableProperty]
    private bool _isInvoiceCreated;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    public PosBillingViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var pkgs = await _apiClient.GetPackagePlansAsync();
            Packages.Clear();
            foreach (var p in pkgs)
            {
                Packages.Add(p);
            }

            var mems = await _apiClient.GetMembersAsync();
            Members.Clear();
            foreach (var m in mems)
            {
                Members.Add(m);
            }

            if (Packages.Count > 0)
            {
                SelectPackage(Packages[0]);
            }

            if (Members.Count > 0)
            {
                SelectedMember = Members[0];
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SelectPackage(PackagePlanDto package)
    {
        SelectedPackage = package;
        SubtotalAmount = package.Price;
        TotalAmount = package.Price; // Exactly equal, 0 discount
    }

    [RelayCommand]
    private async Task ProcessCheckoutAsync()
    {
        if (SelectedMember == null || SelectedPackage == null)
        {
            StatusMessage = "Vui lòng chọn hội viên và gói tập!";
            return;
        }

        IsLoading = true;
        StatusMessage = null;

        try
        {
            if (SelectedPaymentMethod == "VIETQR")
            {
                var content = $"GYM {SelectedMember.UserId} {SelectedPackage.PlanId}";
                VietQrPayload = await _apiClient.GenerateVietQrAsync("TEMP", TotalAmount, content);
                IsVietQrModalOpen = true;
            }
            else
            {
                await FinalizeInvoiceAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task FinalizeInvoiceAsync()
    {
        if (SelectedMember == null || SelectedPackage == null)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var req = new CreateInvoiceRequest(
                SelectedMember.UserId,
                SelectedPackage.PlanId,
                SelectedPaymentMethod,
                TransactionType
            );

            CreatedInvoice = await _apiClient.CreateInvoiceAsync(req);
            IsInvoiceCreated = true;
            IsVietQrModalOpen = false;
            StatusMessage = $"Đã tạo hóa đơn {CreatedInvoice.InvoiceCode} thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi hoàn tất: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
