using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Hubs;

public class BattleHandler : IBattleHandler
{
    ICalculator _Calculator;
    IBattleCollection _Battles;
    IGameDb _Db;
    IGameDbConverter _converter;
    IHubContext<GameHub, IGameHubClient> _HubContext;
    IConnectionMapping _ConnMap;
    public BattleHandler(
        IBattleCollection battleCollection, 
        IGameDb gameDb,
        IHubContext<GameHub, IGameHubClient> hubContext,
        IConnectionMapping connectionMapping,
        IGameDbConverter converter)
    {
        _Calculator = new Calculator();
        _Battles = battleCollection;
        _Db = gameDb;
        _HubContext = hubContext;
        _ConnMap = connectionMapping;
        _converter = converter;
    }

    public async Task CreateDuel(string secondUser, CurrentCallerContext caller)
    {
        Guid battleId = Guid.NewGuid();
        string battleGroupName = battleId.ToString();
        
        IBattle duel = new Duel(
            battleId,
            GameBoard.WithDefaultSize(),
            _Calculator,
            CreateObserver(caller.HubClients.Group(battleGroupName))
        );
        AddUsersOnBattle(duel, caller.UserId, secondUser);
        _Battles.TryAdd(duel);
        
        var addingCallerInGroup = AddCallerInGroup(battleGroupName, caller);
        await AddUserInGroup(
            battleGroupName, 
            _ConnMap.GetConnectionId(secondUser));
        await addingCallerInGroup;
        await caller.HubClients.Group(battleGroupName)
            .NewBattle(GetBattleData(duel));
    }

    Task AddCallerInGroup(string groupName, CurrentCallerContext caller)
    {
        return AddUserInGroup(groupName, caller.ConnectionId);
    }

    IEventsObserver CreateObserver(IGameHubClient client) 
    {
        var observer = new EventsObserver();
        observer.SubscribeToSkillDamage(
            (skillName, sourceId, targetId, targetCurrentHealth) => {
                client.Skill(skillName, sourceId, targetId, targetCurrentHealth);
            }
        );
        return observer;
    }

    void AddUsersOnBattle(IBattle battle, params string[] usersIds)
    {
        foreach(var id in usersIds) 
            battle.AddEntity(GetEntityFor(id));
    }

    IEntity GetEntityFor(string id)
    {
        var r = _Db.SearchEntity(id);
        if (r is null)
            return _converter.DefaultEntity(id);
        return _converter.Entity(r);
    }

    Task AddUserInGroup(string groupName, string connectionId)
    {
        return _HubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    BattleData GetBattleData(IBattle battle) => 
        new BattleData() {
            id = battle.Id,
            board = GetBoardData(battle.Board),
            entities = battle.Entities
        };

    BoardData GetBoardData(IBoard board) =>
        new BoardData() {
            entitiesPosition = GetEntitiesPosition(board),
            size = new(board.Width, board.Height)
        };

    List<EntityPosition> GetEntitiesPosition(IBoard board) {
        List<EntityPosition> coordinates = new();
        foreach (string identifier in board.GetEntities())
        {
            var coordinate = board
                .GetEntityPosition(identifier);
            coordinates.Add(new EntityPosition() {
                entityIdentifier= identifier,
                x = coordinate.X,
                y = coordinate.Y
            });
        }
        return  coordinates;
    }

}