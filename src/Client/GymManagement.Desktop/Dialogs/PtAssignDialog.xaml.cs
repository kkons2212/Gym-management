using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class PtAssignDialog : ContentDialog
{
    private readonly IReadOnlyList<UserSessionDto> _ptList = [];

    public UserSessionDto? SelectedPt
    {
        get
        {
            return CboPtList.SelectedItem as UserSessionDto;
        }
    }

    public string Notes
    {
        get
        {
            return TxtNotes.Text.Trim();
        }
    }

    public PtAssignDialog(string memberName, string subscriptionInfo, IReadOnlyList<UserSessionDto> ptList)
    {
        InitializeComponent();
        _ptList = ptList ?? Array.Empty<UserSessionDto>();

        TxtMemberName.Text = $"Hội viên: {memberName}";
        TxtSubscriptionInfo.Text = subscriptionInfo;
        CboPtList.ItemsSource = _ptList;

        if (_ptList != null && _ptList.Count > 0)
        {
            CboPtList.SelectedIndex = 0;
        }

        PrimaryButtonClick += PtAssignDialog_PrimaryButtonClick;
    }

    private void PtAssignDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        UserSessionDto? pt;
        pt = SelectedPt;

        if (pt == null)
        {
            TxtValidationError.Text = "Vui lòng chọn một Huấn luyện viên PT!";
            TxtValidationError.Visibility = Visibility.Visible;
            args.Cancel = true;
            return;
        }

        TxtValidationError.Visibility = Visibility.Collapsed;
    }
}
