using BattleSimulator.Server.Tests.Builders;
using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Server.Tests.Hubs.Game.Engine;

[TestClass]
public class EngineBattleRequestsTests {
    [TestMethod]
    public async Task Create_Group_On_Hub_With_The_Users_Of_The_Battle() {
        string requesterId = "requesterId";
        string requesterConnectionId = "requesterConnectionId";
        var groupManager = Utils.FakeGroupManager();
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = requesterId,
            target = caller.UserId
        };
        var connectionMapping = A.Fake<IConnectionMapping>();
        A.CallTo(() => connectionMapping.GetConnectionId(requesterId))
            .Returns(requesterConnectionId);
        IGameHubState state = new GameHubStateBuilder()
            .WithConnectionMapping(connectionMapping)
            .Build();
        A.CallTo(() => state.BattleRequests.Get(request.requestId))
            .Returns(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            groupManager);
        A.CallTo(() => 
            groupManager.AddToGroupAsync(
                caller.ConnectionId, 
                A<string>.Ignored,
                A<CancellationToken>.Ignored)
            )
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => 
            groupManager.AddToGroupAsync(
                requesterConnectionId, 
                A<string>.Ignored,
                A<CancellationToken>.Ignored)
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]    
    public async Task When_Accept_A_Request_Notify_The_Requester() {
        string requesterId = "requesterId";
        var requesterClient = A.Fake<IGameHubClient>();
        var hubClients = Utils.FakeHubCallerContext();
        A.CallTo(() => hubClients.Group(""))
            .WithAnyArguments()
            .Returns(requesterClient);
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            hubClients);
        BattleRequest request = new() {
            requester = requesterId,
            target = caller.UserId
        };
        IGameHubState state = Utils.FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => requesterClient.NewBattle(new()))
            .WithAnyArguments()
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]    
    public async Task When_Accept_A_Request_Send_Entities_Data_For_All_Users() {
        var requesterClient = A.Fake<IGameHubClient>();
        var hubClients = Utils.FakeHubCallerContext();
        A.CallTo(() => hubClients.Group(""))
            .WithAnyArguments()
            .Returns(requesterClient);
        IEntity callerEntity = Utils.FakeEntity("callerId");
        CurrentCallerContext caller = new(
            callerEntity.Id, 
            "callerConnectionId",
            hubClients);
        IEntity requesterEntity = Utils.FakeEntity("requesterId");
        BattleRequest request = new() {
            requester = requesterEntity.Id,
            target = caller.UserId
        };
        IGameHubState state = new GameHubStateBuilder()
            .WithBattleCollection(new BattleCollection())
            .Build();
        Utils.AddRequestOnState(state, request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .WithDb(Utils.FakeDbWithEntities(callerEntity, requesterEntity))
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => 
            requesterClient.NewBattle(EntitiesAreInBattleData(callerEntity, requesterEntity)))
            .MustHaveHappenedOnceExactly();
    }

    BattleData EntitiesAreInBattleData(params IEntity[] entities) {
        return A<BattleData>.That.Matches(b => 
            b.entities.Count > 0
            && b.entities.All(e => entities.Contains(e)));
    }
}