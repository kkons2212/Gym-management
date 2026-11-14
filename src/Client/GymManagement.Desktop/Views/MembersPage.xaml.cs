using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PT;

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

    private static MemberDto? ExtractMemberFromSender(object sender)
    {
        if (sender is FrameworkElement fe)
        {
            if (fe.DataContext is MemberDto member)
            {
                return member;
            }
        }
        return null;
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

    public async void OnEditProfileClicked(object sender, RoutedEventArgs e)
    {
        var member = ExtractMemberFromSender(sender);
        if (member == null)
        {
            return;
        }

        var dialog = new MemberDetailDialog(member)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var api = App.GetService<IApiClient>();
            string newUserName = dialog.UserName;
            string? renameTarget = null;
            if (!string.Equals(newUserName, member.UserName, StringComparison.OrdinalIgnoreCase))
            {
                renameTarget = newUserName;
            }

            var req = new UpdateMemberRequest(
                member.UserName,
                dialog.FullName,
                dialog.Phone,
                dialog.Email,
                renameTarget
            );
            await api.UpdateMemberProfileAsync(member.UserName, req);
            await ViewModel.LoadMembersAsync();
        }
    }

    public async void OnRenewUpgradeClicked(object sender, RoutedEventArgs e)
    {
        var member = ExtractMemberFromSender(sender);
        if (member == null)
        {
            return;
        }

        var api = App.GetService<IApiClient>();
        var packages = await api.GetPackagePlansAsync();

        var dialog = new RenewUpgradeModal(member, packages)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.SelectedPlan != null)
        {
            if (dialog.IsUpgrade)
            {
                var req = new UpgradeSubscriptionRequest(member.UserName, dialog.SelectedPlan.PlanId, "admin");
                await api.UpgradeSubscriptionAsync(member.UserName, req);
            }
            else
            {
                var req = new RenewSubscriptionRequest(member.UserName, dialog.SelectedPlan.PlanId, DateOnly.FromDateTime(DateTime.Today), "admin");
                await api.RenewSubscriptionAsync(member.UserName, req);
            }

            await ViewModel.LoadMembersAsync();
        }
    }

    public async void OnAssignPtClicked(object sender, RoutedEventArgs e)
    {
        var member = ExtractMemberFromSender(sender);
        if (member == null)
        {
            return;
        }

        var api = App.GetService<IApiClient>();
        var users = await api.GetUsersAsync();
        var ptList = users.Where(u => u.Role == "PT").ToList();

        var dialog = new PtAssignDialog(member.FullName, member.ActivePlanName ?? "Gói PT", ptList)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.SelectedPt != null)
        {
            var req = new AssignPtRequest(member.UserName, dialog.SelectedPt.UserId, "admin");
            await api.AssignPtAsync(req);
            await ViewModel.LoadMembersAsync();
        }
    }

    public async void OnCancelSubscriptionClicked(object sender, RoutedEventArgs e)
    {
        var member = ExtractMemberFromSender(sender);
        if (member == null)
        {
            return;
        }

        var dialog = new CancelSubscriptionDialog(member)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var api = App.GetService<IApiClient>();
            var req = new CancelSubscriptionRequest(member.UserName, dialog.Reason, "admin");
            await api.CancelSubscriptionAsync(req);
            await ViewModel.LoadMembersAsync();
        }
    }

    public async void OnFreezeClicked(object sender, RoutedEventArgs e)
    {
        var member = ExtractMemberFromSender(sender);
        if (member == null)
        {
            return;
        }

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

    private async void BtnRegisterNewMember_Click(object sender, RoutedEventArgs e)
    {
        IApiClient api;
        RegisterMemberWizard dialog;
        ContentDialogResult result;

        api = App.GetService<IApiClient>();
        var packages = await api.GetPackagePlansAsync();

        dialog = new RegisterMemberWizard(packages)
        {
            XamlRoot = this.XamlRoot
        };

        result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.SelectedPackage != null)
        {
            try
            {
                var memberReq = new RegisterMemberRequest(
                    dialog.FullName,
                    dialog.Phone,
                    dialog.Email,
                    dialog.SelectedPackage.PlanId,
                    DateOnly.FromDateTime(DateTime.Today),
                    "Admin",
                    dialog.MemberCode,
                    dialog.Password
                );

                InvoiceDto? confirmedInvoice = null;

                if (dialog.PaymentMethod == "VIETQR")
                {
                    string transferContent = $"GYM {dialog.Phone} {dialog.SelectedPackage.PlanId}";
                    var qrPayload = await api.GenerateVietQrAsync("TEMP", dialog.SelectedPackage.Price, transferContent);

                    // Allow previous ContentDialog to fully close in WinUI 3
                    await Task.Delay(100);

                    var qrDialog = new VietQrPaymentDialog(
                        qrPayload.Amount,
                        qrPayload.BankName,
                        qrPayload.BankAccount,
                        qrPayload.TransferContent,
                        qrPayload.QrDataUrl,
                        api)
                    {
                        XamlRoot = this.XamlRoot
                    };

                    var qrResult = await qrDialog.ShowAsync();
                    if (!qrDialog.IsPaymentConfirmed && qrResult != ContentDialogResult.Primary)
                    {
                        // Khách chưa chuyển tiền hoặc thu ngân bấm đóng: không tạo hội viên, giữ nguyên danh sách
                        return;
                    }

                    // Đã xác nhận nhận tiền chuyển khoản: tiến hành tạo hội viên và kích hoạt hóa đơn
                    var registeredMember = await api.RegisterMemberAsync(memberReq);

                    var invoiceReq = new CreateInvoiceRequest(
                        registeredMember.UserName,
                        dialog.SelectedPackage.PlanId,
                        dialog.PaymentMethod,
                        "SUBSCRIPTION_PURCHASE"
                    );

                    var invoice = await api.CreateInvoiceAsync(invoiceReq);
                    confirmedInvoice = await api.ConfirmPaymentAsync(invoice.InvoiceCode);
                }
                else
                {
                    // Thanh toán tiền mặt CASH trực tiếp tại quầy
                    var registeredMember = await api.RegisterMemberAsync(memberReq);

                    var invoiceReq = new CreateInvoiceRequest(
                        registeredMember.UserName,
                        dialog.SelectedPackage.PlanId,
                        dialog.PaymentMethod,
                        "SUBSCRIPTION_PURCHASE"
                    );

                    var invoice = await api.CreateInvoiceAsync(invoiceReq);
                    confirmedInvoice = await api.ConfirmPaymentAsync(invoice.InvoiceCode);
                }

                if (confirmedInvoice != null)
                {
                    // Allow previous ContentDialog to fully close in WinUI 3
                    await Task.Delay(100);

                    var receiptDialog = new ReceiptPreviewDialog(confirmedInvoice)
                    {
                        XamlRoot = this.XamlRoot
                    };
                    await receiptDialog.ShowAsync();
                }

                await ViewModel.LoadMembersAsync();
            }
            catch (Exception ex)
            {
                await Task.Delay(100);
                var errorDialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = $"Không thể hoàn tất đăng ký: {ex.Message}",
                    CloseButtonText = "Đóng",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                await ViewModel.LoadMembersAsync();
            }
        }
    }
}
