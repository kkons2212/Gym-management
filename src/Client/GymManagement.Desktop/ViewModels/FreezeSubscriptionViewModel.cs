using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.ViewModels;

public partial class FreezeSubscriptionViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private DateTime _originalEndDate = DateTime.Today;

    [ObservableProperty]
    private string _memberId = string.Empty;

    [ObservableProperty]
    private string _memberName = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _freezeStartDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _freezeEndDate = DateTimeOffset.Now.AddDays(30);

    [ObservableProperty]
    private string _reason = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _projectedEndDate = DateTimeOffset.Now.AddDays(90);

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _message;

    public event Action? OnFreezeSuccess;

    public FreezeSubscriptionViewModel(IApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        _apiClient = apiClient;
    }

    public void Setup(MemberDto member)
    {
        ArgumentNullException.ThrowIfNull(member);

        MemberId = member.UserId;
        MemberName = member.FullName;
        FreezeStartDate = DateTimeOffset.Now;
        FreezeEndDate = DateTimeOffset.Now.AddDays(30);

        if (member.EndDate.HasValue)
        {
            _originalEndDate = member.EndDate.Value;
        }
        else
        {
            _originalEndDate = DateTime.Today;
        }

        RecalculateProjectedEndDate();
    }

    partial void OnFreezeStartDateChanged(DateTimeOffset value)
    {
        RecalculateProjectedEndDate();
    }

    partial void OnFreezeEndDateChanged(DateTimeOffset value)
    {
        RecalculateProjectedEndDate();
    }

    private void RecalculateProjectedEndDate()
    {
        int days = (FreezeEndDate.Date - FreezeStartDate.Date).Days;
        if (days < 0)
        {
            days = 0;
        }

        ProjectedEndDate = _originalEndDate.AddDays(days);
    }

    [RelayCommand]
    private async Task ConfirmFreezeAsync()
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            Message = "Lý do bảo lưu là bắt buộc.";
            return;
        }

        if (Reason.Trim().Length < 3)
        {
            Message = "Lý do bảo lưu phải có tối thiểu 3 ký tự.";
            return;
        }

        IsLoading = true;
        Message = null;

        try
        {
            var req = new FreezeSubscriptionRequest(
                MemberId,
                FreezeStartDate.DateTime,
                FreezeEndDate.DateTime,
                Reason
            );

            await _apiClient.FreezeSubscriptionAsync(req);
            Message = "Bảo lưu gói tập thành công!";
            OnFreezeSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            Message = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
