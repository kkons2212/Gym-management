using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class MemberDetailDialog : ContentDialog
{
    public string OriginalUserName { get; }

    public string UserName
    {
        get
        {
            return TxtUserName.Text.Trim();
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

    public MemberDetailDialog(MemberDto member)
    {
        ArgumentNullException.ThrowIfNull(member);

        InitializeComponent();

        OriginalUserName = member.UserName;
        TxtUserName.Text = member.UserName;
        TxtFullName.Text = member.FullName;
        TxtPhone.Text = member.Phone;
        TxtEmail.Text = member.Email;
        TxtCreatedAt.Text = member.CreatedAt.ToString("dd/MM/yyyy");

        PrimaryButtonClick += MemberDetailDialog_PrimaryButtonClick;
    }

    private void MemberDetailDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            TxtValidationError.Text = "Mã hội viên không được để trống.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (UserName.Length > 10)
        {
            TxtValidationError.Text = "Mã hội viên không được vượt quá 10 ký tự.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(UserName, @"^[A-Za-z0-9_-]+$"))
        {
            TxtValidationError.Text = "Mã hội viên chỉ được chứa chữ cái, chữ số, gạch ngang (-) hoặc gạch dưới (_).";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            TxtValidationError.Text = "Họ và tên không được để trống.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone) || !System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^0\d{9}$"))
        {
            TxtValidationError.Text = "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số (bắt đầu bằng 0).";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationError.Visibility = Visibility.Collapsed;
    }
}

