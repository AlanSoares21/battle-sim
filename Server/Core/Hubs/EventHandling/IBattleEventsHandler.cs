namespace BattleSimulator.Server.Hubs.EventHandling;

public interface IBattleEventsHandler {
    void Attack(string target, string caller);
}