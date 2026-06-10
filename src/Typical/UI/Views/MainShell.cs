using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Stanza.TerminalGui;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Typical.Core.ViewModels;
using Typical.Navigation;

namespace Typical.UI.Views;

[StanzaView<MainViewModel>]
public partial class MainShell : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FrameView _headerFrame;
    private readonly View _contentFrame;
    private readonly FrameView _footerFrame;
    private readonly View _leftSpacer;
    private readonly View _rightSpacer;

    public MainShell(MainViewModel viewModel, IServiceProvider sp)
    {
        _serviceProvider = sp;
        BorderStyle = LineStyle.None;
        Title = viewModel.AppTitle;

        _leftSpacer = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(15),
            Height = Dim.Fill(),
            CanFocus = false,
        };

        _rightSpacer = new View
        {
            X = Pos.AnchorEnd(),
            Y = 0,
            Width = Dim.Percent(15),
            Height = Dim.Fill(),
            CanFocus = false,
        };

        _headerFrame = new FrameView
        {
            Title = "Typical Header",
            X = Pos.Right(_leftSpacer),
            Y = 2,
            Width = Dim.Fill() - Dim.Width(_rightSpacer),
            Height = Dim.Auto(DimAutoStyle.Text, minimumContentDim: 1),
            BorderStyle = LineStyle.None,
        };
        var settingsView = _serviceProvider.GetRequiredService<SettingsView>();
        _headerFrame.Add(settingsView);
        _footerFrame = new FrameView
        {
            Title = "Typical Footer",
            X = Pos.Right(_leftSpacer),
            Y = Pos.AnchorEnd(3),
            Width = Dim.Fill() - Dim.Width(_rightSpacer),
            Height = 3,
            BorderStyle = LineStyle.HeavyDotted,
        };
        _contentFrame = new View
        {
            X = Pos.Right(_leftSpacer),
            Y = Pos.Bottom(_headerFrame),
            Width = Dim.Fill() - Dim.Width(_rightSpacer),
            Height = Dim.Fill() - Dim.Height(_footerFrame),
            CanFocus = true,
        };
        var statsView = _serviceProvider.GetRequiredService<StatsView>();
        statsView.Width = Dim.Fill();
        statsView.Height = Dim.Fill();
        _footerFrame.Add(statsView);
        Add(_leftSpacer, _rightSpacer, _headerFrame, _contentFrame, _footerFrame);

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

        ViewModel = viewModel;

        ViewModel.NavigateToTestViewCommand.Execute(null);
    }

    partial void OnApplyBindings(BindingContext context)
    {
        if (ViewModel is not null)
            this.Bind(ViewModel, vm => vm.CurrentPage, _ => UpdateContent(ViewModel.CurrentPage))
                .AddTo(_bindingContext);
    }

    private void UpdateContent(ObservableObject? viewModel)
    {
        if (viewModel == null)
            return;

        foreach (var child in _contentFrame.SubViews)
        {
            child.Dispose();
        }
        _contentFrame.RemoveAll();

        var view = ViewLocator.GetView(_serviceProvider, viewModel);

        view.X = Pos.Center();
        view.Y = Pos.Center();
        view.Width = Dim.Fill();
        view.Height = Dim.Fill();
        _contentFrame.Add(view);

        view.SetFocus();
    }
}
