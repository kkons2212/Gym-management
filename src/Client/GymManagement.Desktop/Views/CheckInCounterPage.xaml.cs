using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class CheckInCounterPage : Page
{
    public CheckInCounterViewModel ViewModel
    {
        get
        {
            return (CheckInCounterViewModel)DataContext;
        }
    }

    public CheckInCounterPage()
    {
        DataContext = App.GetService<CheckInCounterViewModel>();
        InitializeComponent();
    }
}
