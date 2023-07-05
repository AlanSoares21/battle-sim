using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Tests.Builders;

public class BattleHandlerBuilder
{
    IGameDb? _Db;
    IGameDbConverter? _converter;
    IBattleCollection? _BattleCollection;
    IHubContext<GameHub, IGameHubClient>? _HubContext;
    IConnectionMapping? _ConnectionMapping;

    public BattleHandlerBuilder WithBattleCollection(IBattleCollection collection)
    {
        _BattleCollection = collection;
        return this;
    }

    public BattleHandlerBuilder WithDb(IGameDb db)
    {
        _Db = db;
        return this;
    }

    public BattleHandlerBuilder WithConverter(IGameDbConverter value)
    {
        _converter = value;
        return this;
    }

    public BattleHandlerBuilder WithHubContext(
        IHubContext<GameHub, IGameHubClient> context)
    {
        _HubContext = context;
        return this;
    }

    public BattleHandlerBuilder WithConnectionMapping(IConnectionMapping mapping)
    {
        _ConnectionMapping = mapping;
        return this;
    }
    public IBattleHandler Build()
    {
        if (_BattleCollection is null)
            _BattleCollection = A.Fake<IBattleCollection>();
        if (_Db is null)
            _Db = FakeDb();
        if (_converter is null)
            _converter = FakeConverter();
        if (_HubContext is null)
            _HubContext = A.Fake<IHubContext<GameHub, IGameHubClient>>();
        if (_ConnectionMapping is null)
            _ConnectionMapping = A.Fake<IConnectionMapping>();

        return new BattleHandler(
            _BattleCollection, 
            _Db, 
            _HubContext, 
            _ConnectionMapping,
            _converter);
    }

    IGameDb FakeDb()
    {
        var db = A.Fake<IGameDb>();
        A.CallTo(db)
            .WithReturnType<Entity>()
            .ReturnsNextFromSequence(
                new Entity() { Id = "fakeEntity1" },
                new Entity() { Id = "fakeEntity2" }
            );
        return db;
    }

    IGameDbConverter FakeConverter()
    {
        var converter = A.Fake<IGameDbConverter>();
        A.CallTo(converter)
            .WithReturnType<IEntity>()
            .ReturnsNextFromSequence(
                Utils.FakeEntity("fakeEntity1"),
                Utils.FakeEntity("fakeEntity2")
            );
        return converter;
    }
}