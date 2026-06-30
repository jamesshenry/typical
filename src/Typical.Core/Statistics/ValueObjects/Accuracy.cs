using Vogen;

namespace Typical.Core.Statistics;

[ValueObject<double>]
public readonly partial struct Accuracy
{
    public override string ToString()
    {
        return $"{Value:F1}";
    }
}
