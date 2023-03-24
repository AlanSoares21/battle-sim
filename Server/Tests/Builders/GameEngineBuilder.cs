using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Tests.Builders;

public class GameEngineBuilder
{
    IGameHubState? _state;
    IGameDb? _db;
    ILogger<GameEngine>? _logger;

    public GameEngineBuilder WithState(IGameHubState state) {
        _state = state;
        return this;
    }

    public GameEngineBuilder WithDb(IGameDb db) {
        _db = db;
        return this;
    }

    public GameEngine Build() 
    {
        if (_state is null)
            _state = FakeState();
        if (_db is null)
            _db = FakeDb();
        if (_logger is null)
            _logger = FakeLogger();
        return new GameEngine(_state, _logger, _db);
    }

    IGameHubState FakeState() => new GameHubStateBuilder().Build();

    IGameDb FakeDb() 
    {
        var db = A.Fake<IGameDb>();
        A.CallTo(db)
            .WithReturnType<IEntity?>()
            .Returns(null);
        return db;
    }
    
    ILogger<GameEngine> FakeLogger() => 
        A.Fake<ILogger<GameEngine>>();

}