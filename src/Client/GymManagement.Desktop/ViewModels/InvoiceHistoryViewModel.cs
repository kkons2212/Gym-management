using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Billing;

namespace GymManagement.Desktop.ViewModels;

public partial class InvoiceHistoryViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public ObservableCollection<InvoiceDto> Invoices { get; } = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedStatus = "ALL";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasStatusMessage
    {
        get
        {
            if (!string.IsNullOrEmpty(StatusMessage))
            {
                return true;
            }
            return false;
        }
    }

    partial void OnStatusMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasStatusMessage));
    }

    public InvoiceHistoryViewModel(IApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        _apiClient = apiClient;
    }

    public async Task InitializeAsync()
    {
        await LoadInvoicesAsync();
    }

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        IsLoading = true;
        StatusMessage = null;

        try
        {
            var list = await _apiClient.GetInvoicesAsync(SearchQuery);
            Invoices.Clear();

            foreach (var inv in list)
            {
                if (SelectedStatus == "ALL" || string.Equals(inv.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    Invoices.Add(inv);
                }
            }

            if (Invoices.Count == 0)
            {
                IsEmpty = true;
            }
            else
            {
                IsEmpty = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể tải lịch sử hóa đơn: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ExecuteRefundAsync(string invoiceCode, decimal refundAmount, string reason)
    {
        IsLoading = true;
        try
        {
            await _apiClient.RefundInvoiceAsync(invoiceCode, refundAmount, reason);
            StatusMessage = $"Đã hoàn tiền cho hóa đơn {invoiceCode} thành công!";
            await LoadInvoicesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi hoàn tiền: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

