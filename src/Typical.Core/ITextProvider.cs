namespace Typical.Core;

public interface ITextProvider
{
    Task<string> GetTextAsync();
}
