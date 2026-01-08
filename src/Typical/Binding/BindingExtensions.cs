using CommunityToolkit.Mvvm.ComponentModel;
using Terminal.Gui.Views;

namespace Typical.Binding;

/// <summary>
/// Extension methods for binding Terminal.Gui controls to ViewModel properties.
/// </summary>
public static class BindingExtensions
{
    // Extension methods for binding Terminal.Gui controls to ViewModel properties

    /// <summary>
    /// Binds a Label's Text property one-way to a ViewModel property.
    /// </summary>
    public static IDisposable BindTextOneWay(
        this Label label,
        ObservableObject viewModel,
        Func<string> getter,
        string propertyName
    )
    {
        // Initial sync
        label.Text = getter();

        // Subscribe to changes
        void Handler(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == propertyName)
            {
                label.Text = getter();
            }
        }

        viewModel.PropertyChanged += Handler;

        return new DisposableAction(() => viewModel.PropertyChanged -= Handler);
    }

    /// <summary>
    /// Binds a TextField's Text property two-way to a ViewModel property.
    /// </summary>
    public static IDisposable BindTextTwoWay(
        this TextField textField,
        ObservableObject viewModel,
        Func<string> getter,
        Action<string> setter,
        string propertyName
    )
    {
        // Initial sync
        textField.Text = getter();

        // View → ViewModel
        void TextChangedHandler(object? sender, EventArgs e)
        {
            setter(textField.Text.ToString() ?? string.Empty);
        }

        // ViewModel → View
        void PropertyChangedHandler(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (e.PropertyName == propertyName)
            {
                textField.Text = getter();
            }
        }

        textField.TextChanged += TextChangedHandler;
        viewModel.PropertyChanged += PropertyChangedHandler;

        return new DisposableAction(() =>
        {
            textField.TextChanged -= TextChangedHandler;
            viewModel.PropertyChanged -= PropertyChangedHandler;
        });
    }

    /// <summary>
    /// Binds a CheckBox's CheckedState property two-way to a ViewModel boolean property.
    /// </summary>
    public static IDisposable BindCheckedTwoWay(
        this CheckBox checkBox,
        ObservableObject viewModel,
        Func<bool> getter,
        Action<bool> setter,
        string propertyName
    )
    {
        // Initial sync
        checkBox.CheckedState = getter() ? CheckState.Checked : CheckState.UnChecked;

        // View → ViewModel
        void AcceptedHandler(object? sender, EventArgs e)
        {
            setter(checkBox.CheckedState == CheckState.Checked);
        }

        // ViewModel → View
        void PropertyChangedHandler(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            if (e.PropertyName == propertyName)
            {
                checkBox.CheckedState = getter() ? CheckState.Checked : CheckState.UnChecked;
            }
        }

        checkBox.Accepted += AcceptedHandler;
        viewModel.PropertyChanged += PropertyChangedHandler;

        return new DisposableAction(() =>
        {
            checkBox.Accepted -= AcceptedHandler;
            viewModel.PropertyChanged -= PropertyChangedHandler;
        });
    }
}
