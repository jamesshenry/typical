using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
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

    public NavigationService(
        IServiceProvider services,
        IApplication app,
        IMessenger? messenger = null
    )
    {
        _services = services;
        _app = app;
        _messenger = messenger ?? WeakReferenceMessenger.Default;
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
        (CurrentViewModel as IBindableView)?.OnNavigatedFrom();

        CurrentViewModel = _services.GetRequiredService<TViewModel>();

        (CurrentViewModel as IBindableView)?.OnNavigatedTo();

        _messenger.Send(new NavigationChangedMessage(CurrentViewModel));
    }

    public TResult? ShowModal<TViewModel, TResult>(Action<TViewModel>? configure = null)
        where TViewModel : class, IModalViewModel<TResult>
    {
        var vm = _services.GetRequiredService<TViewModel>();
        configure?.Invoke(vm);
        var view = ViewLocator.GetView(_services, vm);

        if (view is IRunnable runnable)
        {
            EventHandler? handler = null;
            handler = (s, e) =>
            {
                _app.RequestStop();
                vm.RequestClose -= handler;
            };
            vm.RequestClose += handler;
            _app.Run(runnable);
        }
        else
        {
            var host = new Dialog { Title = "Modal Host" };
            host.Add(view);
            _app.Run(host);
        }

        return vm.Result;
    }
}
