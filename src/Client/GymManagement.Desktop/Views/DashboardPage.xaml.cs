using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel => (DashboardViewModel)DataContext;

    public DashboardPage()
    {
        InitializeComponent();
        DataContext = App.GetService<DashboardViewModel>();
        Loaded += async (s, e) => await ViewModel.LoadDashboardCommand.ExecuteAsync(null);
    }
}

