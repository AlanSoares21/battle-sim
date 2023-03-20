using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class GameEngine : IGameEngine
{
    ICalculator _gameCalculator;
    IGameHubState _state;
    ILogger<GameEngine> _logger;
    public GameEngine(
        IGameHubState state, 
        ILogger<GameEngine> logger) {
        _state = state;
        _logger = logger;
        _gameCalculator = new Calculator();
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

    public async Task SendBattleRequest(
        string targetId, 
        CurrentCallerContext caller)
    {
        if (BattleAlreadyRequested(caller.UserId, targetId)) {
            LogBattleAlreadyRequested(caller.UserId, targetId);
            return;
        }
        var battleRequest = new BattleRequest() {
            requester = caller.UserId,
            requestId = Guid.NewGuid(),
            target = targetId
        };
        if (!_state.BattleRequests.TryAdd(battleRequest)) {
            LogNotPossibleRequestBattle(battleRequest);
            return;
        }
        await caller
            .HubClients
            .User(targetId)
            .NewBattleRequest(battleRequest);
        await caller.Connection.BattleRequestSent(battleRequest);
    }

    bool BattleAlreadyRequested(string requesterId, string targetId) {
        return _state
            .BattleRequests
            .RequestsWithUser(requesterId)
            .Any(request => 
                request.UserIsOnRequest(targetId));
    }

    void LogBattleAlreadyRequested(string requester, string target) {
        _logger.LogInformation("A battle with those users has already been requested - requester: {requester} - target: {target}",
            requester,
            target);
    }

    void LogNotPossibleRequestBattle(BattleRequest request) {
        _logger.LogError("Not was possible request battle from {requester} to {target} - request id: {id}", 
            request.requester, 
            request.target, 
            request.requestId);
    }

    public async Task CancelBattleRequest(
        Guid requestId,
        CurrentCallerContext caller)
    {
        BattleRequest? request = GetBattleRequest(requestId);
        if (request is null)
            return;
        if (!request.UserIsOnRequest(caller.UserId)) {
            LogCallerIsNotInTheRequest(request, caller.UserId);
            return;
        }
        if (!_state.BattleRequests.TryRemove(request)) {
            LogCantRemoveRequest(request);
            return;
        }
        await caller
            .Connection
            .BattleRequestCancelled(
                caller.UserId,
                request);
        string secondUserId = request.target;
        if (request.requester != caller.UserId)
            secondUserId = request.requester;
        await caller
            .HubClients
            .User(secondUserId)
            .BattleRequestCancelled(
                caller.UserId,
                request);
    }

    BattleRequest? GetBattleRequest(Guid requestId) {
        try {
            return _state
                .BattleRequests
                .Get(requestId);
        }
        catch (Exception ex) {
            _logger.LogError("Error on search request {id} - Message: {message}",
                requestId,
                ex.Message);
            return null;
        }
    }

    void LogCallerIsNotInTheRequest(
        BattleRequest request, 
        string callerId) {
        _logger.LogError("Caller {caller} is not in the request {id} - requester: {requester} - target: {target}",
            callerId,
            request.requestId,
            request.requester,
            request.target);
    }

    public async Task AcceptBattleRequest(
        Guid requestId, 
        CurrentCallerContext caller,
        IGroupManager groupManager)
    {
        var request = _state.BattleRequests.Get(requestId);
        if (request.requester == caller.UserId) {
            LogRequesterCanNotAcceptTheRequest(request);
            return;
        }
        if (!_state.BattleRequests.TryRemove(request)) {
            LogCantRemoveRequest(request);
            return;
        }
        Guid battleId = Guid.NewGuid();
        bool succesOnCreateBattle = CreateBattle(
            battleId,
            request.target,
            request.requester
        );
        if (!succesOnCreateBattle) {
            LogCanNotCreateBattle(battleId);
            return;
        }
        IBattle battle = _state.Battles.Get(battleId);
        string battleGroupName = battle.Id.ToString();
        await FillGroupWithUsers(groupManager,
            battleGroupName,
            caller.ConnectionId,
            _state.Connections.GetConnectionId(request.requester)
        );
        await caller
            .HubClients
            .Group(battleGroupName)
            .NewBattle(
                battle.Id, 
                GetBoardData(battle.Board)
            );
    }

    void LogRequesterCanNotAcceptTheRequest(BattleRequest request) {
        _logger.LogError("User {user} try accept a request that is not for him - request: {id} - target: {target}",
            request.requester,
            request.requestId,
            request.target);
        return;
    }

    void LogCantRemoveRequest(BattleRequest request) {
        _logger.LogError("Cannot remove request {id} - requester: {requester} - target: {target}",
            request.requestId,
            request.requester,
            request.target);
    }

    bool CreateBattle(Guid battleId, params string[] userIds) {
        IBattle battle = CreateDuel(battleId);
        foreach(var id in userIds)
            battle.AddEntity(GetUserEntity(id));
        return _state.Battles.TryAdd(battle);
    }

    Duel CreateDuel(Guid battleId) => 
        new Duel(battleId, GameBoard.WithDefaultSize(), _gameCalculator);

    IEntity GetUserEntity(string userId) => new Player(userId);

    void LogCanNotCreateBattle(Guid requesterId) {
        _logger.LogError("Error on create battle - requesterId: {requesterId}", 
            requesterId);
    }

    BoardData GetBoardData(IBoard board) =>
        new BoardData() {
            entitiesPosition = GetEntitiesPosition(board),
            height = board.Height,
            width = board.Width
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

    async Task FillGroupWithUsers(
        IGroupManager groupManager,
        string groupName,
        params string[] connectionsId) 
    {
        foreach (var connectionId in connectionsId)
        await groupManager
            .AddToGroupAsync(connectionId, groupName);
    }

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
        var movementIntention = new MovementIntention() {
            cell = coordinate,
            entityIdentifier = caller.UserId
        };
        if (!_state.MovementIntentions.TryAdd(movementIntention))
            LogCanNotAddMoveIntention(coordinate, caller.UserId);
    }

    void LogCanNotAddMoveIntention(Coordinate coordinate, string callerId) {
        _logger.LogError("Can not add movement intention from {caller} to cell {cell}", 
            callerId, 
            coordinate);
    }
}