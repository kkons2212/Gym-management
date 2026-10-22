using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;

namespace GymManagement.Desktop.ViewModels;

public partial class PtScheduleViewModel : ObservableObject
{
    private readonly IPtService _ptService;

    [ObservableProperty]
    private PtProfileDto? _selectedPt;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<PtProfileDto> TrainersList { get; } = [];
    public ObservableCollection<PtSessionDto> SessionsList { get; } = [];

    public PtScheduleViewModel(IPtService ptService)
    {
        _ptService = ptService;
        _ = LoadTrainersAndScheduleAsync();
    }

    [RelayCommand]
    public async Task LoadTrainersAndScheduleAsync()
    {
        var trainers = await _ptService.GetTrainersAsync();
        TrainersList.Clear();
        foreach (var t in trainers)
        {
            TrainersList.Add(t);
        }

        if (SelectedPt == null && TrainersList.Count > 0)
        {
            SelectedPt = TrainersList[0];
        }

        await LoadSessionsForPtAsync();
    }

    [RelayCommand]
    public async Task LoadSessionsForPtAsync()
    {
        if (SelectedPt == null) return;
        var sessions = await _ptService.GetPtScheduleAsync(SelectedPt.PtId, SelectedDate, SelectedDate.AddDays(7));
        SessionsList.Clear();
        foreach (var s in sessions)
        {
            SessionsList.Add(s);
        }
    }

    [RelayCommand]
    public Task OpenAssignDialogAsync()
    {
        StatusMessage = "Mở popup phân công PT cho học viên (UC04.2)...";
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task OpenBookSessionDialogAsync()
    {
        StatusMessage = "Mở popup xếp lịch ca tập mới (UC04.3)...";
        return Task.CompletedTask;
    }
}
