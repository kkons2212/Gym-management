using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PosBillingPage : Page
{
    public PosBillingViewModel ViewModel
    {
        get
        {
            return (PosBillingViewModel)DataContext;
        }
    }

    public PosBillingPage()
    {
        DataContext = App.GetService<PosBillingViewModel>();
        InitializeComponent();
    }
}
