using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GymManagement.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Gym House - Hệ Thống Quản Lý Phòng Gym";

    [ObservableProperty]
    private string _currentRole = "Admin";

    [ObservableProperty]
    private string _operatorName = "Lê Thanh Tùng";

    [RelayCommand]
    private void SwitchRole(string newRole)
    {
        CurrentRole = newRole;
    }
}
