namespace BattleSimulator.Server.Hubs;

public class GameHubState : IGameHubState
{
    private ILogger<GameHubState> _logger;
    public GameHubState(
        IConnectionMapping connectionMapping, 
        ILogger<GameHubState> logger,
        IBattleRequestCollection battleRequests,
        IBattleCollection battles,
        IMovementIntentionCollection movementIntentions
    ) {
        Connections = connectionMapping;
        BattleRequests = battleRequests;
        Battles = battles;
        MovementIntentions = movementIntentions;
        _logger = logger;
    }
    public IConnectionMapping Connections { get; private set; }

    public IBattleRequestCollection BattleRequests { get; private set; }

    public IBattleCollection Battles { get; private set; }
    public IMovementIntentionCollection MovementIntentions { get; private set; }
}