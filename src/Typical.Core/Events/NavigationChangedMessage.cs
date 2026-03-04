using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Typical.Core.Events;

/// <summary>
/// Message sent when the active view model has changed.
/// </summary>
public class NavigationChangedMessage(ObservableObject value)
    : ValueChangedMessage<ObservableObject>(value);
