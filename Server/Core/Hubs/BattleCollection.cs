using System.Collections.Concurrent;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs;

public class BattleCollection : IBattleCollection
{
    ConcurrentDictionary<Guid, IBattle> _data;
    public BattleCollection() {
        _data = new();
    }

    public IBattle Get(Guid battleId) => _data[battleId];

    public Guid GetBattleIdByEntity(string entityIdentifier)
    {
        foreach (Guid battleId in _data.Keys)
            if (_data[battleId].Entities
                .Exists(entity => entity.Identifier == entityIdentifier)
            )
                return battleId;
        throw new KeyNotFoundException($"Not found a battle with entity {entityIdentifier}");
    }

    public bool TryAdd(IBattle battle) => 
        _data.TryAdd(battle.Id, battle);

    public bool TryRemove(IBattle battle) => 
        _data.TryRemove(new(battle.Id, battle));
}