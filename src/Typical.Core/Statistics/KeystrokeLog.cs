namespace Typical.Core.Statistics;

public record struct KeystrokeLog(string Value, KeystrokeType Type, long Timestamp, long OffsetMs);
