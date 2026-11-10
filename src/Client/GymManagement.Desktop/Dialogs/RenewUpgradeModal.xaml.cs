using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class RenewUpgradeModal : ContentDialog
{
    private readonly MemberDto _member;
    private readonly IReadOnlyList<PackagePlanDto> _packages;

    public bool IsUpgrade
    {
        get
        {
            if (RbUpgrade != null && RbUpgrade.IsChecked.HasValue)
            {
                return RbUpgrade.IsChecked.Value;
            }
            return false;
        }
    }

    public PackagePlanDto? SelectedPlan
    {
        get
        {
            return CboPackages.SelectedItem as PackagePlanDto;
        }
    }

    public string PaymentMethod
    {
        get
        {
            if (RbVietQr != null && RbVietQr.IsChecked.HasValue && RbVietQr.IsChecked.Value)
            {
                return "VIETQR";
            }
            return "CASH";
        }
    }

    public decimal TotalAmount
    {
        get
        {
            PackagePlanDto? plan;
            plan = SelectedPlan;
            if (plan != null)
            {
                return plan.Price;
            }
            return 0m;
        }
    }

    public RenewUpgradeModal(MemberDto member, IReadOnlyList<PackagePlanDto> packages)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(packages);

        InitializeComponent();

        _member = member;
        _packages = packages;

        TxtMemberInfo.Text = $"Hội viên: {member.FullName} ({member.UserName})";
        TxtCurrentPlan.Text = member.ActivePlanName ?? "Chưa có gói";

        if (member.EndDate.HasValue)
        {
            TxtCurrentEndDate.Text = $"Hạn dùng: {member.EndDate.Value:dd/MM/yyyy}";
        }
        else
        {
            TxtCurrentEndDate.Text = "Hạn dùng: N/A";
        }

        TxtRemainingSessions.Text = $"Còn {member.RemainingCheckInSessions} lượt gym • {member.RemainingPtSessions} buổi PT";

        CboPackages.ItemsSource = _packages;
        if (_packages.Count > 0)
        {
            CboPackages.SelectedIndex = 0;
        }

        UpdatePriceDisplay();
        PrimaryButtonClick += RenewUpgradeModal_PrimaryButtonClick;
    }

    private void OnModeChanged(object sender, RoutedEventArgs e)
    {
        if (LblSelectPackage == null)
        {
            return;
        }

        if (IsUpgrade)
        {
            LblSelectPackage.Text = "Chọn gói tập mới để nâng cấp:";
        }
        else
        {
            LblSelectPackage.Text = "Chọn gói tập để gia hạn thêm:";
        }

        UpdatePriceDisplay();
    }

    private void CboPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePriceDisplay();
    }

    private void UpdatePriceDisplay()
    {
        if (TxtPaymentAmount == null)
        {
            return;
        }

        PackagePlanDto? plan;
        plan = SelectedPlan;

        if (plan != null)
        {
            TxtPaymentAmount.Text = $"{plan.Price:N0} VNĐ";
        }
        else
        {
            TxtPaymentAmount.Text = "0 VNĐ";
        }
    }

    private void RenewUpgradeModal_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (SelectedPlan == null)
        {
            TxtError.Text = "Vui lòng chọn gói tập để tiếp tục.";
            TxtError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtError.Visibility = Visibility.Collapsed;
    }
}

