using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;

namespace BattleSimulator.Server.Tests.Hubs.Game.RequestHandler;

[TestClass]
public class AcceptRequestTests
{
    [TestMethod]
    public void When_Accept_A_Request_Remove_The_Request() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        var requestCollection = Utils.FakeBattleRequestCollection(request);

        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestCollection)
            .Build();
        handler.Accept(request.requestId, caller);

        RequestHasBeenRemoved(requestCollection, request);
    }

    void RequestHasBeenRemoved(IBattleRequestCollection collection, BattleRequest request)
    {
        A.CallTo(() => collection.TryRemove(request))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void When_Accept_A_Request_Create_A_Battle() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        var requestCollection = Utils.FakeBattleRequestCollection(request);
        Utils.EnableRemoveRequest(requestCollection, request);
        
        var battleHandler = A.Fake<IBattleHandler>();

        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithBattleHandler(battleHandler)
            .WithRequestCollection(requestCollection)
            .Build();
        handler.Accept(request.requestId, caller);

        BattleWasCreated(battleHandler, request, caller);
    }

    void BattleWasCreated(
        IBattleHandler handler, 
        BattleRequest request,
        CurrentCallerContext caller)
    {
        A.CallTo(() => handler.CreateDuel(request.requester, caller))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void When_The_Caller_Is_Not_The_Target_The_Request_Should_Not_Be_Accepted() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() { target = "iAmNotTheCaller" };
        var requestCollection = Utils.FakeBattleRequestCollection(request);
        Utils.EnableRemoveRequest(requestCollection, request);
        
        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestCollection)
            .Build();
        handler.Accept(request.requestId, caller);

        NoRequestsShouldBeRemoved(requestCollection);
    }

    [TestMethod]
    [DataRow("iAmInBattle", "iAmNotInBattle")]
    [DataRow("iAmNotInBattle", "iAmInBattle")]
    public void Can_Not_Accept_The_Request_When_Requester_Is_In_Battle(
        string requester,
        string target)
    {
        CurrentCallerContext caller = new(
            target, 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = requester,
            target = caller.UserId
        };
        var requestCollection = Utils.FakeBattleRequestCollection(request);
        Utils.EnableRemoveRequest(requestCollection, request);
        IBattleCollection battleCollection = A.Fake<IBattleCollection>();
        
        A.CallTo(() => battleCollection.GetBattleIdByEntity(UserIsOnBattle()))
            .Returns(Guid.NewGuid());

        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithBattleCollection(battleCollection)
            .WithRequestCollection(requestCollection)
            .Build();
        handler.Accept(request.requestId, caller);

        NoRequestsShouldBeRemoved(requestCollection);
    }

    string UserIsOnBattle()
    {
        return A<string>.That.Matches(v => v == "iAmInBattle");
    }

    void NoRequestsShouldBeRemoved(IBattleRequestCollection collection)
    {
        A.CallTo(() => collection.TryRemove(A<BattleRequest>.Ignored))
            .MustNotHaveHappened();
    }

    // TODO:: notify requester of the request accepted
}