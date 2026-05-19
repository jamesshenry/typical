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
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private bool _enableLogging = true;

    public SettingsViewModel(
        IDialogService dialogService,
        INavigationService navService,
        ILogger<SettingsViewModel> logger,
        IMessenger messenger
    )
    {
        _dialogService = dialogService;
        _navService = navService;
        _logger = logger;
        _messenger = messenger;
    }

    [RelayCommand]
    private void QuoteMode()
    {
        var message = new GameResetMessage(new QuoteMode(QuoteLength.Medium));
        _messenger.Send(message);
    }

    [RelayCommand]
    private void Cancel() => _navService.NavigateTo<HomeViewModel>();
}
