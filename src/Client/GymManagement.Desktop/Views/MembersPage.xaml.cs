using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Api;
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
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.InitializeAsync();
            }
        };
    }

    private async void OnSearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        await ViewModel.LoadMembersAsync();
    }

    private async void OnFilterClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string status)
        {
            ViewModel.StatusFilter = status;
            await ViewModel.LoadMembersAsync();
        }
    }

    public async void OnFreezeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MemberDto member)
        {
            var dialog = new FreezeSubscriptionDialog(member.FullName, member.EndDate)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var api = App.GetService<IApiClient>();
                var req = new FreezeSubscriptionRequest(member.UserId, dialog.FreezeStartDate, dialog.FreezeEndDate, dialog.Reason);
                await api.FreezeSubscriptionAsync(req);
                await ViewModel.LoadMembersAsync();
            }
        }
    }

    public async void OnInBodyClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MemberDto member)
        {
            var dialog = new InBodyEntryDialog(member.FullName)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var api = App.GetService<IApiClient>();
                var req = new InBodyEntryRequest(
                    member.UserId,
                    dialog.HeightCm,
                    dialog.WeightKg,
                    dialog.BodyFatPercentage,
                    dialog.SkeletalMuscleMass,
                    dialog.Notes
                );
                await api.RecordInBodyMetricAsync(req);
            }
        }
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
