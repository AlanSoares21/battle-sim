using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class BattleHandler : IBattleHandler
{
    ICalculator _Calculator;
    IBattleCollection _Battles;
    IGameDb _Db;
    public BattleHandler(IBattleCollection battleCollection, IGameDb gameDb)
    {
        _Calculator = new Calculator();
        _Battles = battleCollection;
        _Db = gameDb;
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
        AddUsersOnBattle(duel, request.requester, request.target);
        _Battles.TryAdd(duel);
    }

    IEventsObserver CreateObserver(IGameHubClient client) 
    {
        var observer = new EventsObserver();
        return observer;
    }

    void AddUsersOnBattle(IBattle battle, params string[] usersIds)
    {
        foreach(var id in usersIds)
            battle.AddEntity(_Db.GetEntityFor(id));
    }
}