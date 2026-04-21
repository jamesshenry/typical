using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Interfaces;

namespace Typical.Core.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IRecipient<NavigationChangedMessage>
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    public partial string AppTitle { get; set; } = "Typical";

    [ObservableProperty]
    public partial string StatusText { get; set; } = "Ready";

    [ObservableProperty]
    public partial ObservableObject? CurrentPage { get; set; }

    public MainViewModel(
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<MainViewModel> logger
    )
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        WeakReferenceMessenger.Default.RegisterAll(this);
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

    public void Receive(NavigationChangedMessage message)
    {
        CurrentPage = message.Value;
    }
}
