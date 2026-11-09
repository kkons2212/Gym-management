using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class CheckInCounterPage : Page
{
    public CheckInCounterViewModel ViewModel
    {
        get
        {
            return (CheckInCounterViewModel)DataContext;
        }
    }

    public CheckInCounterPage()
    {
        DataContext = App.GetService<CheckInCounterViewModel>();
        InitializeComponent();
        Loaded += CheckInCounterPage_Loaded;
    }

    private async void CheckInCounterPage_Loaded(object sender, RoutedEventArgs e)
    {
        TxtSearchInput.Focus(FocusState.Programmatic);
        await ViewModel.InitializeAsync();
    }

    private void TxtSearchInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.CheckInManualCommand.Execute(null);
            e.Handled = true;
        }
    }

    private async void BtnOpenOverrideDialog_Click(object sender, RoutedEventArgs e)
    {
        OverrideCheckInDialog dialog;
        ContentDialogResult result;
        string prefill;

        prefill = ViewModel.SearchQuery;
        dialog = new OverrideCheckInDialog(prefill);
        dialog.XamlRoot = this.XamlRoot;

        result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.ExecuteOverrideCheckInAsync(dialog.MemberIdentifier, dialog.FullOverrideReason);
        }
    }

    private void CboShift_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox? combo;
        ComboBoxItem? selectedItem;
        string shift;

        combo = sender as ComboBox;
        if (combo != null)
        {
            selectedItem = combo.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Tag != null)
            {
                shift = selectedItem.Tag.ToString()!;
                if (ViewModel != null)
                {
                    ViewModel.SelectedShift = shift;
                }
            }
        }
    }
}
