using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core;

public class GameEngineFactory : IGameEngineFactory
{
    private readonly ITextProvider _textProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILoggerFactory _loggerFactory;

    // The factory gets all the DI-managed services.
    public GameEngineFactory(
        ITextProvider textProvider,
        IEventAggregator eventAggregator,
        ILoggerFactory loggerFactory
    )
    {
        _textProvider = textProvider;
        _eventAggregator = eventAggregator;
        _loggerFactory = loggerFactory;
    }

    // The Create method uses the runtime data to construct the GameEngine.
    public GameEngine Create(GameOptions options)
    {
        // We need a fresh GameStats for each GameEngine instance.
        var statsLogger = _loggerFactory.CreateLogger<GameStats>();
        var stats = new GameStats(_eventAggregator, null, statsLogger);

        var engineLogger = _loggerFactory.CreateLogger<GameEngine>();

        return new GameEngine(_textProvider, _eventAggregator, options, stats, engineLogger);
    }
}

public interface IGameEngineFactory
{
    GameEngine Create(GameOptions options);
}
