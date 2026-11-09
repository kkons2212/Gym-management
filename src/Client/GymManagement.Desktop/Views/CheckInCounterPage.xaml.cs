using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class CheckInCounterPage : Page
{
    public CheckInCounterViewModel ViewModel => (CheckInCounterViewModel)DataContext;

    public CheckInCounterPage()
    {
        DataContext = App.GetService<CheckInCounterViewModel>();
        InitializeComponent();
        Loaded += (s, e) => TxtSearchInput.Focus(FocusState.Programmatic);
    }

    private void TxtSearchInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.CheckInManualCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void BtnOverridePhone_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ManualOverrideCommand.Execute("Quên điện thoại mang theo");
    }

    private void BtnOverrideTurnstile_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ManualOverrideCommand.Execute("Cổng turnstile kẹt cơ khí");
    }
}
