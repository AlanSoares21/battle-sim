using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Hubs.Game.RequestHandler;

[TestClass]
public class NewRequestsTests
{
    [TestMethod]
    public async Task Register_Request()
    {
        var requestCollection = A.Fake<IBattleRequestCollection>();
        CurrentCallerContext callerContext = new() {
            UserId = "callerId"
        };
        string targetId = "targetId";

        var handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestCollection)
            .Build();
        await handler.SendTo(targetId, callerContext);
        
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

    [TestMethod]
    public async Task Notify_The_Target_From_The_New_Request()
    {
        IGameHubClient targetConnection = A.Fake<IGameHubClient>();
        string targetId = "targetId";
        var hubClients = Utils.FakeHubContextWithClientForUser(targetId, targetConnection);
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);

        var handler = new RequestsHandlerBuilder().Build();
        await handler.SendTo(targetId, callerContext);

        SomeRequestSent(targetConnection);
    }

    void SomeRequestSent(IGameHubClient client)
    {
        A.CallTo(() => client.NewBattleRequest(A<BattleRequest>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Notify_Requester_When_Battle_Request_Sent() {
        string targetId = "targetId";
        IGameHubClient callerConnection = A.Fake<IGameHubClient>();
        var hubClients = Utils.FakeHubContextWithClientForCaller(callerConnection);
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);

        var handler = new RequestsHandlerBuilder().Build();
        await handler.SendTo(targetId, callerContext);

        CheckBattleRequestSent(callerConnection);
    }

    void CheckBattleRequestSent(IGameHubClient client)
    {
        A.CallTo(() => client.BattleRequestSent(A<BattleRequest>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Dont_Register_Duplicated_Request() {
        string targetId = "targetId";
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var requestList = new BattleRequest[] {
            new() {
                target = targetId,
                requester = callerContext.UserId
            }
        };
        var requestCollection = A.Fake<IBattleRequestCollection>();
        SetRequestListForAnyUser(requestCollection, requestList);

        var handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestCollection)
            .Build();
        await handler.SendTo(targetId, callerContext);
        
        ShoudNotTryAddAnyRequest(requestCollection);
    }

    [TestMethod]
    public async Task Dont_Register_The_Request_When_The_Target_Alredy_Requested_The_Current_Requester() {
        string targetId = "targetId";
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var requestList = new BattleRequest[] {
            new() {
                target = callerContext.UserId,
                requester = targetId
            }
        };
        var requestCollection = A.Fake<IBattleRequestCollection>();
        SetRequestListForAnyUser(requestCollection, requestList);
        
        var handler = new RequestsHandlerBuilder()
            .WithRequestCollection(requestCollection)
            .Build();
        await handler.SendTo(targetId, callerContext);
        
        ShoudNotTryAddAnyRequest(requestCollection);
    }

    void ShoudNotTryAddAnyRequest(IBattleRequestCollection collection)
    {
        A.CallTo(() => collection.TryAdd(A<BattleRequest>.Ignored))
            .MustNotHaveHappened();
    }

    void SetRequestListForAnyUser(
        IBattleRequestCollection collection, 
        IList<BattleRequest> requests)
    {
        A.CallTo(() => collection.RequestsWithUser(A<string>.Ignored))
            .Returns(requests);   
    }
}