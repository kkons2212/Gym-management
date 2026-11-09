using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.PTSchedules;

namespace GymManagement.Desktop.Views;

public sealed partial class PtSchedulePage : Page
{
    public PtScheduleViewModel ViewModel
    {
        get
        {
            return (PtScheduleViewModel)DataContext;
        }
    }

    public PtSchedulePage()
    {
        DataContext = App.GetService<PtScheduleViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        };
    }

    private async void BtnReviewSession_Click(object sender, RoutedEventArgs e)
    {
        Button? btn;
        PtSessionSlotDto? slot;
        SessionReviewDialog dialog;
        ContentDialogResult result;

        btn = sender as Button;
        if (btn != null)
        {
            slot = btn.DataContext as PtSessionSlotDto;
            if (slot != null)
            {
                dialog = new SessionReviewDialog(slot.PtName, slot.MemberName, slot.ScheduledDate, slot.StartTime, slot.EndTime);
                dialog.XamlRoot = this.XamlRoot;

                result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.LoadScheduleAsync();
                }
            }
        }
    }
}
