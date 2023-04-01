using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

public class BattleEventsHandlerBuilder
{
    IAttacksRequestedList? _attackList;
    IBattleCollection? _battles;
    IEventsQueue? _eventsQueue;

    public BattleEventsHandlerBuilder WithAttackList(IAttacksRequestedList value) 
    {
        _attackList = value;
        return this;
    }

    public BattleEventsHandlerBuilder WithEventsQueue(IEventsQueue value) 
    {
        _eventsQueue = value;
        return this;
    }

    public BattleEventsHandlerBuilder WithBattles(IBattleCollection value) 
    {
        _battles = value;
        return this;
    }

    public IBattleEventsHandler Build() 
    {
        if (_attackList is null)
            _attackList = A.Fake<IAttacksRequestedList>();
        if (_battles is null)
            _battles = A.Fake<IBattleCollection>();
        if (_eventsQueue is null)
            _eventsQueue = A.Fake<IEventsQueue>();
        
        return new BattleEventsHandler(
            _attackList,
            _battles,
            A.Fake<ILogger<BattleEventsHandler>>(),
            _eventsQueue);
    }    
}