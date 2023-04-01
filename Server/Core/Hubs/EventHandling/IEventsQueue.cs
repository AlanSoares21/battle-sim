namespace BattleSimulator.Server.Hubs.EventHandling;

public interface IEventsQueue
{
    IGameEvent Dequeue();
    void Enqueue(IGameEvent gameEvent);
    bool IsEmpty();
}