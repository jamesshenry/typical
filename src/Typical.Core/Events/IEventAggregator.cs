using System.Reflection.Metadata;

namespace Typical.Core.Events;

public interface IEventAggregator
{
    void Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : class;
    void Unsubscribe<TEvent>(Action<TEvent> handler)
        where TEvent : class;
    void Publish<TEvent>(TEvent eventToPublish)
        where TEvent : class;
}

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];
    private readonly Lock _aggregatorLock = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        lock (_aggregatorLock)
        {
            if (!_handlers.TryGetValue(eventType, out List<Delegate>? value))
            {
                value = [];
                _handlers[eventType] = value;
            }

            value.Add(handler);
        }
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
        where TEvent : class
    {
        var eventType = typeof(TEvent);
        lock (_aggregatorLock)
        {
            if (_handlers.TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers.Remove(handler);

                if (eventHandlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }
    }

    public void Publish<TEvent>(TEvent eventToPublish)
        where TEvent : class
    {
        var eventType = typeof(TEvent);
        List<Delegate> handlersSnapshot;

        lock (_aggregatorLock)
        {
            if (!_handlers.TryGetValue(eventType, out var eventHandlers))
            {
                return;
            }

            handlersSnapshot = eventHandlers.ToList();
        }

        foreach (var handler in handlersSnapshot)
        {
            ((Action<TEvent>)handler)(eventToPublish);
        }
    }
}
