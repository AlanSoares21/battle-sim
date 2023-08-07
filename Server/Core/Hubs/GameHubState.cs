namespace BattleSimulator.Server.Hubs;

public class GameHubState : IGameHubState
{
    private ILogger<GameHubState> _logger;
    public GameHubState(
        IConnectionMapping connectionMapping, 
        ILogger<GameHubState> logger,
        IBattleRequestCollection battleRequests,
        IBattleCollection battles
    ) {
        Connections = connectionMapping;
        BattleRequests = battleRequests;
        Battles = battles;
        _logger = logger;
    }
    public IConnectionMapping Connections { get; private set; }

    public IBattleRequestCollection BattleRequests { get; private set; }

    public IBattleCollection Battles { get; private set; }
}