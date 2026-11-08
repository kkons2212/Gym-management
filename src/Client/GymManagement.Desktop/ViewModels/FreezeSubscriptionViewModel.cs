using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.ViewModels;

public partial class FreezeSubscriptionViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _memberId = string.Empty;

    [ObservableProperty]
    private string _memberName = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _freezeStartDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _freezeEndDate = DateTimeOffset.Now.AddDays(30);

    [ObservableProperty]
    private string _reason = "Bảo lưu theo nhu cầu cá nhân";

    [ObservableProperty]
    private DateTimeOffset _projectedEndDate = DateTimeOffset.Now.AddDays(90);

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _message;

    public event Action? OnFreezeSuccess;

    public FreezeSubscriptionViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public void Setup(MemberDto member)
    {
        MemberId = member.UserId;
        MemberName = member.FullName;
        FreezeStartDate = DateTimeOffset.Now;
        FreezeEndDate = DateTimeOffset.Now.AddDays(30);
        ProjectedEndDate = (member.EndDate ?? DateTime.Today).AddDays(30);
    }

    [RelayCommand]
    private async Task ConfirmFreezeAsync()
    {
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
            Message = "Bảo lưu thành công!";
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

