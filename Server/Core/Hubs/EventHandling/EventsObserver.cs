using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs.EventHandling;

public class EventsObserver : IEventsObserver
{
    List<Action<string, string, string, Coordinate>> _skillDamageSubscribers;
    List<Func<Task>> _manaRecoverSubscribers;

    public EventsObserver() 
    {
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
}