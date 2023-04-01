using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Server.Hubs.EventHandling;

public class GameEvent : IGameEvent
{
    public GameEvent(string source, string target) {
        this.Source = source;
        this.Target = target;
        this.Type = GameEventType.Unknowed;
    }
    public string Source { get; private set; }

    public string Target { get; private set; }

    public GameEventType Type { get; private set; }

    public ISkillBase? Skill { get; private set; }

    public void SetSkill(ISkillBase skill)
    {
        this.Type = GameEventType.Skill;
        this.Skill = skill;
    }
}