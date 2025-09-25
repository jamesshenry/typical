namespace Typical.TUI.Views;

public interface IView
{
    // Renders the content of the view.
    Task RenderAsync();
}

public class StatsView : IView
{
    public Task RenderAsync()
    {
        throw new NotImplementedException();
    }
}

public class MainMenuView : IView
{
    public Task RenderAsync()
    {
        throw new NotImplementedException();
    }
}
