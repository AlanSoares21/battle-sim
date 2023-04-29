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

        RequestsHandler handler = new RequestsHandlerBuilder().Build();
        await handler.SendTo(targetId, callerContext);

        SomeRequestSent(targetConnection);
    }

    void SomeRequestSent(IGameHubClient client)
    {
        A.CallTo(() => client.NewBattleRequest(A<BattleRequest>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    // TODO:: notify the requester
    [TestMethod]
    public async Task Notify_Requester_When_Battle_Request_Sent() {
        string targetId = "targetId";
        IGameHubClient callerConnection = A.Fake<IGameHubClient>();
        var hubClients = Utils.FakeHubContextWithClientForCaller(callerConnection);
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);

        RequestsHandler handler = new RequestsHandlerBuilder().Build();
        await handler.SendTo(targetId, callerContext);

        CheckBattleRequestSent(callerConnection);
    }

    void CheckBattleRequestSent(IGameHubClient client)
    {
        A.CallTo(() => client.BattleRequestSent(A<BattleRequest>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
    
    // TODO:: dont send request if a request between users already exists
    
    // TODO:: accept request
    // TODO:: requester can not accept the request
    // TODO:: notify requester of the request accepted
    // TODO:: request can not be accepted if one of users are in a battle
    // TODO:: call BattleOpHandler creation method after the request been accepted
    // TODO:: remove request register after accept
}