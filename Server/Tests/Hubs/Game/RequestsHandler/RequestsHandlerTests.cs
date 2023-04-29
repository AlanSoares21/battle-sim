using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Hubs.Game.RequestHandler;

[TestClass]
public class RequestsHandlerTests
{
    [TestMethod]
    public void Register_Request()
    {
        var requestCollection = A.Fake<IBattleRequestCollection>();
        CurrentCallerContext callerContext = new() {
            UserId = "callerId"
        };
        string targetId = "targetId";
        var handler = CreateHandler(requestCollection);
        handler.RequestDuel(targetId, callerContext);
        RequestWithUsersHasBeenRegistered(
            callerContext.UserId, 
            targetId, 
            requestCollection);
    }

    void RequestWithUsersHasBeenRegistered(
        string caller, 
        string target, 
        IBattleRequestCollection requestCollection)
    {
        A.CallTo(() => 
            requestCollection.TryAdd(A<BattleRequest>.That
                .Matches(req => req.requester == caller && req.target == target)
            )
        ).MustHaveHappenedOnceExactly();
    }

    RequestsHandler CreateHandler(IBattleRequestCollection requestCollection) 
    {
        return new RequestsHandler(
            requestCollection,
            A.Fake<ILogger<RequestsHandler>>());
    }

    // TODO:: notify the target
    // TODO:: dont send request if a request between users already exists
    
    // TODO:: accept request
    // TODO:: requester can not accept the request
    // TODO:: notify requester of the request accepted
    // TODO:: request can not be accepted if one of users are in a battle
    // TODO:: call BattleOpHandler creation method after the request been accepted
    // TODO:: remove request register after accept
}