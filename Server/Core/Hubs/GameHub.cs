using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

[Authorize]
public class GameHub : Hub<IGameHubClient>, IGameHubServer
{
    IGameHubState _hubState;
    ILogger<GameHub> _logger;
    IGameEngine _engine;
    IBattleEventsHandler _eventsHandler;
    IRequestsHandler _RequestsHandler;
    public GameHub(
        ILogger<GameHub> logger, 
        IGameHubState hubState,
        IGameEngine engine,
        IBattleEventsHandler eventsHandler,
        IRequestsHandler requestsHandler) {
        _logger = logger;
        _hubState = hubState;
        _engine = engine;
        _eventsHandler = eventsHandler;
        _RequestsHandler = requestsHandler;
    }
    public async Task ListUsers() 
    {
        _logger.LogInformation("start - list users for {user}", Context.UserIdentifier);
        await _engine.ListUsers(GetCurrentCallerContext());
        _logger.LogInformation("end - list users for {user}", Context.UserIdentifier);
    }

    public async Task SendBattleRequest(string targetId) {
        _logger.LogInformation("send new battle request from {requester} to {target}", 
            Context.UserIdentifier, 
            targetId);
        await _RequestsHandler.SendTo(
            targetId,
            GetCurrentCallerContext()
        );
        _logger.LogInformation("new battle request sent from {requester} to {target}", 
            Context.UserIdentifier, 
            targetId);
    }

    public void AcceptBattle(Guid requestId) {
        _logger.LogInformation("User {name} accept battle request {id} ", 
            Context.UserIdentifier, 
            requestId);
        _RequestsHandler.Accept(requestId, GetCurrentCallerContext());
        _logger.LogInformation("Request {id} accept", requestId);
    }

    public Task Move(int x, int y)
    {
        _engine.Move(new(x, y), GetCurrentCallerContext());
        return Task.CompletedTask;
    }

    public async Task CancelBattleRequest(Guid requesterId)
    {
        _logger.LogInformation("Canceling battle request {id} - user: {user}",
            requesterId,
            Context.UserIdentifier);
        await _RequestsHandler.Cancel(
            requesterId, 
            GetCurrentCallerContext());
        _logger.LogInformation("Battle request {id} cancelled by user: {user}",
            requesterId,
            Context.UserIdentifier);
    }

    public async Task CancelBattle(Guid battleId)
    {
        _logger.LogInformation("User {caller} cancelling battle {id}",
            Context.UserIdentifier,
            battleId);
        await _engine.CancelBattle(battleId, GetCurrentCallerContext());
        _logger.LogInformation("Battle {id} canceled by {caller}",
            battleId,
            Context.UserIdentifier);
    }

    public void Attack(string targetId)
    {
        _eventsHandler.Attack(targetId, GetCurrentUserId());
    }

    public void Skill(string skillName, string targetId)
    {
        _eventsHandler.Skill(skillName, targetId, GetCurrentCallerContext());
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("New user connected - user: {user}", Context.UserIdentifier);
        await _engine.HandleUserConnected(GetCurrentCallerContext());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User disconnected - user: {user}", Context.UserIdentifier);
        await _engine.HandleUserDisconnected(GetCurrentCallerContext());
        await base.OnDisconnectedAsync(exception);
    }

    CurrentCallerContext GetCurrentCallerContext() {
        string userId = GetCurrentUserId();
        return new CurrentCallerContext(
            userId,
            Context.ConnectionId,
            Clients
        );
    }

    string GetCurrentUserId() {
        string? userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId)) {
            _logger.LogError("UserId is null or empty - userId: {userId}", userId);
            throw new Exception($"User identifier null or empty.");
        }
        return userId;
    }

}