using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Server.Hubs.EventHandling;

public interface IGameEvent
{
    string Source { get; }
    string Target { get; }
    GameEventType Type { get; }
    ISkillBase? Skill { get; }

    void SetSkill(ISkillBase skill);
}

public enum GameEventType {
    Unknowed = 0,
    Skill = 1
}