using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Typical.Core.Data;

public class Quote
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required string Author { get; set; }
    public List<string> Tags { get; set; } = [];
    public int WordCount { get; set; }
    public int CharCount { get; set; }
}
