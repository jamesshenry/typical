using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Typical.Binding;

public static class BindingExtensions
{
    /// <summary>
    /// Generic One-Way Binding: VM -> UI
    /// Works for strings, bools, ints, or custom objects.
    /// </summary>
    public static IDisposable Bind<T>(
        this ObservableObject viewModel,
        string propertyName,
        Func<T> getter,
        Action<T> updateUi
    )
    {
        void Handler(object? sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, propertyName, StringComparison.Ordinal))
            {
                updateUi(getter());
            }
        }

        viewModel.PropertyChanged += Handler;
        updateUi(getter());

        return new DisposableAction(() => viewModel.PropertyChanged -= Handler);
    }

    /// <summary>
    /// Two-Way String Binding: VM <-> TextField
    /// </summary>
    public static IDisposable BindText(
        this ObservableObject viewModel,
        string propertyName,
        View target,
        Func<string> getter,
        Action<string>? setter = null
    )
    {
        var vmToUi = viewModel.Bind(
            propertyName,
            getter,
            val =>
            {
                if (target.Text != val)
                {
                    target.Text = val;
                    target.SetNeedsDraw();
                }
            }
        );

        if (setter != null)
        {
            void OnTextChanged(object? s, EventArgs e) => setter(target.Text);
            target.TextChanged += OnTextChanged;

            return new DisposableAction(() =>
            {
                vmToUi.Dispose();
                target.TextChanged -= OnTextChanged;
            });
        }

        return vmToUi;
    }

    /// <summary>
    /// Two-Way Boolean Binding: VM <-> CheckBox
    /// </summary>
    public static IDisposable BindChecked(
        this ObservableObject viewModel,
        string propertyName,
        CheckBox checkBox,
        Func<bool> getter,
        Action<bool> setter
    )
    {
        var vmToUi = viewModel.Bind(
            propertyName,
            getter,
            val =>
            {
                var newState = val ? CheckState.Checked : CheckState.UnChecked;
                if (checkBox.CheckedState != newState)
                {
                    checkBox.CheckedState = newState;
                    checkBox.SetNeedsDraw();
                }
            }
        );

        void OnUiChanged(object? s, EventArgs e) =>
            setter(checkBox.CheckedState == CheckState.Checked);
        checkBox.Accepted += OnUiChanged;

        return new DisposableAction(() =>
        {
            vmToUi.Dispose();
            checkBox.Accepted -= OnUiChanged;
        });
    }

    /// <summary>
    /// Command: Connects a ViewModel command to a Button.
    /// </summary>
    public static IDisposable BindCommand(
        this ObservableObject _,
        IRelayCommand command,
        Button button
    )
    {
        void UpdateEnabled(object? s, EventArgs e) => button.Enabled = command.CanExecute(null);
        void OnAccept(object? s, EventArgs e) => command.Execute(null);

        command.CanExecuteChanged += UpdateEnabled;
        button.Accepting += OnAccept;
        button.Enabled = command.CanExecute(null);

        return new DisposableAction(() =>
        {
            command.CanExecuteChanged -= UpdateEnabled;
            button.Accepting -= OnAccept;
        });
    }
}
