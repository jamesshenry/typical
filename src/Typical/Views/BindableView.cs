using CommunityToolkit.Mvvm.ComponentModel;
using Terminal.Gui.ViewBase;
using Typical.Binding;
using Typical.Core.Interfaces;

namespace Typical.Views;

/// <summary>
/// Base class for Views that are bound to ViewModels.
/// Provides lifecycle management and binding context.
/// </summary>
public abstract class BindableView<TViewModel> : View, IBindableView
    where TViewModel : ObservableObject
{
    /// <summary>
    /// The ViewModel instance.
    /// </summary>
    protected readonly TViewModel ViewModel;

    /// <summary>
    /// The binding context for managing bindings.
    /// </summary>
    protected readonly BindingContext BindingContext;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the ViewModelView class.
    /// </summary>
    protected BindableView(TViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = new BindingContext();

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        Initialized += (s, e) => SetupBindings();
    }

    /// <summary>
    /// Template method for setting up bindings.
    /// Override in derived classes to configure bindings.
    /// </summary>
    protected abstract void SetupBindings();

    /// <summary>
    /// Called when a ViewModel property changes.
    /// Override in derived classes for custom handling.
    /// </summary>
    protected virtual void OnViewModelPropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    ) { }

    /// <summary>
    /// Called when the view is navigated to.
    /// </summary>
    public virtual void OnNavigatedTo() { }

    /// <summary>
    /// Called when the view is navigated away from.
    /// </summary>
    public virtual void OnNavigatedFrom() { }

    /// <summary>
    /// Disposes the view and cleans up bindings.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            BindingContext.Dispose();
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}
