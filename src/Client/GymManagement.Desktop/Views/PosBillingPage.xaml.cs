using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Packages;

namespace GymManagement.Desktop.Views;

public sealed partial class PosBillingPage : Page
{
    public PosBillingViewModel ViewModel
    {
        get
        {
            return (PosBillingViewModel)DataContext;
        }
    }

    public PosBillingPage()
    {
        DataContext = App.GetService<PosBillingViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        };
    }

    private void OnPackageSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView lv && lv.SelectedItem is PackagePlanDto plan)
        {
            ViewModel.SelectPackage(plan);
        }
    }

    private void BtnPaymentMethod_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string method)
        {
            ViewModel.SelectedPaymentMethod = method;

            var accentBrush = Application.Current.Resources["AccentPrimaryBrush"] as Brush;
            BtnCash.Background = null;
            BtnVietQr.Background = null;
            BtnCard.Background = null;

            if (method == "CASH")
            {
                BtnCash.Background = accentBrush;
            }
            else if (method == "VIETQR")
            {
                BtnVietQr.Background = accentBrush;
            }
            else if (method == "CREDIT_CARD")
            {
                BtnCard.Background = accentBrush;
            }
        }
    }

    private void OnPurchaseSelected(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.TransactionType = "SUBSCRIPTION_PURCHASE";
        }
    }

    private void OnRenewalSelected(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.TransactionType = "SUBSCRIPTION_RENEWAL";
        }
    }

    private async void OnProcessCheckoutClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedPaymentMethod == "VIETQR" && ViewModel.VietQrPayload != null)
        {
            var dialog = new VietQrPaymentDialog(
                ViewModel.VietQrPayload.Amount,
                ViewModel.VietQrPayload.BankName,
                ViewModel.VietQrPayload.BankAccount,
                ViewModel.VietQrPayload.TransferContent,
                ViewModel.VietQrPayload.QrDataUrl)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (dialog.IsPaymentConfirmed || result == ContentDialogResult.Primary)
            {
                await ViewModel.FinalizeInvoiceAsync();
            }
        }
    }
}
