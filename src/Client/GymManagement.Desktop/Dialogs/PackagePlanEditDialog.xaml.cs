using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Settings;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class PackagePlanEditDialog : ContentDialog
{
    private readonly IApiClient _apiClient;
    private readonly PackagePlanDto _targetPlan;

    internal PackagePlanDto? SavedPlan { get; private set; }

    public PackagePlanEditDialog(IApiClient apiClient, PackagePlanDto plan)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(plan);

        _apiClient = apiClient;
        _targetPlan = plan;

        InitializeComponent();

        TxtPlanId.Text = plan.PlanId;
        TxtName.Text = plan.Name;
        TxtDescription.Text = plan.Description;
        NbPrice.Value = (double)plan.Price;
        NbDurationDays.Value = plan.DurationDays;
        if (plan.MaxCheckInSessions.HasValue)
        {
            NbMaxCheckInSessions.Value = plan.MaxCheckInSessions.Value;
        }
        else
        {
            NbMaxCheckInSessions.Value = 30;
        }

        CbIsPtIncluded.IsChecked = plan.IsPtIncluded;

        if (plan.TotalPtSessions.HasValue)
        {
            NbTotalPtSessions.Value = plan.TotalPtSessions.Value;
        }
        else
        {
            NbTotalPtSessions.Value = 0;
        }

        if (plan.IsPtIncluded)
        {
            PnlPtSessions.Visibility = Visibility.Visible;
        }

        SelectStatus(plan.Status);

        PrimaryButtonClick += PackagePlanEditDialog_PrimaryButtonClick;
    }

    private void SelectStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return;
        }

        foreach (var item in CboStatus.Items)
        {
            if (item is ComboBoxItem cbi && cbi.Tag != null)
            {
                if (string.Equals(cbi.Tag.ToString(), status, StringComparison.OrdinalIgnoreCase))
                {
                    CboStatus.SelectedItem = cbi;
                    return;
                }
            }
        }
    }

    private void CbIsPtIncluded_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (CbIsPtIncluded.IsChecked == true)
        {
            PnlPtSessions.Visibility = Visibility.Visible;
        }
        else
        {
            PnlPtSessions.Visibility = Visibility.Collapsed;
        }
    }

    private async void PackagePlanEditDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        PnlValidationError.Visibility = Visibility.Collapsed;
        TxtValidationError.Text = string.Empty;

        string name = TxtName.Text.Trim();
        string description = TxtDescription.Text.Trim();
        decimal price = (decimal)NbPrice.Value;
        int durationDays = (int)NbDurationDays.Value;
        int maxCheckIn = (int)NbMaxCheckInSessions.Value;
        bool isPtIncluded = CbIsPtIncluded.IsChecked == true;
        int totalPtSessions = 0;
        if (isPtIncluded)
        {
            totalPtSessions = (int)NbTotalPtSessions.Value;
        }

        // 1. Validation
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
        {
            ShowError("Tên gói tập phải có ít nhất 3 ký tự.");
            return;
        }

        if (price <= 0)
        {
            ShowError("Đơn giá gói tập phải lớn hơn 0 VNĐ.");
            return;
        }

        if (durationDays <= 0)
        {
            ShowError("Thời hạn gói tập phải lớn hơn 0 ngày.");
            return;
        }

        // 2. Idempotency Lock
        IsPrimaryButtonEnabled = false;
        PrSaving.Visibility = Visibility.Visible;
        PrSaving.IsActive = true;

        try
        {
            var request = new UpdatePackagePlanRequest(
                name,
                description,
                price,
                durationDays,
                maxCheckIn,
                isPtIncluded,
                totalPtSessions
            );

            SavedPlan = await _apiClient.UpdatePackagePlanAsync(_targetPlan.PlanId, request);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Không thể cập nhật gói tập: {ex.Message}");
        }
        finally
        {
            IsPrimaryButtonEnabled = true;
            PrSaving.IsActive = false;
            PrSaving.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowError(string message)
    {
        TxtValidationError.Text = message;
        PnlValidationError.Visibility = Visibility.Visible;
    }
}

