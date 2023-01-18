using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IConnectionMapping
{
    List<string> UsersIds();
    string GetConnectionId(string userId);
    bool TryAdd(string userId, string connectionId);
    bool TryRemove(string userId, string connectionId);
}