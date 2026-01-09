using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngineFactory : IGameEngineFactory
{
    private readonly ITextProvider _textProvider;
    private readonly ILoggerFactory _loggerFactory;

    // The factory gets all the DI-managed services.
    public GameEngineFactory(ITextProvider textProvider, ILoggerFactory loggerFactory)
    {
        _textProvider = textProvider;
        _loggerFactory = loggerFactory;
    }

    // The Create method uses the runtime data to construct the GameEngine.
    public GameEngine Create(GameOptions options)
    {
        var engineLogger = _loggerFactory.CreateLogger<GameEngine>();

        return new GameEngine(_textProvider, options, engineLogger);
    }
}

public interface IGameEngineFactory
{
    GameEngine Create(GameOptions options);
}
