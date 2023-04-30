using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs.EventHandling;

public interface IBattleEventsHandler {
    void Attack(string target, string caller);
    void Skill(string skillName, string target, CurrentCallerContext caller);
}