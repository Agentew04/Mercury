using JetBrains.Annotations;
using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Common;

/// <summary>
/// Implements an event bus to allow decoupled modules to communicate with each other by publishing and subscribing
/// to different event types.
/// </summary>
/// <remarks>
/// This module uses a publish-subscribe system.
/// </remarks>
public sealed class EventBus {
    private readonly Dictionary<Type, object> handlers = new(); // nao tem problema object aqui pq sao todos classes

    #region Stateless

    /// <summary>
    /// Subscribes a handler to a specific event type. Whenever an event of that type is published,
    /// <paramref name="handler"/> is invoked.
    /// </summary>
    /// <param name="handler"></param>
    /// <typeparam name="TEvent"></typeparam>
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) {
        return Subscribe<Action<TEvent>,TEvent>(
            handler,
            static (h,e) => h(e)
        );
    }
    
    public IDisposable Subscribe<TEvent>(Func<TEvent, ValueTask> handler) {
        return Subscribe<Func<TEvent,ValueTask>,TEvent>(
            handler,
            static (h,e) => h(e)
        );
    }

    #endregion

    #region Stateful

    /// <summary>
    /// Subscribes a handler to a specific event type. Whenever an event of that type is published,
    /// <paramref name="handler"/> is invoked.
    /// </summary>
    /// <param name="handler"></param>
    /// /// <param name="state"></param>
    /// <typeparam name="TEvent"></typeparam>
    public IDisposable Subscribe<TState,TEvent>(TState state, [RequireStaticDelegate(IsError = true)] Action<TState,TEvent> handler) {
        HandlerList<TEvent> list = GetHandlers<TEvent>();
        StateEventHandler<TState,TEvent> entry = new(handler, null, state); // with state
        list.Add(entry);
        return new Subscription<TEvent>(list,entry);
    }
    
    public IDisposable Subscribe<TState,TEvent>(TState state, Func<TState,TEvent, ValueTask> handler) {
        HandlerList<TEvent> list = GetHandlers<TEvent>();
        StateEventHandler<TState,TEvent> entry = new(null, handler, state);  // with state
        list.Add(entry);
        return new Subscription<TEvent>(list,entry);
    }

    #endregion

    /// <summary>
    /// Publishes an event of type <typeparamref name="T"/>. All handlers subscribed to that event type will be invoked
    /// in no particular order. The event data is passed as an argument to the handlers.
    /// </summary>
    /// <param name="e"></param>
    /// <typeparam name="T"></typeparam>
    public void Publish<T>(in T e) {
        if (handlers.TryGetValue(typeof(T), out object? list)) {
            ((HandlerList<T>)list).Publish(e);
        }
    }
    
    public ValueTask PublishAsync<T>(T e) {
        if (handlers.TryGetValue(typeof(T), out object? list)) {
            return ((HandlerList<T>)list).PublishAsync(e);
        }
        return ValueTask.CompletedTask;
    }

    private HandlerList<T> GetHandlers<T>() {
        if (!handlers.TryGetValue(typeof(T), out object? value)) {
            value = new HandlerList<T>();
            handlers.Add(typeof(T), value);
        }
        return (HandlerList<T>)value;
    }

    #region Helper Classes

    private interface IEventHandler<in T> {
        ValueTask Invoke(T e);
    }

    private sealed class EventHandler<TEvent> : IEventHandler<TEvent> {
        private Action<TEvent>? syncHandler;
        private Func<TEvent, ValueTask>? asyncHandler;
        
        public EventHandler(Action<TEvent>? syncHandler, Func< TEvent, ValueTask>? asyncHandler) {
            this.syncHandler = syncHandler;
            this.asyncHandler = asyncHandler;
        }
        
        public ValueTask Invoke(TEvent e) {
            if (syncHandler is not null) {
                syncHandler(e);
                return ValueTask.CompletedTask;
            }

            return asyncHandler!(e);
        }
    }

    private sealed class StateEventHandler<TState,TEvent> : IEventHandler<TEvent> {
        private Action<TState, TEvent>? syncHandler;
        private Func<TState, TEvent, ValueTask>? asyncHandler;
        private TState state;
        
        public StateEventHandler(Action<TState,TEvent>? syncHandler, Func<TState, TEvent, ValueTask>? asyncHandler, TState state) {
            this.syncHandler = syncHandler;
            this.asyncHandler = asyncHandler;
            this.state = state;
        }

        public ValueTask Invoke(TEvent e) {
            if (syncHandler is not null) {
                syncHandler(state, e);
                return ValueTask.CompletedTask;
            }

            return asyncHandler!(state, e);
        }
    }

    private sealed class HandlerList<T> {
        private readonly List<IEventHandler<T>> handlers = [];

        public void Add(IEventHandler<T> handler) {
            handlers.Add(handler);
        }

        public void Remove(IEventHandler<T> handler) {
            handlers.Remove(handler);
        }

        public void Publish(T e) {
            foreach (IEventHandler<T> handler in handlers) {
                _ = handler.Invoke(e);
            }
        }

        public async ValueTask PublishAsync(T e) {
            foreach (IEventHandler<T> handler in handlers) {
                await handler.Invoke(e);
            }
        }
    }
    
    private class Subscription<TEvent> : IDisposable {

        private HandlerList<TEvent> list;
        private readonly IEventHandler<TEvent> handler;
        
        public Subscription(HandlerList<TEvent> list, IEventHandler<TEvent> handler) {
            this.list = list;
            this.handler = handler;
        } 
        
        public void Dispose() {
            HandlerList<TEvent> current = Interlocked.Exchange(ref list, null);
            current?.Remove(handler);
        }
    }

    #endregion
}


