using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Views;

namespace GymManagement.Desktop;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Mặc định điều hướng về trang Tổng quan
        ContentFrame.Navigate(typeof(DashboardPage));
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem selectedItem)
        {
            var tag = selectedItem.Tag?.ToString();
            Type? pageType = tag switch
            {
                "Dashboard" => typeof(DashboardPage),
                "Members" => typeof(MembersPage),
                "CheckIn" => typeof(CheckInPage),
                "PtSchedule" => typeof(PtSchedulePage),
                "Billing" => typeof(BillingPage),
                "Settings" => typeof(SettingsPage),
                "Reports" => typeof(ReportsPage),
                _ => typeof(DashboardPage)
            };

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}
