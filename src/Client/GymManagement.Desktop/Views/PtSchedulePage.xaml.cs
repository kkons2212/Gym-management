using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class PtSchedulePage : Page
{
    public PtScheduleViewModel ViewModel
    {
        get
        {
            return (PtScheduleViewModel)DataContext;
        }
    }

    public PtSchedulePage()
    {
        DataContext = App.GetService<PtScheduleViewModel>();
        InitializeComponent();
    }
}
