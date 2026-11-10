using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class CancelSubscriptionDialog : ContentDialog
{
    public string Reason
    {
        get
        {
            return TxtReason.Text.Trim();
        }
    }

    public CancelSubscriptionDialog(MemberDto member)
    {
        ArgumentNullException.ThrowIfNull(member);

        InitializeComponent();

        TxtMemberInfo.Text = $"{member.FullName} ({member.UserName})";
        TxtPlanName.Text = member.ActivePlanName ?? "Gói thành viên";

        PrimaryButtonClick += CancelSubscriptionDialog_PrimaryButtonClick;
    }

    private void CancelSubscriptionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            TxtValidationError.Text = "Vui lòng nhập lý do hủy gói tập.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (Reason.Length < 10)
        {
            TxtValidationError.Text = "Lý do hủy gói phải có độ dài tối thiểu 10 ký tự.";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationError.Visibility = Visibility.Collapsed;
    }
}

