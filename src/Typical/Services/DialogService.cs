using Terminal.Gui.App;
using Terminal.Gui.Views;
using Typical.Core.Interfaces;

namespace Typical.Services;

public class DialogService : IDialogService
{
    private readonly IApplication _app;

    public DialogService(IApplication app)
    {
        _app = app;
    }

    public bool Confirm(
        string title,
        string message,
        string okText = "Yes",
        string cancelText = "No"
    )
    {
        int? result = MessageBox.Query(_app, title, message, okText, cancelText);
        return result == 0;
    }

    public void ShowInfo(string title, string message)
    {
        MessageBox.Query(_app, title, message, "Ok");
    }

    public void ShowError(string title, string message)
    {
        MessageBox.ErrorQuery(_app, title, message);
    }
}
