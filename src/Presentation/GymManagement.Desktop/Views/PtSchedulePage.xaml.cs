using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PtSchedulePage : Page
{
    public PtScheduleViewModel ViewModel { get; }

    public PtSchedulePage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PtScheduleViewModel>();
    }
}
