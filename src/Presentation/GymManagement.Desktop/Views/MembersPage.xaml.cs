using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class MembersPage : Page
{
    public MembersViewModel ViewModel { get; }

    public MembersPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MembersViewModel>();
    }
}
