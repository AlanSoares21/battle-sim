using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Database;

namespace BattleSimulator.Server.Hubs;

public class GameEngine : IGameEngine
{
    ICalculator _gameCalculator;
    IGameHubState _state;
    ILogger<GameEngine> _logger;
    IGameDb _database;
    ISkillProvider _skillProvider;
    public GameEngine(
        IGameHubState state, 
        ILogger<GameEngine> logger,
        IGameDb database,
        ISkillProvider skillProvider) {
        _state = state;
        _logger = logger;
        _gameCalculator = new Calculator();
        _database = database;
        _skillProvider = skillProvider;
    }
    public async Task HandleUserDisconnected(
        CurrentCallerContext caller)
    {
        RemoveRequestsWithUser(caller.UserId);
        RemoveConnectionMapping(caller);
        RemoveBattle(caller);
        await NotifyUserDisconnect(caller);
    }

    void RemoveRequestsWithUser(string userId) {
        _logger.LogInformation("Removing battle requests with user {userId}", userId);
        var requests = _state.BattleRequests.RequestsWithUser(userId);
        _logger.LogInformation("Removing {count} battle requests with user {userId}", 
            requests.Count, 
            userId);
        foreach (var request in requests)
            if(!_state.BattleRequests.TryRemove(request))
                logErrorOnRemoveRequestWithUser(request, userId);
        _logger.LogInformation("Requests with user {userId} have been removed.", userId);
    }

    void logErrorOnRemoveRequestWithUser(BattleRequest request, string userId) {
        _logger.LogError("Battle request {id} not removed - Requester: {requester} - Target: {target} - UserId used to remove: {userId}",
            request.requestId,
            request.requester,
            request.target,
            userId);
    }

    void RemoveConnectionMapping(CurrentCallerContext caller) {
        if (!_state.Connections.TryRemove(caller.UserId, caller.ConnectionId)) {
            LogErrorRemovingConnectionMapping(caller.UserId);
            throw new Exception($"Not possible remove key for connection - UserId: {caller.UserId}");
        }
    }

    void LogErrorRemovingConnectionMapping(string userId) {
        _logger.LogError("Not possible remove connection record on discconect - user: {user}", userId);
    }

    void RemoveBattle(CurrentCallerContext caller) {
        var battle = GetBattleByEntity(caller.UserId);
        if (battle is null)
            return;
    
        if (!_state.Battles.TryRemove(battle)) {
            LogFailToRemoveBattle(battle);
        }
    
    }

    void LogNotFoundBattleWithEntity(
        string entityId, 
        KeyNotFoundException ex) {
        _logger.LogInformation("Battle with entity {id} not found. Message: {message}",
            entityId,
            ex.Message);
    }

    private async Task NotifyUserDisconnect(CurrentCallerContext caller) 
    {
        _logger.LogInformation("start notifying user disconnected - user: {user}", caller.UserId);
        await caller
            .HubClients
            .Others
            .UserDisconnect(new() { name = caller.UserId });
        _logger.LogInformation("finish notify user disconnected - user: {user}", caller.UserId); 
    }

    public async Task HandleUserConnected(CurrentCallerContext caller)
    {
        AddConnectionMapping(caller);
        await NotifyConnect(caller);
    }

    void AddConnectionMapping(CurrentCallerContext caller) {
        if (!_state.Connections.TryAdd(caller.UserId, caller.ConnectionId)) {
            _logger.LogError("Not possible add connection - user: {user} - conn id: {connId}", 
                caller.UserId,
                caller.ConnectionId);
            throw new Exception($"Not possible register connection - User: {caller.UserId}");
        }
    }
    async Task NotifyConnect(CurrentCallerContext caller) {
        _logger.LogInformation("start notifying user connected - user: {user}", caller.UserId);
        await caller
            .HubClients
            .Others
            .UserConnect(new() { name = caller.UserId });
        _logger.LogInformation("finish notify user connected  - user: {user}", caller.UserId);
    }

    public async Task ListUsers(CurrentCallerContext caller)
    {
        var connectedUsers = GetUsersExcept(caller.UserId);
        await caller
            .Connection
            .ListConnectedUsers(connectedUsers);
    }

    IEnumerable<UserConnected> GetUsersExcept(string userId) => 
        _state.Connections
            .UsersIds()
            .Where(username => username != userId)
            .Select(username => new UserConnected() { name = username });

    public async Task CancelBattle(
        Guid battleId, 
        CurrentCallerContext caller) 
    {
        IBattle? battle = GetBattle(battleId);
        if (battle is null)
            return;
        if (!battle.EntityIsIntheBattle(caller.UserId)) {
            LogCallerIsNotInTheBattle(caller, battle);
            return;
        }
        bool result = _state
            .Battles
            .TryRemove(battle);
        if (!result)  {
            LogFailToRemoveBattle(battle);
            return;
        }
        await caller
            .HubClients
            .Group(battle.Id.ToString())
            .BattleCancelled(caller.UserId, battle.Id);
    }

    IBattle? GetBattle(Guid battleId) {
        try {
            return _state.Battles.Get(battleId);
        }
        catch (KeyNotFoundException ex) {
            LogBattleNotFound(battleId, ex);
            return null;
        }
    }

    void LogBattleNotFound(Guid battleId, KeyNotFoundException ex) {
        _logger.LogError("Battle {id} not found - Message: {message}",
            battleId,
            ex.Message);
    }

    void LogCallerIsNotInTheBattle(
        CurrentCallerContext caller,
        IBattle battle) 
    {
        _logger.LogInformation("User {user} is not in the battle {id} but tried cancel it.",
            caller.UserId,
            battle.Id);
    }

    void LogFailToRemoveBattle(IBattle battle) 
    {
        _logger.LogError("Fail to remove battle {id}",
            battle.Id);
    }

    public void Move(Coordinate coordinate, CurrentCallerContext caller) 
    {
        var battle = GetBattleByEntity(caller.UserId);
        if (battle is null)
            return;
        battle.RegisterMove(caller.UserId, coordinate);
    }

    IBattle? GetBattleByEntity(string entityId) {
        try {
            Guid battleId = _state
                .Battles
                .GetBattleIdByEntity(entityId);
            return _state.Battles.Get(battleId);
        } catch (KeyNotFoundException ex) {
            LogNotFoundBattleWithEntity(entityId, ex);
            return null;
        }
    }
}