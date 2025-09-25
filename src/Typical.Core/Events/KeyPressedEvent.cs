using Typical.Core.Statistics;

namespace Typical.Core.Events;

internal record KeyPressedEvent(char Character, KeystrokeType Type, int Position);
