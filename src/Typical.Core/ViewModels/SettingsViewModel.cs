using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Interfaces;

namespace Typical.Core.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private bool _enableLogging = true;

    public SettingsViewModel(
        IDialogService dialogService,
        INavigationService navService,
        ILogger<SettingsViewModel> logger
    )
    {
        _dialogService = dialogService;
        _navService = navService;
        _logger = logger;
    }

    [RelayCommand]
    private void QuoteMode()
    {
        var message = new GameResetMessage(new QuoteMode(QuoteLength.Medium));
        WeakReferenceMessenger.Default.Send(message);
    }

    [RelayCommand]
    private void Cancel() => _navService.NavigateTo<HomeViewModel>();
}
