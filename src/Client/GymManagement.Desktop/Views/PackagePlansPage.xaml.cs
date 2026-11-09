using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PackagePlansPage : Page
{
    public PackagePlansViewModel ViewModel
    {
        get
        {
            return (PackagePlansViewModel)DataContext;
        }
    }

    public PackagePlansPage()
    {
        DataContext = App.GetService<PackagePlansViewModel>();
        InitializeComponent();
    }
}
