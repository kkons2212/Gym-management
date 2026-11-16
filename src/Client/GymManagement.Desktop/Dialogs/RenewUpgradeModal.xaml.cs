using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class RenewUpgradeModal : ContentDialog
{
    private readonly MemberDto _member;
    private readonly IReadOnlyList<PackagePlanDto> _packages;
    private readonly PackagePlanDto? _currentPlan;

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
            PackagePlanDto? selectedPlan;
            selectedPlan = SelectedPlan;

            if (selectedPlan == null)
            {
                return 0m;
            }

            if (IsUpgrade && _currentPlan != null)
            {
                // Nâng cấp: Phí thanh toán là chi phí chênh lệch giữa gói mới và gói hiện tại
                decimal priceDifference = selectedPlan.Price - _currentPlan.Price;
                if (priceDifference > 0)
                {
                    return priceDifference;
                }
                return 0m;
            }

            // Gia hạn hoặc đăng ký kích hoạt: Toàn bộ đơn giá niêm yết của gói
            return selectedPlan.Price;
        }
    }

    public RenewUpgradeModal(MemberDto member, IReadOnlyList<PackagePlanDto> packages)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(packages);

        InitializeComponent();

        _member = member;
        _packages = packages;
        _currentPlan = FindCurrentPlan();

        TxtMemberInfo.Text = $"Hội viên: {member.FullName} ({member.UserName})";

        if (_currentPlan != null)
        {
            TxtCurrentPlan.Text = _currentPlan.Name;
        }
        else if (!string.IsNullOrWhiteSpace(member.ActivePlanName))
        {
            TxtCurrentPlan.Text = member.ActivePlanName;
        }
        else
        {
            TxtCurrentPlan.Text = "Chưa có gói";
        }

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

        // Định nghĩa lựa chọn gói tập nếu đã có thực thể gói hiện tại (loaded plan entity)
        if (_currentPlan != null)
        {
            RbRenew.IsChecked = true;
            CboPackages.SelectedItem = _currentPlan;
            CboPackages.IsEnabled = false; // Khóa ComboBox khi gia hạn vì chỉ gia hạn đúng gói hiện tại
            LblSelectPackage.Text = "Gói tập hiện tại sẽ gia hạn:";
        }
        else
        {
            // Hội viên chưa có gói tập hợp lệ: Vô hiệu hóa 'Gia Hạn Gói Hiện Tại', mặc định chuyển sang Nâng Cấp/Đăng Ký
            RbRenew.IsEnabled = false;
            RbUpgrade.IsChecked = true;
            CboPackages.IsEnabled = true;
            if (_packages.Count > 0)
            {
                CboPackages.SelectedIndex = 0;
            }
            LblSelectPackage.Text = "Chọn gói tập kích hoạt mới:";
        }

        UpdatePriceDisplay();
        PrimaryButtonClick += RenewUpgradeModal_PrimaryButtonClick;
    }

    private PackagePlanDto? FindCurrentPlan()
    {
        if (string.IsNullOrWhiteSpace(_member.ActivePlanName))
        {
            return null;
        }

        foreach (var p in _packages)
        {
            if (string.Equals(p.PlanId, _member.ActivePlanName, StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }

            if (string.Equals(p.Name, _member.ActivePlanName, StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }
        }

        return null;
    }

    private void OnModeChanged(object sender, RoutedEventArgs e)
    {
        if (LblSelectPackage == null || CboPackages == null)
        {
            return;
        }

        if (IsUpgrade)
        {
            LblSelectPackage.Text = "Chọn gói tập mới để nâng cấp:";
            CboPackages.IsEnabled = true;

            // Nếu đang chọn đúng gói hiện tại, tự động gợi ý gói cấp cao hơn
            if (_currentPlan != null && SelectedPlan == _currentPlan)
            {
                PackagePlanDto? higherPlan = null;
                foreach (var p in _packages)
                {
                    if (p.Price > _currentPlan.Price)
                    {
                        higherPlan = p;
                        break;
                    }
                }

                if (higherPlan != null)
                {
                    CboPackages.SelectedItem = higherPlan;
                }
                else
                {
                    foreach (var p in _packages)
                    {
                        if (p != _currentPlan)
                        {
                            CboPackages.SelectedItem = p;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            LblSelectPackage.Text = "Gói tập hiện tại sẽ gia hạn:";
            if (_currentPlan != null)
            {
                CboPackages.SelectedItem = _currentPlan;
                CboPackages.IsEnabled = false; // Khóa lựa chọn vì gia hạn đúng gói hiện tại
            }
            else
            {
                CboPackages.IsEnabled = true;
            }
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

        if (plan == null)
        {
            TxtPaymentAmount.Text = "0 VNĐ";
            if (TxtPriceExplanation != null)
            {
                TxtPriceExplanation.Text = "Vui lòng chọn gói tập";
            }
            return;
        }

        decimal amount = TotalAmount;
        TxtPaymentAmount.Text = $"{amount:N0} VNĐ";

        if (TxtPriceExplanation != null)
        {
            if (IsUpgrade && _currentPlan != null)
            {
                decimal diff = plan.Price - _currentPlan.Price;
                if (diff > 0)
                {
                    TxtPriceExplanation.Text = $"Chênh lệch nâng cấp ({plan.Price:N0} đ - {_currentPlan.Price:N0} đ)";
                }
                else if (diff == 0)
                {
                    TxtPriceExplanation.Text = "Gói cùng mức giá niêm yết (0 VNĐ chênh lệch)";
                }
                else
                {
                    TxtPriceExplanation.Text = $"Gói thấp hơn gói hiện tại ({_currentPlan.Price:N0} đ) - Không thu thêm phí";
                }
            }
            else
            {
                TxtPriceExplanation.Text = "Đơn giá gia hạn niêm yết cố định (0 chiết khấu)";
            }
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

        if (IsUpgrade && _currentPlan != null && string.Equals(SelectedPlan.PlanId, _currentPlan.PlanId, StringComparison.OrdinalIgnoreCase))
        {
            TxtError.Text = "Gói nâng cấp phải khác với gói tập hiện tại. Nếu muốn gia hạn thêm thời gian gói này, vui lòng chọn 'Gia Hạn Gói Hiện Tại'.";
            TxtError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtError.Visibility = Visibility.Collapsed;
    }
}
