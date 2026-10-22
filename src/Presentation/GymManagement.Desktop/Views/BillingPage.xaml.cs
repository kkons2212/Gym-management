using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class BillingPage : Page
{
    public BillingViewModel ViewModel { get; }

    public BillingPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<BillingViewModel>();
    }
}
