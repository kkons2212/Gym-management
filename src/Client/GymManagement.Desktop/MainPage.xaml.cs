using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using GymManagement.Desktop.ViewModels;
using GymManagement.Desktop.Views;

namespace GymManagement.Desktop;

public sealed partial class MainPage : Page
{
    public ShellViewModel ShellVm { get; }

    public MainPage()
    {
        InitializeComponent();
        ShellVm = App.GetService<ShellViewModel>();
        DataContext = ShellVm;

        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Default to Check-in counter
        if (NavView.MenuItems.Count > 0 && NavView.SelectedItem == null)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    public void NavigateTo(string tag)
    {
        Type targetPage = tag switch
        {
            "CheckIn" => typeof(CheckInCounterPage),
            "Pos" => typeof(PosBillingPage),
            "Members" => typeof(MembersPage),
            "Schedule" => typeof(PtSchedulePage),
            "Packages" => typeof(PackagePlansPage),
            "Dashboard" => typeof(DashboardPage),
            "Settings" => typeof(SettingsPage),
            _ => typeof(CheckInCounterPage)
        };

        if (ContentFrame.CurrentSourcePageType != targetPage)
        {
            ContentFrame.Navigate(targetPage);
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        ShellVm.Logout();
        if (Frame != null)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }

    public void HandleShortcut(Windows.System.VirtualKey key)
    {
        switch (key)
        {
            case Windows.System.VirtualKey.F1:
                SelectMenuItemByTag("CheckIn");
                break;
            case Windows.System.VirtualKey.F2:
                SelectMenuItemByTag("Pos");
                break;
            case Windows.System.VirtualKey.F3:
                SelectMenuItemByTag("Members");
                break;
            case Windows.System.VirtualKey.F5:
                SelectMenuItemByTag("Schedule");
                break;
        }
    }

    private void SelectMenuItemByTag(string tag)
    {
        foreach (var menuItem in NavView.MenuItems)
        {
            if (menuItem is NavigationViewItem navItem && (string)navItem.Tag == tag)
            {
                NavView.SelectedItem = navItem;
                break;
            }
        }
    }
}
