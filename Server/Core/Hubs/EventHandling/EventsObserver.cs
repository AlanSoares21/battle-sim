using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs.EventHandling;

public class EventsObserver : IEventsObserver
{
    List<Action<string, string, string, Coordinate>> _skillDamageSubscribers;

    public EventsObserver() 
    {
        _skillDamageSubscribers = new();
    }

    public void SubscribeToSkillDamage(
        Action<string, string, string, Coordinate> subscriber)
    {
        _skillDamageSubscribers.Add(subscriber);
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

    public Task ManaRecovered()
    {
        throw new NotImplementedException();
    }
}