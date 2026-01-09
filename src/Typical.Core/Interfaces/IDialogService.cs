namespace Typical.Core.Interfaces;

public interface IDialogService
{
    bool Confirm(string title, string message, string okText = "Yes", string cancelText = "No");
    void ShowInfo(string title, string message);
    void ShowError(string title, string message);
}
