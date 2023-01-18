using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Builders;

public class GameEngineBuilder
{
    IGameHubState? _state;
    ILogger<GameEngine>? _logger;

    public GameEngineBuilder WithState(IGameHubState state) {
        _state = state;
        return this;
    }

    public GameEngine Build() 
    {
        if (_state is null)
            _state = FakeState();
        if (_logger is null)
            _logger = FakeLogger();
        return new GameEngine(_state, _logger);
    }

    ILogger<GameEngine> FakeLogger() => 
        A.Fake<ILogger<GameEngine>>();

    IGameHubState FakeState() => new GameHubStateBuilder().Build();
}