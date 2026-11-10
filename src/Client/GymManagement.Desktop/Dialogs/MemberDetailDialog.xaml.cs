using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class MemberDetailDialog : ContentDialog
{
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

        TxtUserName.Text = member.UserName;
        TxtFullName.Text = member.FullName;
        TxtPhone.Text = member.Phone;
        TxtEmail.Text = member.Email;
        TxtCreatedAt.Text = member.CreatedAt.ToString("dd/MM/yyyy");

        PrimaryButtonClick += MemberDetailDialog_PrimaryButtonClick;
    }

    private void MemberDetailDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            TxtValidationError.Text = "Họ và tên không được để trống.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            TxtValidationError.Text = "Số điện thoại không được để trống.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (Phone.Length < 9 || Phone.Length > 11)
        {
            TxtValidationError.Text = "Số điện thoại phải có độ dài từ 9 đến 11 chữ số.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationError.Visibility = Visibility.Collapsed;
    }
}

