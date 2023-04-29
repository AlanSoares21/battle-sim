using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class RequestsHandler
{
    IBattleRequestCollection _Requests;
    ILogger<RequestsHandler> _Logger;
    public RequestsHandler(
        IBattleRequestCollection requestCollection,
        ILogger<RequestsHandler> logger)
    {
        _Requests = requestCollection;
        _Logger = logger;
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
}