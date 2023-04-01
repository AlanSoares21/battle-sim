namespace BattleSimulator.Server.Hubs.EventHandling;

public class EventsQueue : IEventsQueue
{
    private Queue<IGameEvent> _queue;

    public EventsQueue() {
        _queue = new();
    }

    public IGameEvent Dequeue() => _queue.Dequeue();

    public void Enqueue(IGameEvent gameEvent)
    {
        _queue.Enqueue(gameEvent);
    }

    public bool IsEmpty() => _queue.Count == 0;
}