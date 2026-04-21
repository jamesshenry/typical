namespace Typical.Core.Interfaces;

/// <summary>
/// Interface for views that support navigation lifecycle events.
/// </summary>
public interface INavigatableView
{
    /// <summary>
    /// Called when the view is navigated to.
    /// </summary>
    void OnNavigatedTo();

    /// <summary>
    /// Called when the view is navigated away from.
    /// </summary>
    void OnNavigatedFrom();
}
