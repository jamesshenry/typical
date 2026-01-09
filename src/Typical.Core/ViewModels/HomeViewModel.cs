using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Typical.Core.Interfaces;

namespace Typical.Core.ViewModels;

public sealed partial class HomeViewModel : ObservableObject, IBindableView
{
    private readonly INavigationService _navService;
    private readonly ILogger<HomeViewModel> _logger;

    public HomeViewModel(INavigationService navigationService, ILogger<HomeViewModel> logger)
    {
        _navService = navigationService;
        _logger = logger;
    }

    [ObservableProperty]
    private string _welcomeMessage = "Welcome to the Dashboard!";

    [RelayCommand]
    private void NavigateSettings() => _navService.NavigateTo<SettingsViewModel>();

    public void OnNavigatedTo()
    {
        _logger.LogInformation($"Navigated to {nameof(HomeViewModel)}");
    }

    public void OnNavigatedFrom()
    {
        _logger.LogInformation($"Navigated from {nameof(HomeViewModel)}");
    }
}
