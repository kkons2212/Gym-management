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
        ShellVm = App.GetService<ShellViewModel>();
        DataContext = ShellVm;
        InitializeComponent();

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
        Type targetPage;
        if (tag == "CheckIn")
        {
            targetPage = typeof(CheckInCounterPage);
        }
        else if (tag == "Pos")
        {
            targetPage = typeof(PosBillingPage);
        }
        else if (tag == "Members")
        {
            targetPage = typeof(MembersPage);
        }
        else if (tag == "Schedule")
        {
            targetPage = typeof(PtSchedulePage);
        }
        else if (tag == "Dashboard")
        {
            targetPage = typeof(DashboardPage);
        }
        else if (tag == "Staff")
        {
            targetPage = typeof(SettingsPage);
        }
        else if (tag == "Packages")
        {
            targetPage = typeof(PackagePlansPage);
        }
        else
        {
            targetPage = typeof(CheckInCounterPage);
        }

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
        // Tuyệt đối không dùng F1 cho điểm danh (ux_rules.md 1.2)
        if (key == Windows.System.VirtualKey.F2)
        {
            SelectMenuItemByTag("Pos");
        }
        else if (key == Windows.System.VirtualKey.F3)
        {
            SelectMenuItemByTag("Members");
        }
        else if (key == Windows.System.VirtualKey.F5)
        {
            SelectMenuItemByTag("Schedule");
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
