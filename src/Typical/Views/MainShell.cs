using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.Interfaces;
using Typical.Core.ViewModels;
using Typical.Navigation;

namespace Typical.Views;

public class MainShell : Window
{
    private readonly MainViewModel _viewModel;
    private readonly INavigationService _navService;
    private readonly IServiceProvider _serviceProvider;
    private readonly View _contentContainer;
    private readonly Label _statusLabel;
    private readonly BindingContext _bindingContext;

    public MainShell(MainViewModel viewModel, INavigationService navService, IServiceProvider sp)
    {
        _viewModel = viewModel;
        _navService = navService;
        _serviceProvider = sp;
        _bindingContext = new BindingContext();
        BorderStyle = LineStyle.RoundedDashed;
        Title = _viewModel.AppTitle;

        _statusLabel = new Label { Y = Pos.AnchorEnd(1), Width = Dim.Fill() };

        _contentContainer = new FrameView
        {
            Title = "Content Frame",
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2,
            CanFocus = true,
            BorderStyle = DefaultBorderStyle,
        };

        Add(_contentContainer, _statusLabel);

        _bindingContext.AddBinding(
            _viewModel.BindText(
                nameof(_viewModel.StatusText),
                _statusLabel,
                () => _viewModel.StatusText
            )
        );

        _navService.PropertyChanged += OnNavServicePropertyChanged;

        _viewModel.NavigateToGameViewCommand.Execute(null);

        this.Activating += (s, e) =>
        {
            if (e.Context?.Binding is MouseBinding { MouseEvent: { } mouse })
            {
                if (mouse.Flags.HasFlag(MouseFlags.LeftButtonClicked))
                {
                    this.SetFocus();
                }

                e.Handled = true;
            }
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bindingContext.Dispose();
        }
        base.Dispose(disposing);
    }

    private void OnNavServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
        {
            UpdateContent(_navService.CurrentViewModel);
        }
    }

    private void UpdateContent(ObservableObject? viewModel)
    {
        if (viewModel == null)
            return;

        _contentContainer.RemoveAll();

        var view = ViewLocator.GetView(_serviceProvider, viewModel);

        view.X = Pos.Center();
        view.Y = Pos.Center();

        _contentContainer.Add(view);

        view.SetFocus();
    }
}
