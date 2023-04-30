using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Builders;

public class BattleHandlerBuilder
{
    IBattleCollection? _BattleCollection;

    public BattleHandlerBuilder WithBattleCollection(IBattleCollection collection)
    {
        _BattleCollection = collection;
        return this;
    }
    public IBattleHandler Build()
    {
        if (_BattleCollection is null)
            _BattleCollection = A.Fake<IBattleCollection>();
        return new BattleHandler(_BattleCollection);
    }
}