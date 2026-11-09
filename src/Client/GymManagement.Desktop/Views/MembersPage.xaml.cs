using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.Views;

public sealed partial class MembersPage : Page
{
    public MembersViewModel ViewModel
    {
        get
        {
            return (MembersViewModel)DataContext;
        }
    }

    public MembersPage()
    {
        DataContext = App.GetService<MembersViewModel>();
        InitializeComponent();
    }

    private async void BtnRegisterNewMember_Click(object sender, RoutedEventArgs e)
    {
        IApiClient api;
        RegisterMemberWizard dialog;
        ContentDialogResult result;

        api = App.GetService<IApiClient>();
        var packages = await api.GetPackagePlansAsync();

        dialog = new RegisterMemberWizard(packages);
        dialog.XamlRoot = this.XamlRoot;

        result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.SelectedPackage != null)
        {
            var memberReq = new RegisterMemberRequest(
                dialog.FullName,
                dialog.Phone,
                dialog.Email,
                dialog.SelectedPackage.PlanId,
                DateOnly.FromDateTime(DateTime.Today),
                "Admin"
            );

            var registeredMember = await api.RegisterMemberAsync(memberReq);

            var invoiceReq = new CreateInvoiceRequest(
                registeredMember.UserName,
                dialog.SelectedPackage.PlanId,
                dialog.PaymentMethod,
                "SUBSCRIPTION_PURCHASE"
            );

            await api.CreateInvoiceAsync(invoiceReq);
            await ViewModel.LoadMembersAsync();
        }
    }
}
