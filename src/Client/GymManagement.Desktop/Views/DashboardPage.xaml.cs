using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel
    {
        get
        {
            return (DashboardViewModel)DataContext;
        }
    }

    public DashboardPage()
    {
        DataContext = App.GetService<DashboardViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDashboardCommand.ExecuteAsync(null);
            }
        };
    }
}
