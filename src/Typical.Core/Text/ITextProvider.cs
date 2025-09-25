namespace Typical.Core.Text;

public interface ITextProvider
{
    Task<TextSample> GetTextAsync();
}
