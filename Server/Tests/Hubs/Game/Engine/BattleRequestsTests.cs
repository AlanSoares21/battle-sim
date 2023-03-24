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
    public async Task Send_Battle_Request_To_Target_User() {
        IGameHubClient targetConnection = A.Fake<IGameHubClient>();
        string targetId = "targetId";
        var hubClients = Utils.FakeHubCallerContext();
        A.CallTo(() => hubClients.User(targetId))
            .Returns(targetConnection);
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);
        IGameEngine engine = new GameEngineBuilder().Build();
        await engine.SendBattleRequest(targetId, callerContext);
        A.CallTo(() => targetConnection.NewBattleRequest(new()))
            .WithAnyArguments()
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Notify_Requester_When_Battle_Request_Sent() {
        IGameHubClient callerConnection = A.Fake<IGameHubClient>();
        var hubClients = Utils.FakeHubCallerContext();
        A.CallTo(() => hubClients.Caller)
            .Returns(callerConnection);
        string targetId = "targetId";
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);
        IGameEngine engine = new GameEngineBuilder().Build();
        await engine.SendBattleRequest(targetId, callerContext);
        A.CallTo(() => callerConnection.BattleRequestSent(new()))
            .WithAnyArguments()
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Register_Battle_Request() {
        string targetId = "targetId";
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        IGameHubState state = new GameHubStateBuilder().Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.SendBattleRequest(targetId, callerContext);
        A.CallTo(() => state.BattleRequests.TryAdd(
                BattleRequestWithUsers(callerContext.UserId, targetId)
            ))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Dont_Register_Duplicated_Request() {
        string targetId = "targetId";
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        List<BattleRequest> requests = new() {
            new BattleRequest() {
                target = targetId,
                requester = callerContext.UserId
            }
        };
        IGameHubState state = new GameHubStateBuilder().Build();
        A.CallTo(() => state
            .BattleRequests
            .RequestsWithUser(""))
                .WithAnyArguments()
                .Returns(requests);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.SendBattleRequest(targetId, callerContext);
        A.CallTo(() => state.BattleRequests.TryAdd(new()))
            .WithAnyArguments()
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task Dont_Register_The_Request_When_The_Target_Alredy_Requested_The_Current_Requester() {
        string targetId = "targetId";
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        List<BattleRequest> requests = new() {
            new BattleRequest() {
                target = callerContext.UserId,
                requester = targetId
            }
        };
        IGameHubState state = new GameHubStateBuilder().Build();
        A.CallTo(() => state
            .BattleRequests
            .RequestsWithUser(""))
                .WithAnyArguments()
                .Returns(requests);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.SendBattleRequest(targetId, callerContext);
        A.CallTo(() => state.BattleRequests.TryAdd(new()))
            .WithAnyArguments()
            .MustNotHaveHappened();
    }

    BattleRequest BattleRequestWithUsers(
        string requester,
        string target) {
        return A<BattleRequest>.That.Matches(req => 
            req.requester == requester && req.target == target);
    }

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
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.CancelBattleRequest(
            request.requestId, 
            caller);
        A.CallTo(() => state.BattleRequests.TryRemove(request))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Notify_Users_Of_A_Cancelled_Request() {
        IGameHubClient callerClient = A.Fake<IGameHubClient>();
        IHubCallerClients<IGameHubClient> hubConnections = 
            Utils.FakeHubCallerContext();
        A.CallTo(() => hubConnections.Caller)
            .Returns(callerClient);
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            hubConnections);
        string anotherUserId = "anotherUserId";
        IGameHubClient anotherUserClient = A.Fake<IGameHubClient>();
        A.CallTo(() => hubConnections.User(anotherUserId))
            .Returns(anotherUserClient);
        BattleRequest request = new() {
            requester = caller.UserId,
            target = anotherUserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.CancelBattleRequest(request.requestId, caller);
        A.CallTo(() => 
            callerClient.BattleRequestCancelled(
                caller.UserId, 
                request)
            )
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => 
            anotherUserClient.BattleRequestCancelled(
                caller.UserId, 
                request)
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Dont_Cancel_A_Request_That_The_Caller_Is_Not_Inner() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        string anotherUserId = "anotherUserId";
        BattleRequest request = new() {
            requester = "someUser",
            target = anotherUserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.CancelBattleRequest(
            request.requestId, 
            caller);
        A.CallTo(() => state.BattleRequests.TryRemove(request))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task Remove_Battle_Request_When_Accept_A_Battle() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => state.BattleRequests.TryRemove(request))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Requester_Can_Not_Accept_Battle() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = caller.UserId,
            target = "target"
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => state.BattleRequests.TryRemove(request))
            .WithAnyArguments()
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task Create_Battle_When_Accept_A_Request() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => state.Battles.TryAdd(A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Place_Entities_In_The_Created_Battle() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => state.Battles.TryAdd(UserIsInTheBattle(caller.UserId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => state.Battles.TryAdd(UserIsInTheBattle(request.requester)))
            .MustHaveHappenedOnceExactly();
    }

    IBattle UserIsInTheBattle(string userId) => 
        A<IBattle>.That.Matches(battle => 
            battle.Entities.Any(e => e.Id == userId));

    [TestMethod]
    public async Task When_Create_Battle_And_Dont_Found_Entity_Data_On_Db_Use_Entity_With_Default_Data()
    {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => 
            state.Battles.TryAdd(
                BattleWithDefaultEntityForUser(caller.UserId)))
            .MustHaveHappened();
    }

    IBattle BattleWithDefaultEntityForUser(string userId) {
        return A<IBattle>.That.Matches(b => 
            b.Entities.Exists(e => 
                e.Id == userId && EntityIsDefault(e)));
    }

    bool EntityIsDefault(IEntity entity) =>
        entity.Damage == 10
        && entity.DefenseAbsorption == 0.1
        && entity.HealthRadius == 25
        && entity.CurrentHealth.CoordinatesAreEqual(25, 25)
        && WeaponIsDefault(entity.Weapon);
    

    bool WeaponIsDefault(Weapon weapon) =>
        weapon.damageOnX == DamageDirection.Positive
        && weapon.damageOnY == DamageDirection.Neutral;

    [TestMethod]
    public async Task When_Create_Battle_Get_Entity_Data_From_DB()
    {
        IEntity callerEntity = Utils.FakeEntity("callerId");
        IEntity requesterEntity = Utils.FakeEntity("requesterId");
        CurrentCallerContext caller = new(
            callerEntity.Id, 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = requesterEntity.Id,
            target = caller.UserId
        };
        IGameHubState state = FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .WithDb(FakeDbWithEntities(callerEntity, requesterEntity))
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => 
            state.Battles.TryAdd(
                A<IBattle>.That.Matches(battle => 
                    EntitiesAreInBattle(battle, callerEntity, requesterEntity))))
            .MustHaveHappened();
    }

    IGameDb FakeDbWithEntities(params IEntity[] entities) {
        IGameDb gameDb = A.Fake<IGameDb>();
        foreach (var entity in entities)
            A.CallTo(() => gameDb.SearchEntity(entity.Id))
                .Returns(entity);
        return gameDb;
    }

    bool EntitiesAreInBattle(IBattle battle, params IEntity[] entities) {
        foreach (var entity in entities)
        {
            if (!battle.Entities.Contains(entity))
                return false;
        }
        return true;
    }

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
        IGameHubState state = FakeStateWithRequest(request);
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
        IGameEngine engine = new GameEngineBuilder()
            .WithState(FakeStateWithRequest(request))
            .WithDb(FakeDbWithEntities(callerEntity, requesterEntity))
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

    IGameHubState FakeStateWithRequest(BattleRequest request) {
        IGameHubState state = new GameHubStateBuilder().Build();
        AddRequestOnState(state, request);
        return state;
    }

    void AddRequestOnState(IGameHubState state, BattleRequest request) {
        A.CallTo(() => state.BattleRequests.Get(request.requestId))
            .Returns(request);
    }
}