using Vogen;

namespace Typical.Core.Statistics;

[ValueObject<double>]
public readonly partial struct Wpm
{
    public override readonly string ToString()
    {
        return $"{Value:F1}";
    }
}
