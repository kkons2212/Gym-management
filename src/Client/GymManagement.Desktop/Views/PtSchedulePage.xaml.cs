using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Api;
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

    private async void BtnAddNewTrainer_Click(object sender, RoutedEventArgs e)
    {
        var apiClient = App.GetService<IApiClient>();
        var dialog = new TrainerProfileDialog(apiClient)
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
        if (dialog.SavedTrainer != null && ViewModel != null)
        {
            await ViewModel.InitializeAsync();
        }
    }

    private async void BtnManageTrainers_Click(object sender, RoutedEventArgs e)
    {
        var apiClient = App.GetService<IApiClient>();
        var dialog = new TrainerListDialog(apiClient)
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
        if (ViewModel != null)
        {
            await ViewModel.InitializeAsync();
        }
    }

    private async void BtnReviewSession_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PtSessionSlotDto slot)
        {
            var dialog = new SessionReviewDialog(slot.PtName, slot.MemberName, slot.ScheduledDate, slot.StartTime, slot.EndTime)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && ViewModel != null)
            {
                await ViewModel.ConfirmSessionResultAsync(slot.SessionId, dialog.SelectedOutcome, dialog.ReviewNotes);
            }
        }
    }
}
