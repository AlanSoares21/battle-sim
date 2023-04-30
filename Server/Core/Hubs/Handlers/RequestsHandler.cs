using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class RequestsHandler : IRequestsHandler
{
    IBattleRequestCollection _Requests;
    IBattleCollection _Battles;
    ILogger<RequestsHandler> _Logger;
    IBattleHandler _BattleHandler;
    public RequestsHandler(
        IBattleRequestCollection requestCollection,
        IBattleHandler battleHandler,
        IBattleCollection battleCollection,
        ILogger<RequestsHandler> logger)
    {
        _Requests = requestCollection;
        _Logger = logger;
        _BattleHandler = battleHandler;
        _Battles = battleCollection;
    }

    public async Task SendTo(string target, CurrentCallerContext caller)
    {
        if (AlreadyRequested(caller.UserId, target)) 
        {
            _Logger.LogInformation("A battle with those users has already been requested - requester: {requester} - target: {target}",
                caller.UserId,
                target);
            return;
        }
        var request = new BattleRequest() {
            requester = caller.UserId,
            requestId = Guid.NewGuid(),
            target = target
        };
        if (!_Requests.TryAdd(request)) 
        {
            _Logger.LogError("Not possible request battle from {requester} to {target} - request id: {id}", 
                request.requester, 
                request.target, 
                request.requestId);
            return;
        }
        _Logger.LogInformation("New request {id} from {requester} to {target}", 
            request.requestId,
            request.requester,
            request.target);
        var sendingToTarget = caller
            .HubClients
            .User(target)
            .NewBattleRequest(request);
        await caller.Connection.BattleRequestSent(request);
        await sendingToTarget;
    }

    bool AlreadyRequested(string requesterId, string targetId) {
        return _Requests
            .RequestsWithUser(requesterId)
            .Any(request => 
                request.UserIsOnRequest(targetId));
    }

    public void Accept(Guid requestId, CurrentCallerContext caller)
    {
        var request = _Requests.Get(requestId);
        if (request.target != caller.UserId) {
            _Logger.LogInformation("User {user} try accept a request, but he is not the target - request: {id} - target: {target}",
                request.requester,
                request.requestId,
                request.target);
            return;
        }
        if (UserIsInBattle(request.requester) || UserIsInBattle(request.target))
        {
            _Logger.LogInformation("One of users {requester}, {target} was in a battle when the target tried accept the request {id}",
                request.requester,
                request.target,
                request.requestId);
            return;
        }
        if (!_Requests.TryRemove(request)) 
        {
            _Logger.LogError("Cannot remove request {id} - requester: {requester} - target: {target}",
                request.requestId,
                request.requester,
                request.target);
            return;
        }
        _Logger.LogInformation("Request accepted request {id} - requester: {requester} - target: {target}",
            request.requestId,
            request.requester,
            request.target);
        _BattleHandler.CreateBattle();
    }

    bool UserIsInBattle(string userId)
    {
        try 
        {
            _Battles.GetBattleIdByEntity(userId);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public async Task Cancel(Guid requestId, CurrentCallerContext caller)
    {
        BattleRequest? request = GetBattleRequest(requestId);
        if (request is null)
            return;
        if (!request.UserIsOnRequest(caller.UserId)) {
            _Logger.LogError("Caller {caller} is not in the request {id} - requester: {requester} - target: {target}",
                caller.UserId,
                request.requestId,
                request.requester,
                request.target);
            return;
        }
        if (!_Requests.TryRemove(request)) {
            _Logger.LogError("Can not remove request {id} - requester: {requester} - target: {target}",
                request.requestId,
                request.requester,
                request.target);
            return;
        }

        var callerCancelNotification = caller
            .Connection
            .BattleRequestCancelled(caller.UserId, request);
        await caller
            .HubClients
            .User(GetSecondUserId(caller.UserId, request))
            .BattleRequestCancelled(caller.UserId, request);
        await callerCancelNotification;
    }

    BattleRequest? GetBattleRequest(Guid requestId) {
        try {
            return _Requests
                .Get(requestId);
        }
        catch (Exception ex) {
            _Logger.LogInformation("Error on search request {id} - Message: {message}",
                requestId,
                ex.Message);
            return null;
        }
    }

    string GetSecondUserId(string caller, BattleRequest request)
    {
        if (request.requester != caller)
            return request.requester;
        return request.target;
    }
}