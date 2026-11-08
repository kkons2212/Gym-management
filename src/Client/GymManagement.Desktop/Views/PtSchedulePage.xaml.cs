using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PtSchedulePage : Page
{
    public PtScheduleViewModel ViewModel => (PtScheduleViewModel)DataContext;

    public PtSchedulePage()
    {
        InitializeComponent();
        DataContext = App.GetService<PtScheduleViewModel>();
        Loaded += async (s, e) => await ViewModel.InitializeAsync();
    }
}

