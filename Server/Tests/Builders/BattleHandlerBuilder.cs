using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Builders;

public class BattleHandlerBuilder
{
    IGameDb? _Db;
    IBattleCollection? _BattleCollection;

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

    public IBattleHandler Build()
    {
        if (_BattleCollection is null)
            _BattleCollection = A.Fake<IBattleCollection>();
        if (_Db is null)
            _Db = FakeDb();
        return new BattleHandler(_BattleCollection, _Db);
    }

    IGameDb FakeDb()
    {
        var db = A.Fake<IGameDb>();
        A.CallTo(db)
            .WithReturnType<IEntity>()
            .ReturnsNextFromSequence(
                Utils.FakeEntity("fakeEntity1"),
                Utils.FakeEntity("fakeEntity2")
            );
        return db;
    }
}