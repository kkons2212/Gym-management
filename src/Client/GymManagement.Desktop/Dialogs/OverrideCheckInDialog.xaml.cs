using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class OverrideCheckInDialog : ContentDialog
{
    public string MemberIdentifier
    {
        get
        {
            return TxtMemberIdentifier.Text.Trim();
        }
    }

    public string SelectedReason
    {
        get
        {
            ComboBoxItem? selectedItem;
            selectedItem = CboReason.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Content != null)
            {
                return selectedItem.Content.ToString()!;
            }
            return "Quên mang điện thoại / Hết pin";
        }
    }

    public string Notes
    {
        get
        {
            return TxtNotes.Text.Trim();
        }
    }

    public string FullOverrideReason
    {
        get
        {
            string reason;
            string detail;
            reason = SelectedReason;
            detail = Notes;
            if (!string.IsNullOrWhiteSpace(detail))
            {
                return $"{reason}: {detail}";
            }
            return reason;
        }
    }

    public OverrideCheckInDialog(string? prefilledIdentifier = null)
    {
        InitializeComponent();

        if (!string.IsNullOrWhiteSpace(prefilledIdentifier))
        {
            TxtMemberIdentifier.Text = prefilledIdentifier.Trim();
        }

        PrimaryButtonClick += OverrideCheckInDialog_PrimaryButtonClick;
    }

    private void OverrideCheckInDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        string id;
        string detail;
        id = MemberIdentifier;
        detail = Notes;

        if (string.IsNullOrWhiteSpace(id))
        {
            TxtValidationMessage.Text = "Vui lòng nhập Mã hội viên hoặc Số điện thoại!";
            TxtValidationMessage.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        if (CboReason.SelectedIndex == 3 && string.IsNullOrWhiteSpace(detail))
        {
            TxtValidationMessage.Text = "Với lý do 'Khác', bắt buộc phải nhập ghi chú chi tiết!";
            TxtValidationMessage.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationMessage.Visibility = Visibility.Collapsed;
    }

    private void CboReason_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TxtNotes != null && CboReason != null)
        {
            if (CboReason.SelectedIndex == 3)
            {
                TxtNotes.PlaceholderText = "Bắt buộc nhập lý do chi tiết vào đây...";
            }
            else
            {
                TxtNotes.PlaceholderText = "Nhập giải trình ngoại lệ nếu cần...";
            }
        }
    }
}

