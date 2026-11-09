using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get
        {
            return (SettingsViewModel)DataContext;
        }
    }

    public SettingsPage()
    {
        DataContext = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
