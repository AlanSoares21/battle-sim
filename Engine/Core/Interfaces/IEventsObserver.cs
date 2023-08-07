using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleSimulator.Engine.Interfaces;

public interface IEventsObserver 
{
    void SkillDamage(
        string skillName, 
        string sourceId, 
        string targetId,
        Coordinate targetCurrentHealth);
    Task ManaRecovered();
    Task Moved(Dictionary<string, Coordinate> entitiesMovedTo);
}