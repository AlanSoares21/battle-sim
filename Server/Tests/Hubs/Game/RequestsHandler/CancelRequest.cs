using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Tests.Hubs.Game;

[TestClass]
public class CancelRequest
{
    [TestMethod]
    public async Task Remove_Request_When_A_User_Cancel() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        string anotherUserId = "anotherUserId";
        BattleRequest request = new() {
            requester = caller.UserId,
            target = anotherUserId
        };
        var requestsCollection = Utils.FakeBattleRequestCollection(request);
        
        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestsCollection)
            .Build();
        await handler.Cancel(request.requestId, caller);
        
        RequestShouldBeRemoved(requestsCollection, request);
    }

    void RequestShouldBeRemoved(
        IBattleRequestCollection collection, 
        BattleRequest request)
    {
        A.CallTo(() => collection.TryRemove(request))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Notify_Users_Of_A_Cancelled_Request() {
        string anotherUserId = "anotherUserId";
        IGameHubClient anotherUserClient = A.Fake<IGameHubClient>();
        IGameHubClient callerClient = A.Fake<IGameHubClient>();
        IHubCallerClients<IGameHubClient> hubConnections = 
            Utils.FakeHubContextWithClientForCaller(callerClient);
        Utils.AddClientForUser(hubConnections, anotherUserId, anotherUserClient);
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            hubConnections);
        BattleRequest request = new() {
            requester = caller.UserId,
            target = anotherUserId
        };
        var requestList = Utils.FakeBattleRequestCollection(request); 
        Utils.EnableRemoveRequest(requestList, request);

        IRequestsHandler engine = new RequestsHandlerBuilder()
            .WithRequestCollection(requestList)
            .Build();
        await engine.Cancel(request.requestId, caller);

        BattleRequestCancelledShouldBeTriggered(callerClient, caller.UserId, request);
        BattleRequestCancelledShouldBeTriggered(anotherUserClient, caller.UserId, request);
    }

    void BattleRequestCancelledShouldBeTriggered(
        IGameHubClient client, 
        string cancellerId,
        BattleRequest request)
    {
        A.CallTo(() => client.BattleRequestCancelled(cancellerId, request))
            .MustHaveHappenedOnceExactly();
    }
    
    [TestMethod]
    public async Task Dont_Cancel_A_Request_When_The_Caller_Is_Not_Inner() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        string anotherUserId = "anotherUserId";
        BattleRequest request = new() {
            requester = "someUser",
            target = anotherUserId
        };
        var requestCollection = Utils.FakeBattleRequestCollection(request);

        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestCollection)
            .Build();
        await handler.Cancel(request.requestId, caller);

        RequestShouldNotBeRemoved(requestCollection, request);
    }

    void RequestShouldNotBeRemoved(
        IBattleRequestCollection collection, 
        BattleRequest request)
    {
        A.CallTo(() => collection.TryRemove(request))
            .MustNotHaveHappened();
    }

}