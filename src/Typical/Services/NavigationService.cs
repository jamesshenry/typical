using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Navigation;

namespace Typical.Services;

public class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _services;
    private readonly IApplication _app;
    private readonly IMessenger _messenger;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(
        IServiceProvider services,
        IApplication app,
        IMessenger messenger,
        ILogger<NavigationService> logger
    )
    {
        _services = services;
        _app = app;
        _messenger = messenger;
        _logger = logger;
    }

    private ObservableObject? _currentViewModel;

    public ObservableObject CurrentViewModel
    {
        get => _currentViewModel!;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public void NavigateTo<TViewModel>()
        where TViewModel : ObservableObject
    {
        (CurrentViewModel as INavigatableView)?.OnNavigatedFrom();

        CurrentViewModel = _services.GetRequiredService<TViewModel>();

        (CurrentViewModel as INavigatableView)?.OnNavigatedTo();

        _messenger.Send(new NavigationChangedMessage(CurrentViewModel));
    }

    public void NavigateTo<TViewModel>(Action<TViewModel> configure)
        where TViewModel : ObservableObject
    {
        (CurrentViewModel as INavigatableView)?.OnNavigatedFrom();
        var nextViewModel = _services.GetRequiredService<TViewModel>();

        configure?.Invoke(nextViewModel);

        CurrentViewModel = nextViewModel;

        (CurrentViewModel as INavigatableView)?.OnNavigatedTo();

        _messenger.Send(new NavigationChangedMessage(nextViewModel));
    }

    public TResult? ShowModal<TViewModel, TResult>(Action<TViewModel>? configure = null)
        where TViewModel : class, IModalViewModel<TResult>
    {
        var vm = _services.GetRequiredService<TViewModel>();
        configure?.Invoke(vm);
        var view = ViewLocator.GetView(_services, vm);

        EventHandler? handler = null;
        handler = (s, e) => _app.RequestStop();
        vm.RequestClose += handler;

        try
        {
            if (view is Dialog dialog)
            {
                _logger.LogInformation(
                    "Showing modal dialog directly for {ViewModelType}",
                    typeof(TViewModel).Name
                );
                _app.Run(dialog);
            }
            else if (view is IRunnable runnable)
            {
                _logger.LogInformation(
                    "Showing runnable modal view for {ViewModelType}: {ViewType}",
                    typeof(TViewModel).Name,
                    view.GetType().Name
                );
                _app.Run(runnable);
            }
            else
            {
                _logger.LogInformation(
                    "Wrapping non-runnable modal view for {ViewModelType}: {ViewType}",
                    typeof(TViewModel).Name,
                    view.GetType().Name
                );
                var host = new Dialog { Title = "Modal Host" };
                host.Add(view);
                _app.Run(host);
            }
        }
        finally
        {
            vm.RequestClose -= handler;
        }

        return vm.Result;
    }
}
