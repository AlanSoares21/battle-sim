using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;
public interface IMovementIntentionCollection
{
    public IEnumerable<MovementIntention> List();
    public MovementIntention Get(string entityIdentifier);
    public bool TryAdd(MovementIntention intention);
    public bool TryRemove(MovementIntention intention);
}