using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Builders;

public class GameHubStateBuilder
{
    IConnectionMapping? connectionMapping;
    ILogger<GameHubState>? logger;
    IBattleRequestCollection? requestCollection;
    IBattleCollection? battleCollection;

    public GameHubStateBuilder WithConnectionMapping(IConnectionMapping connectionMapping) 
    {
        this.connectionMapping = connectionMapping;
        return this;
    }

    public GameHubStateBuilder WithLogger(ILogger<GameHubState> logger) 
    {
        this.logger = logger;
        return this;
    }
    public GameHubStateBuilder WithBattleRequestCollection(IBattleRequestCollection collection) 
    {
        this.requestCollection = collection;
        return this;
    }

    public GameHubStateBuilder WithBattleCollection(IBattleCollection collection) 
    {
        this.battleCollection = collection;
        return this;
    }
    
    public GameHubState Build() {
        if (connectionMapping is null)
            connectionMapping = FakeConnectionMapping();
        if (logger is null)
            logger = FakeLogger();
        if (requestCollection is null)
            requestCollection = FakeRequestCollection();
        if (battleCollection is null)
            battleCollection = FakeBattleCollection();
        return new GameHubState(
            connectionMapping, 
            logger,
            requestCollection,
            battleCollection
        );
    }

    IConnectionMapping FakeConnectionMapping() {
        var connectionMapping = A.Fake<IConnectionMapping>();
        A.CallTo(connectionMapping)
            .WithReturnType<bool>()
            .Returns(true);
        return connectionMapping;
    }
    ILogger<GameHubState> FakeLogger() =>
        A.Fake<ILogger<GameHubState>>();

    IBattleRequestCollection FakeRequestCollection() {
        var collection = A.Fake<IBattleRequestCollection>();
        A.CallTo(collection)
            .WithReturnType<bool>()
            .Returns(true);
        return collection;
    }

    IBattleCollection FakeBattleCollection() {
        var collection = A.Fake<IBattleCollection>();
        A.CallTo(collection)
            .WithReturnType<bool>()
            .Returns(true);
        return collection;
    }
}