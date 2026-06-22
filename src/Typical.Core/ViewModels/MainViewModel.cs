using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Typical.Core.Data;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IStatsRepository _statsRepository;
    private readonly IDialogService _dialogService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    public partial string AppTitle { get; set; } = "Typical";

    [ObservableProperty]
    public partial string StatusText { get; set; } = "Ready";

    [ObservableProperty]
    public partial ObservableObject? CurrentPage { get; set; }

    public MainViewModel(
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<MainViewModel> logger,
        IMessenger messenger,
        IStatsRepository statsRepository
    )
    {
        _navigationService = navigationService;
        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(navigationService.CurrentViewModel))
            {
                CurrentPage = _navigationService.CurrentViewModel;
            }
        };
        _dialogService = dialogService;
        _logger = logger;
        _messenger = messenger;

        _messenger.Register<MainViewModel, TestCompletedMessage>(this, (r, m) => r.Receive(m));
        _messenger.Register<MainViewModel, TestResetMessage>(this, (r, m) => r.Receive(m));
        _messenger.Register<MainViewModel, ShowResultDialogMessage>(this, (r, m) => r.Receive(m));
        _statsRepository = statsRepository;
    }

    [RelayCommand]
    private void NavigateToTestView() => _navigationService.NavigateTo<TypingViewModel>();

    //[RelayCommand]
    //private void NavigateHome() => _navigationService.NavigateTo<HomeViewModel>();

    [RelayCommand]
    private void NavigateSettings() => _navigationService.NavigateTo<SettingsViewModel>();

    [RelayCommand]
    private void ShowAbout()
    {
        _dialogService.ShowError("About", "Typical: A Terminal.Gui v2 MVVM Demo");
    }

    public void Receive(TestCompletedMessage message)
    {
        _navigationService.ShowModal<ResultsViewModel, bool>(vm => vm.Initialize(message.Result));
    }

    public async void Receive(TestResetMessage message)
    {
        try
        {
            _navigationService.NavigateTo<TypingViewModel>(vm =>
            {
                vm.InitializeAsync().Wait();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling test reset");
        }
    }

    public async void Receive(ShowResultDialogMessage message)
    {
        try
        {
            TestResult result = await _statsRepository.GetTestResultAsync();
            _navigationService.ShowModal<ResultsViewModel, bool>(
                (vm) =>
                {
                    vm.Initialize(result);
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing random result");
        }
    }
}
