using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.PT;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class TrainerListDialog : ContentDialog
{
    private readonly IApiClient _apiClient;
    private List<PtProfileDto> _allTrainers = [];

    private readonly ObservableCollection<PtProfileDto> _displayedTrainers = [];

    public TrainerListDialog(IApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);

        _apiClient = apiClient;
        InitializeComponent();
        LvTrainers.ItemsSource = _displayedTrainers;

        Opened += async (s, e) =>
        {
            await LoadTrainersAsync();
        };

        Loaded += async (s, e) =>
        {
            if (_allTrainers.Count == 0)
            {
                await LoadTrainersAsync();
            }
        };
    }

    private async Task LoadTrainersAsync()
    {
        PbLoading.Visibility = Visibility.Visible;
        try
        {
            var list = await _apiClient.GetTrainersAsync();
            _allTrainers = list.ToList();

            if (_allTrainers.Count == 0)
            {
                var users = await _apiClient.GetUsersAsync();
                var fallbackList = new List<PtProfileDto>();
                foreach (var u in users)
                {
                    bool isPt = string.Equals(u.Role, "PT", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(u.Role, "TRAINER", StringComparison.OrdinalIgnoreCase) ||
                                (!string.IsNullOrWhiteSpace(u.FullName) && u.FullName.Contains("PT", StringComparison.OrdinalIgnoreCase));
                    if (isPt)
                    {
                        fallbackList.Add(new PtProfileDto(
                            PtId: u.UserId,
                            FullName: u.FullName,
                            Phone: string.Empty,
                            Email: u.Email,
                            Specialty: "HLV Thể Hình",
                            ExperienceYears: 1,
                            RatingAverage: 5.0m,
                            ReviewCount: 0,
                            IsActive: true,
                            ActiveStudentsCount: 0,
                            PendingSessionsCount: 0
                        ));
                    }
                }
                _allTrainers = fallbackList;
            }

            ApplyFilter();
        }
        catch
        {
            // Fallback gracefully
        }
        finally
        {
            PbLoading.Visibility = Visibility.Collapsed;
        }
    }

    private void ApplyFilter()
    {
        string query = AsbSearch.Text.Trim().ToLowerInvariant();
        string statusTag = "ALL";
        if (CboStatusFilter.SelectedItem is ComboBoxItem item && item.Tag != null)
        {
            statusTag = item.Tag.ToString()!;
        }

        _displayedTrainers.Clear();

        foreach (var t in _allTrainers)
        {
            string fullName = string.Empty;
            if (t.FullName != null)
            {
                fullName = t.FullName.ToLowerInvariant();
            }

            string phone = string.Empty;
            if (t.Phone != null)
            {
                phone = t.Phone;
            }

            string specialty = string.Empty;
            if (t.Specialty != null)
            {
                specialty = t.Specialty.ToLowerInvariant();
            }

            bool matchQuery = string.IsNullOrWhiteSpace(query) ||
                              fullName.Contains(query) ||
                              phone.Contains(query) ||
                              specialty.Contains(query);

            bool matchStatus = true;
            if (statusTag == "ACTIVE")
            {
                matchStatus = t.IsActive;
            }
            else if (statusTag == "INACTIVE")
            {
                matchStatus = !t.IsActive;
            }

            if (matchQuery && matchStatus)
            {
                _displayedTrainers.Add(t);
            }
        }

        if (_displayedTrainers.Count == 0)
        {
            PnlEmpty.Visibility = Visibility.Visible;
        }
        else
        {
            PnlEmpty.Visibility = Visibility.Collapsed;
        }
    }

    private void AsbSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        ApplyFilter();
    }

    private void CboStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private async void BtnAddNewTrainer_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TrainerProfileDialog(_apiClient)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (dialog.SavedTrainer != null)
        {
            await LoadTrainersAsync();
        }
    }

    private async void BtnEditTrainer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is PtProfileDto trainer)
        {
            var dialog = new TrainerProfileDialog(_apiClient, trainer)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (dialog.SavedTrainer != null)
            {
                await LoadTrainersAsync();
            }
        }
    }
}

