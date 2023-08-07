using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs.EventHandling;

public class EventsObserver : IEventsObserver
{
    List<Action<string, string, string, Coordinate>> _skillDamageSubscribers;
    List<Func<Task>> _manaRecoverSubscribers;
    List<Func<Dictionary<string, Coordinate>, Task>> _moveSubscribers;

    public EventsObserver() 
    {
        _moveSubscribers = new();
        _skillDamageSubscribers = new();
        _manaRecoverSubscribers = new();
    }

    public void SubscribeToSkillDamage(
        Action<string, string, string, Coordinate> subscriber)
    {
        _skillDamageSubscribers.Add(subscriber);
    }

    public void SubscribeToManaRecovered(Func<Task> subscriber)
    {
        _manaRecoverSubscribers.Add(subscriber);
    }

    public void SubscribeToMove(Func<Dictionary<string, Coordinate>, Task> subscriber)
    {
        _moveSubscribers.Add(subscriber);
    }

    public void SkillDamage(
        string skillName, 
        string sourceId, 
        string targetId, 
        Coordinate targetCurrentHealth)
    {
        foreach (var subscriber in _skillDamageSubscribers)
            subscriber(skillName, sourceId, targetId, targetCurrentHealth);
    }

    public async Task ManaRecovered()
    {
        foreach(var subscriber in _manaRecoverSubscribers)
            await subscriber();
    }

    public async Task Moved(Dictionary<string, Coordinate> entitiesMovedTo)
    {
        foreach(var subscriber in _moveSubscribers)
            await subscriber(entitiesMovedTo);
    }
}