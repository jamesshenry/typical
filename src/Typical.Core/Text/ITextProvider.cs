namespace Typical.Core.Text;

public interface ITextProvider
{
    Task<string> GetTextAsync();
}
