using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Core.DTOs;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class TrainersPage : Page
{
    public TrainersViewModel ViewModel { get; }

    public TrainersPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<TrainersViewModel>();
    }

    public static string FormatStatus(bool isActive) => isActive ? "ACTIVE" : "INACTIVE";
    public static string FormatSalary(decimal salary) => salary.ToString("N0") + " đ";
    public static string FormatExperience(int years) => $"{years} năm KN";
    public static string FormatDob(DateOnly? dob) => dob.HasValue ? dob.Value.ToString("dd/MM/yyyy") : "--";
    public static string FormatStudents(int count) => $"{count} học viên";
    public static string FormatSessions(int count) => $"{count} ca chưa xong";

    private void OnEditTrainerClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PtProfileDto trainer })
        {
            ViewModel.OpenEditTrainerDialog(trainer);
        }
    }

    private async void OnDeactivateTrainerClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PtProfileDto trainer })
        {
            await ViewModel.RequestDeactivateTrainerAsync(trainer);
        }
    }
}
