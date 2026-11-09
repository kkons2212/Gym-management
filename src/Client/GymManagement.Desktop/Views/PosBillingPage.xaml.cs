using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Members;

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
        else
        {
            await ViewModel.FinalizeInvoiceAsync();
        }
    }

    private async void BtnQuickAddMember_Click(object sender, RoutedEventArgs e)
    {
        IApiClient api;
        RegisterMemberWizard dialog;
        ContentDialogResult result;
        string registeredPhone;

        api = App.GetService<IApiClient>();
        var packages = await api.GetPackagePlansAsync();

        dialog = new RegisterMemberWizard(packages);
        dialog.XamlRoot = this.XamlRoot;

        result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.SelectedPackage != null)
        {
            registeredPhone = dialog.Phone;
            var req = new RegisterMemberRequest(
                dialog.FullName,
                dialog.Phone,
                dialog.Email,
                dialog.SelectedPackage.PlanId,
                DateOnly.FromDateTime(DateTime.Today),
                "Admin"
            );

            await api.RegisterMemberAsync(req);
            await ViewModel.ReloadMembersAsync();

            var newMem = ViewModel.Members.FirstOrDefault(m => m.Phone == registeredPhone);
            if (newMem != null)
            {
                ViewModel.SelectedMember = newMem;
            }
        }
    }
}
