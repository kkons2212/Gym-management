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
    private bool _isInitialized;

    private readonly ObservableCollection<PtProfileDto> _displayedTrainers = [];

    public TrainerListDialog(IApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);

        _apiClient = apiClient;
        InitializeComponent();
        _isInitialized = true;
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
        if (PbLoading != null)
        {
            PbLoading.Visibility = Visibility.Visible;
        }

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
            if (PbLoading != null)
            {
                PbLoading.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void ApplyFilter()
    {
        if (!_isInitialized || PnlEmpty == null)
        {
            return;
        }

        string query = string.Empty;
        if (AsbSearch != null && AsbSearch.Text != null)
        {
            query = AsbSearch.Text.Trim().ToLowerInvariant();
        }

        string statusTag = "ALL";
        if (CboStatusFilter != null && CboStatusFilter.SelectedItem is ComboBoxItem item && item.Tag != null)
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

        if (PnlEmpty != null)
        {
            if (_displayedTrainers.Count == 0)
            {
                PnlEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                PnlEmpty.Visibility = Visibility.Collapsed;
            }
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
}

