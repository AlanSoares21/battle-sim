namespace BattleSimulator.Engine.Interfaces;

public interface IEventsObserver 
{
    void SkillDamage(
        string skillName, 
        string sourceId, 
        string targetId,
        Coordinate targetCurrentHealth);
}