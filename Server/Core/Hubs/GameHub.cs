using System.Collections.Concurrent;
using BattleSimulator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

[Authorize]
public class GameHub : Hub<IGameHubClient>, IGameHubServer
{
    IGameHubState _hubState;
    private ILogger<GameHub> _logger;
    private IGameEngine _engine;
    public GameHub(
        ILogger<GameHub> logger, 
        IGameHubState hubState,
        IGameEngine engine) {
        _logger = logger;
        _hubState = hubState;
        _engine = engine;
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
        await _engine.SendBattleRequest(
            targetId,
            GetCurrentCallerContext()
        );
        _logger.LogInformation("new battle request sent from {requester} to {target}", 
            Context.UserIdentifier, 
            targetId);
    }

    public async Task AcceptBattle(Guid requestId) {
        _logger.LogInformation("User {name} accept battle request {id} ", 
            Context.UserIdentifier, 
            requestId);
        await _engine.AcceptBattleRequest(
            requestId, 
            GetCurrentCallerContext(),
            Groups);
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
        await _engine.CancelBattleRequest(
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