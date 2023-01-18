using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IGameHubState
{
    IConnectionMapping Connections { get; }
    IBattleRequestCollection BattleRequests { get; }
    IBattleCollection Battles { get; }
    IMovementIntentionCollection MovementIntentions { get; }
}