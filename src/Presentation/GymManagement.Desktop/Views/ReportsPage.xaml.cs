using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class ReportsPage : Page
{
    public ReportsViewModel ViewModel { get; }

    public ReportsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ReportsViewModel>();
    }
}
