using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Stanza.TerminalGui;

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

        if (view is IStanzaView stanzaView)
        {
            stanzaView.ViewModel = vm;
        }

        if (view is not Dialog dialog)
        {
            throw new InvalidOperationException(
                $"View for {typeof(TViewModel).Name} must be a Dialog."
            );
        }

        void StopRequest(object? s, EventArgs e) => _app.RequestStop();
        vm.RequestClose += StopRequest;

        try
        {
            _app.Run(dialog);
        }
        finally
        {
            vm.RequestClose -= StopRequest;

            try
            {
                dialog.Dispose();
            }
            catch { }
        }

        return vm.Result;
    }
}
