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
            Utils.FakeEntity(callerContext.UserId));
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
            Utils.FakeEntity("entityA"),
            Utils.FakeEntity("entityB")
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

    [TestMethod]
    public async Task Notify_Battle_Group_When_Battle_Is_Cancelled() 
    {
        var hubClients = Utils.FakeHubCallerContext();
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            hubClients);
        var battle = FakeBattleWithPlayers(
            Utils.FakeEntity(callerContext.UserId));
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
        string callerId = "callerUserId";
        CurrentCallerContext callerContext = new(
            callerId,
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var battle = FakeBattle(Guid.NewGuid(), callerId);
        IGameEngine engine = GameEngineWithBattleCollection(
            FakeBattleCollection(battle)
        );
        engine.Move(coordinate, callerContext);
        A.CallTo(() => 
            battle.RegisterMove(callerId, coordinate)
        ).MustHaveHappenedOnceExactly();
    }

    IBattle FakeBattle(Guid id, params string[] entitiesIds) 
    {
        IBattle battle = A.Fake<IBattle>();
        A.CallTo(() => battle.Id)
            .Returns(id);
        List<IEntity> fakeEntitties = new();
        foreach (var entityId in entitiesIds)
        {
            fakeEntitties.Add(Utils.FakeEntity(entityId));
        }
        A.CallTo(() => battle.Entities).Returns(fakeEntitties);
        return battle;        
    }

    IBattleCollection FakeBattleCollection(IBattle battle)
    {
        var battles = A.Fake<IBattleCollection>();
        foreach (var entity in battle.Entities)
        {
            A.CallTo(() => 
                battles.GetBattleIdByEntity(entity.Id)
            ).Returns(battle.Id);
        }
        A.CallTo(() => battles.Get(battle.Id))
            .Returns(battle);
        return battles;
    }

    IGameEngine GameEngineWithBattleCollection(
        IBattleCollection collection)
    {
        var state = new GameHubStateBuilder()
            .WithBattleCollection(collection)
            .Build();
        return new GameEngineBuilder()
            .WithState(state)
            .Build();
    }
    
}