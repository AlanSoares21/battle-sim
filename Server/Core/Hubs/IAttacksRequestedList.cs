namespace BattleSimulator.Server.Hubs;

public interface IAttacksRequestedList
{
    bool RegisterAttack(string source, string target);
    IEnumerable<KeyValuePair<string, string>> ListAttacks();
    bool RemoveAttack(string source);
}