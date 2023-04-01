namespace BattleSimulator.Server.Hubs.EventHandling;

public interface IGameEvent
{
    string Source { get; }
    string Target { get; }
    GameEventType Type { get; }
    string? SkillName { get; }
}

public enum GameEventType {
    Skill = 0
}