namespace BattleSimulator.Server.Hubs;

public interface IBattleEventsHandler {
    void Attack(string target, string caller);
}