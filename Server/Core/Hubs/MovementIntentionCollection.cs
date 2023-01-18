using System.Collections.Concurrent;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class MovementIntentionCollection : IMovementIntentionCollection
{
    ConcurrentDictionary<string, MovementIntention> _data;
    public MovementIntentionCollection() {
        _data = new();
    }

    public MovementIntention Get(string entityIdentifier) => 
        _data[entityIdentifier];

    public IEnumerable<MovementIntention> List() => _data.Values;

    public bool TryAdd(MovementIntention intention) {
        if (_data.ContainsKey(intention.entityIdentifier))
            return _data.TryUpdate(
                intention.entityIdentifier, 
                intention, 
                _data[intention.entityIdentifier]);   
        return _data.TryAdd(intention.entityIdentifier, intention);
    }

    public bool TryRemove(MovementIntention intention) => 
        _data.TryRemove(new(intention.entityIdentifier, intention));
}