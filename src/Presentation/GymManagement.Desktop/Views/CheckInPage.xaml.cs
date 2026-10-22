using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class CheckInPage : Page
{
    public CheckInViewModel ViewModel { get; }

    public CheckInPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CheckInViewModel>();
    }
}
