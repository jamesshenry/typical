using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Binding;
using Typical.Core.Events;
using Typical.Core.ViewModels;
using Typical.Navigation;

namespace Typical.Views;

public class MainShell : Window, IRecipient<NavigationChangedMessage>
{
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly View _contentContainer;

    // private readonly Label _statusLabel;
    private readonly BindingContext _bindingContext;

    public MainShell(MainViewModel viewModel, IServiceProvider sp)
    {
        _viewModel = viewModel;
        _serviceProvider = sp;
        _bindingContext = new BindingContext();
        BorderStyle = LineStyle.RoundedDashed;
        Title = _viewModel.AppTitle;

        // _statusLabel = new Label { Y = Pos.AnchorEnd(1), Width = Dim.Fill() };
        var statsView = _serviceProvider.GetRequiredService<StatsView>();
        statsView.Y = Pos.AnchorEnd(3);
        statsView.X = Pos.Left(this);
        statsView.Width = Dim.Fill();
        statsView.Height = 3;
        _contentContainer = new FrameView
        {
            Title = "Content Frame",
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Dim.Fill(),
            Height = Dim.Fill() - 6,
            CanFocus = true,
            BorderStyle = DefaultBorderStyle,
        };

        Add(_contentContainer, statsView);

        _bindingContext.AddBinding(
            _viewModel.Bind(
                nameof(_viewModel.CurrentPage),
                () => _viewModel.CurrentPage,
                _ => UpdateContent(_viewModel.CurrentPage)
            )
        );

        WeakReferenceMessenger.Default.Register(this);

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
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
        base.Dispose(disposing);
    }

    public void Receive(NavigationChangedMessage message)
    {
        UpdateContent(message.Value);
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
