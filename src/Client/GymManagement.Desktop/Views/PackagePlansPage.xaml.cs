using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PackagePlansPage : Page
{
    public PackagePlansViewModel ViewModel => (PackagePlansViewModel)DataContext;

    public PackagePlansPage()
    {
        DataContext = App.GetService<PackagePlansViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadPackagesCommand.ExecuteAsync(null);
            }
        };
    }
}

