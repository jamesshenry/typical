namespace Typical.Core.Interfaces;

public interface IModalViewModel<TResult>
{
    // The result the modal will return (e.g., a bool, a string, or a complex object)
    TResult? Result { get; }

    // An event to tell the View: "I am done, please stop the loop"
    event EventHandler? RequestClose;
}
