namespace Typical.Core.Text;

/// <summary>
/// Represents a piece of text to be used in a typing game,
/// including the text itself and relevant metadata.
/// This is a generic DTO, decoupled from any specific data source.
/// </summary>
public record TextSample
{
    /// <summary>
    /// A unique identifier from the original data source, if available.
    /// This is useful for features like "Play Next Quote".
    /// </summary>
    public int? SourceId { get; init; }

    /// <summary>
    /// The text the user will be typing.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// The generic "source" of the text (e.g., an author's name, a book title, "Common Words")._
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// The number of words in the text.
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// The number of characters in the text.
    /// </summary>
    public int CharCount { get; init; }
}
