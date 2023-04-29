using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class RequestsHandler : IRequestsHandler
{
    IBattleRequestCollection _Requests;
    ILogger<RequestsHandler> _Logger;
    IBattleHandler _BattleHandler;
    public RequestsHandler(
        IBattleRequestCollection requestCollection,
        IBattleHandler battleHandler,
        ILogger<RequestsHandler> logger)
    {
        _Requests = requestCollection;
        _Logger = logger;
        _BattleHandler = battleHandler;
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
            _Logger.LogError("User {user} try accept a request, but he is not the target - request: {id} - target: {target}",
                request.requester,
                request.requestId,
                request.target);
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
}