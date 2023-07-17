using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Database.Models;
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
    IServerConfig _config;
    public BattleHandler(
        IBattleCollection battleCollection, 
        IGameDb gameDb,
        IHubContext<GameHub, IGameHubClient> hubContext,
        IConnectionMapping connectionMapping,
        IGameDbConverter converter,
        IServerConfig config)
    {
        _Calculator = new Calculator();
        _Battles = battleCollection;
        _Db = gameDb;
        _HubContext = hubContext;
        _ConnMap = connectionMapping;
        _converter = converter;
        _config = config;
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
        
        Entity callerEntity = GetEntityOrDefault(caller.UserId), 
        secondUserEntity = GetEntityOrDefault(secondUser);
        AddUsersOnBattle(duel, callerEntity, secondUserEntity);
        _Battles.TryAdd(duel);
        
        var addingCallerInGroup = AddCallerInGroup(battleGroupName, caller);
        await AddUserInGroup(
            battleGroupName, 
            _ConnMap.GetConnectionId(secondUser));
        await addingCallerInGroup;
        List<Entity> entities = new() { callerEntity, secondUserEntity };
        await caller.HubClients.Group(battleGroupName)
            .NewBattle(GetBattleData(duel, entities));
    }

    Entity GetEntityOrDefault(string id)
    {
        var entityOnDb = _Db.SearchEntity(id);
        if (entityOnDb is null)
            return _config.DefaultEntity(id);
        return entityOnDb;
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

    void AddUsersOnBattle(IBattle battle, params Entity[] users)
    {
        foreach(var user in users) 
        {
            var entity = _converter.Entity(user);
            foreach (var equip in user.Equips)
                entity.AddEquip(_converter.Equip(equip));
            battle.AddEntity(entity);
        }
    }

    Task AddUserInGroup(string groupName, string connectionId)
    {
        return _HubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    BattleData GetBattleData(IBattle battle, List<Entity> entities) => 
        new BattleData() {
            id = battle.Id,
            board = GetBoardData(battle.Board),
            entities = entities
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