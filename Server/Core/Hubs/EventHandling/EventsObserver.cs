using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs.EventHandling;

public class EventsObserver : IEventsObserver
{
    public void SkillDamage(
        string skillName, 
        string sourceId, 
        string targetId, 
        Coordinate targetCurrentHealth)
    {
        throw new NotImplementedException();
    }
}