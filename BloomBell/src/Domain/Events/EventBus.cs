using System;
using System.Collections.Generic;
using System.Threading;
using BloomBell.src.Infrastructure.Game;

namespace BloomBell.src.Domain.Events;

/// <summary>
/// A lightweight, type-keyed publish/subscribe event bus.
/// Subscribers register handlers for a specific event type and are notified
/// whenever that event is published. This decouples producers from consumers,
/// eliminating race conditions caused by direct state mutation.
/// </summary>
public sealed class EventBus : IDisposable
{
    private readonly Lock syncLock = new();
    private readonly Dictionary<Type, List<Delegate>> subscribers = [];

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        lock (syncLock)
        {
            var type = typeof(TEvent);
            if (!subscribers.TryGetValue(type, out var handlers))
            {
                handlers = [];
                subscribers[type] = handlers;
            }

            handlers.Add(handler);
        }
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        lock (syncLock)
        {
            var type = typeof(TEvent);
            if (subscribers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public void Publish<TEvent>(TEvent eventData)
    {
        List<Delegate> snapshot;

        lock (syncLock)
        {
            var type = typeof(TEvent);
            if (!subscribers.TryGetValue(type, out var handlers) || handlers.Count == 0)
                return;

            snapshot = [.. handlers];
        }

        foreach (var handler in snapshot)
        {
            try
            {
                ((Action<TEvent>)handler).Invoke(eventData);
            }
            catch (Exception ex)
            {
                GameServices.PluginLog.Error(ex, $"EventBus handler threw for {typeof(TEvent).Name}");
            }
        }
    }

    public void Dispose()
    {
        lock (syncLock)
        {
            subscribers.Clear();
        }
    }
}
