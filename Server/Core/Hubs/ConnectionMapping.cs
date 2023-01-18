using System.Collections.Concurrent;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class ConnectionMapping : IConnectionMapping
{
    ConcurrentDictionary<string, string> connections;
    public ConnectionMapping() {
        connections = new();
    }

    public string GetConnectionId(string userId) => connections[userId];

    public List<string> UsersIds() => 
        connections.Select(v => v.Key).ToList();

    public bool TryAdd(string userId, string connectionId) => 
        connections.TryAdd(userId, connectionId);

    public bool TryRemove(string userId, string connectionId) => 
        connections.TryRemove(new(userId, connectionId));
}