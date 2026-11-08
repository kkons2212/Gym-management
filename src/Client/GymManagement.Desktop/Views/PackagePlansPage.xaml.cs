using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PackagePlansPage : Page
{
    public PackagePlansViewModel ViewModel => (PackagePlansViewModel)DataContext;

    public PackagePlansPage()
    {
        InitializeComponent();
        DataContext = App.GetService<PackagePlansViewModel>();
        Loaded += async (s, e) => await ViewModel.LoadPackagesCommand.ExecuteAsync(null);
    }
}

