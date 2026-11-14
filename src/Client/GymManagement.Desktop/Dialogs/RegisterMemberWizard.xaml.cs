using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class RegisterMemberWizard : ContentDialog
{
    private readonly IReadOnlyList<PackagePlanDto> _packagePlans = [];

    public string? MemberCode
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TxtMemberCode.Text))
            {
                return null;
            }

            return TxtMemberCode.Text.Trim();
        }
    }

    public string FullName
    {
        get
        {
            return TxtFullName.Text.Trim();
        }
    }

    public string Phone
    {
        get
        {
            return TxtPhone.Text.Trim();
        }
    }

    public string Email
    {
        get
        {
            return TxtEmail.Text.Trim();
        }
    }

    public string Gender
    {
        get
        {
            ComboBoxItem? item;
            item = CboGender.SelectedItem as ComboBoxItem;
            if (item != null && item.Content != null)
            {
                return item.Content.ToString()!;
            }
            return "Nam";
        }
    }

    public PackagePlanDto? SelectedPackage
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
            ComboBoxItem? item;
            item = CboPaymentMethod.SelectedItem as ComboBoxItem;
            if (item != null && item.Tag != null)
            {
                return item.Tag.ToString()!;
            }
            return "CASH";
        }
    }

    public RegisterMemberWizard(IReadOnlyList<PackagePlanDto> packagePlans)
    {
        InitializeComponent();
        _packagePlans = packagePlans ?? Array.Empty<PackagePlanDto>();
        CboPackages.ItemsSource = _packagePlans;

        if (_packagePlans != null && _packagePlans.Count > 0)
        {
            CboPackages.SelectedIndex = 0;
            UpdatePriceDisplay();
        }

        PrimaryButtonClick += RegisterMemberWizard_PrimaryButtonClick;
    }

    private void RegisterMemberWizard_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        string name;
        string phone;
        name = FullName;
        phone = Phone;

        if (!string.IsNullOrWhiteSpace(MemberCode))
        {
            if (MemberCode.Length > 10)
            {
                TxtValidationError.Text = "Mã hội viên không được vượt quá 10 ký tự!";
                TxtValidationError.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(MemberCode, @"^[A-Za-z0-9_-]+$"))
            {
                TxtValidationError.Text = "Mã hội viên chỉ được chứa chữ cái, chữ số, gạch ngang (-) hoặc gạch dưới (_)!";
                TxtValidationError.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            TxtValidationError.Text = "Họ và tên hội viên không được để trống!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(phone) || !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$"))
        {
            TxtValidationError.Text = "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số (bắt đầu bằng 0)!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (SelectedPackage == null)
        {
            TxtValidationError.Text = "Vui lòng chọn một gói dịch vụ để kích hoạt!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationError.Visibility = Visibility.Collapsed;
    }

    private void CboPackages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePriceDisplay();
    }

    private void UpdatePriceDisplay()
    {
        PackagePlanDto? pkg;
        pkg = SelectedPackage;
        if (pkg != null && TxtTotalAmount != null)
        {
            TxtTotalAmount.Text = $"{pkg.Price:N0} đ";
        }
    }
}
