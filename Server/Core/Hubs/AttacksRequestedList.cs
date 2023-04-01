using System.Collections.Concurrent;

namespace BattleSimulator.Server.Hubs;

public class AttacksRequestedList : IAttacksRequestedList
{
    ConcurrentDictionary<string, string> _attacks = new();

    public IEnumerable<KeyValuePair<string, string>> ListAttacks() =>
        _attacks.AsEnumerable();

    public bool RegisterAttack(string source, string target)
    {
        return _attacks.AddOrUpdate(source, target, (_, _) => target) == target;
    }

    public bool RemoveAttack(string source) => 
        _attacks.Remove(source,  out _);
}