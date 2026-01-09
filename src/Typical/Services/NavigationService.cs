using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui.App;
using Terminal.Gui.Views;
using Typical.Core.Interfaces;
using Typical.Navigation;

namespace Typical.Services;

public class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _services;
    private readonly IApplication _app;

    public NavigationService(IServiceProvider services, IApplication app)
    {
        _services = services;
        _app = app;
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
        if (CurrentViewModel is IBindableView currentViewModel)
        {
            currentViewModel.OnNavigatedFrom();
        }

        CurrentViewModel = _services.GetRequiredService<TViewModel>();

        if (CurrentViewModel is IBindableView newViewModel)
        {
            newViewModel.OnNavigatedTo();
        }
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
