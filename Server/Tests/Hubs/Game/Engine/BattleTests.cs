using BattleSimulator.Server.Tests.Builders;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests.Hubs.Game.Engine;

[TestClass]
public class BattleTests
{
    [TestMethod]
    public async Task Remove_Battle_When_Request_To_Cancel()
    {
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var battle = FakeBattleWithPlayers(
            FakeEntity(callerContext.UserId));
        A.CallTo(() => battle.EntityIsIntheBattle(callerContext.UserId))
            .Returns(true);
        var state = new GameHubStateBuilder().Build();
        AddBattleInTheState(battle, state);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.CancelBattle(battle.Id, callerContext);
        A.CallTo(() => state.Battles.TryRemove(battle))
            .MustHaveHappenedOnceExactly();
    }


    [TestMethod]
    public async Task User_Not_In_The_Battle_Can_Not_Cancel_The_Battle()
    {
        var battle = FakeBattleWithPlayers(
            FakeEntity("entityA"),
            FakeEntity("entityB")
        );   
        var state = new GameHubStateBuilder().Build();
        AddBattleInTheState(battle, state);
        CurrentCallerContext userNotInBattle = new(
            "userNotInBattleId",
            "userNotInBattleConnectionId",
            Utils.FakeHubCallerContext());
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.CancelBattle(battle.Id, userNotInBattle);
        A.CallTo(() => state.Battles.TryRemove(battle))
            .MustNotHaveHappened();    
    }

    IEntity FakeEntity(string identifier) {
        var entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Identifier).Returns(identifier);
        return entity;
    }

    [TestMethod]
    public async Task Notify_Battle_Group_When_Battle_Is_Cancelled() 
    {
        var hubClients = Utils.FakeHubCallerContext();
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);
        var battle = FakeBattleWithPlayers(
            FakeEntity(callerContext.UserId));
        A.CallTo(() => battle.EntityIsIntheBattle(callerContext.UserId))
            .Returns(true);
        var state = new GameHubStateBuilder().Build();
        AddBattleInTheState(battle, state);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        var groupClient = A.Fake<IGameHubClient>();
        A.CallTo(() => hubClients.Group(battle.Id.ToString()))
            .Returns(groupClient);
        await engine.CancelBattle(battle.Id, callerContext);
        A.CallTo(() => groupClient.BattleCancelled(
            callerContext.UserId,
            battle.Id
        )).MustHaveHappenedOnceExactly();
    }

    void AddBattleInTheState(IBattle battle, IGameHubState state) 
    {
        A.CallTo(() => state.Battles.Get(battle.Id)).Returns(battle);
    }

    IBattle FakeBattleWithPlayers(params IEntity[] entities) {
        var battle = FakeBattle(Guid.NewGuid());
        A.CallTo(() => battle.Entities).Returns(entities.ToList());
        return battle;
    }

    IBattle FakeBattle(Guid id) 
    {
        IBattle battle = A.Fake<IBattle>();
        A.CallTo(() => battle.Id)
            .Returns(id);
        return battle;        
    }

    [TestMethod]
    public async Task Do_Nothing_When_Try_Cancel_An_Unregistered_Battle()
    {
        Guid someBattleId = Guid.NewGuid();
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var state = new GameHubStateBuilder().Build();
        A.CallTo(() => state.Battles.Get(A<Guid>.Ignored))
            .Throws(() => new KeyNotFoundException());
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        var groupClient = A.Fake<IGameHubClient>();
        await engine.CancelBattle(someBattleId, callerContext);
        A.CallTo(() => state.Battles.TryRemove(A<IBattle>.Ignored))
            .MustNotHaveHappened();    
        A.CallTo(() => groupClient.BattleCancelled(
            A<string>.Ignored,
            A<Guid>.Ignored
        )).MustNotHaveHappened();
    }

    [TestMethod]
    public void Add_Movement_Intention() 
    {
        Coordinate coordinate = new(1, 2);
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var state = new GameHubStateBuilder().Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        engine.Move(coordinate, callerContext);
        A.CallTo(() => state
                .MovementIntentions
                .TryAdd(EntityMoveTo(callerContext.UserId, coordinate)))
            .MustHaveHappenedOnceExactly();
    }

    MovementIntention EntityMoveTo(string entityId, Coordinate coordinate) {
        return A<MovementIntention>.That.Matches(intention => 
            intention.cell.IsEqual(coordinate) &&
            intention.entityIdentifier == entityId);
    }
}