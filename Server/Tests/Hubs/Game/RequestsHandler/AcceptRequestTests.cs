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
        var requestCollection = A.Fake<IBattleRequestCollection>();
        Utils.AddRequestOnCollection(requestCollection, request);

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

    // TODO:: call BattleOpHandler creation method after the request been accepted
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
        var requestCollection = A.Fake<IBattleRequestCollection>();
        Utils.AddRequestOnCollection(requestCollection, request);
        EnableRemoveRequest(requestCollection, request);
        var battleHandler = A.Fake<IBattleHandler>();

        IRequestsHandler handler = new RequestsHandlerBuilder()
            .WithBattleHandler(battleHandler)
            .WithRequestCollection(requestCollection)
            .Build();
        handler.Accept(request.requestId, caller);

        BattleWasCreated(battleHandler);
    }

    void EnableRemoveRequest(IBattleRequestCollection collection, BattleRequest request)
    {
        A.CallTo(() => collection.TryRemove(request))
            .Returns(true);
    }

    void BattleWasCreated(IBattleHandler handler)
    {
        A.CallTo(() => handler.CreateBattle())
            .MustHaveHappenedOnceExactly();
    }

    // TODO:: requester can not accept the request
    // TODO:: request can not be accepted if one of users are in a battle

    // TODO:: notify requester of the request accepted
}