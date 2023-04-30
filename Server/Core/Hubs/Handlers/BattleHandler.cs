using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class BattleHandler : IBattleHandler
{
    ICalculator _Calculator;
    IBattleCollection _Battles;
    public BattleHandler(IBattleCollection battleCollection)
    {
        _Calculator = new Calculator();
        _Battles = battleCollection;
    }
    public void CreateDuel(BattleRequest request, CurrentCallerContext caller)
    {
        Guid battleId = Guid.NewGuid();
        string battleGroupName = battleId.ToString();
        IBattle duel = new Duel(
            battleId,
            GameBoard.WithDefaultSize(),
            _Calculator,
            CreateObserver(caller.HubClients.Group(battleGroupName))
        );
        _Battles.TryAdd(duel);
    }

    IEventsObserver CreateObserver(IGameHubClient client) 
    {
        var observer = new EventsObserver();
        return observer;
    }
}