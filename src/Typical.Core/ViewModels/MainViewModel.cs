using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Typical.Core.Interfaces;

namespace Typical.Core.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private string _appTitle = "Typical";

    [ObservableProperty]
    private string _statusText = "Ready";

    public MainViewModel(
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<MainViewModel> logger
    )
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;
    }

    [RelayCommand]
    private void NavigateToGameView() => _navigationService.NavigateTo<TypingViewModel>();

    [RelayCommand]
    private void NavigateHome() => _navigationService.NavigateTo<HomeViewModel>();

    [RelayCommand]
    private void NavigateSettings() => _navigationService.NavigateTo<SettingsViewModel>();

    [RelayCommand]
    private void ShowAbout()
    {
        _dialogService.ShowError("About", "Typical: A Terminal.Gui v2 MVVM Demo");
    }
}
