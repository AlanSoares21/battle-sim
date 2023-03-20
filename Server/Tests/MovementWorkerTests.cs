using BattleSimulator.Server.Tests.Builders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Workers;

namespace BattleSimulator.Server.Tests;

[TestClass]
public class MovementWorkerTests
{
    [TestMethod]
    public void When_The_Entity_Is_Not_In_A_Battle_Remove_Intention() {
        string entityId = "entityId";
        var movementIntentions = CreateMoveIntentionsCollection();
        MovementIntention intention = new() {
            entityIdentifier = entityId,
            cell = new(1, 2)
        };
        movementIntentions.TryAdd(intention);
        IGameHubState state = new GameHubStateBuilder()
            .WithMovementIntentionCollection(movementIntentions)
            .Build();
        A.CallTo(() => state.Battles.GetBattleIdByEntity(entityId))
            .Throws<KeyNotFoundException>();
        MovementIntentionsWorker worker = CreateWorkerWithState(state);
        worker.MoveEntities();
        var movements = movementIntentions.List();
        Assert.IsFalse(movements.Contains(intention));
    }

    [TestMethod]
    public void When_The_Intention_Can_Not_Be_Resolved_Then_Remove_It() {
        IEntity entity = CreateEntity("entityId");
        var battles = BattleCollectionWithEntityOnABattle(entity);
        var movementIntentions = CreateMoveIntentionsCollection();
        Coordinate invalidCellToMove = new(-1, -1);
        MovementIntention intention = new() {
            entityIdentifier = entity.Id,
            cell = invalidCellToMove
        };
        movementIntentions.TryAdd(intention);
        IGameHubState state = new GameHubStateBuilder()
            .WithBattleCollection(battles)
            .WithMovementIntentionCollection(movementIntentions)
            .Build();
        MovementIntentionsWorker worker = CreateWorkerWithState(state);
        worker.MoveEntities();
        var movements = movementIntentions.List();
        Assert.IsFalse(movements.Contains(intention));
    }

    [TestMethod]
    public void When_The_Intention_Are_Resolved_Then_Remove_It() {
        IEntity entity = CreateEntity("entityId");
        var battles = BattleCollectionWithEntityOnABattle(entity);
        var movementIntentions = CreateMoveIntentionsCollection();
        MovementIntention intention = new() {
            entityIdentifier = entity.Id,
            cell = new(1, 0)
        };
        movementIntentions.TryAdd(intention);
        IGameHubState state = new GameHubStateBuilder()
            .WithBattleCollection(battles)
            .WithMovementIntentionCollection(movementIntentions)
            .Build();
        MovementIntentionsWorker worker = CreateWorkerWithState(state);
        worker.MoveEntities();
        var movements = movementIntentions.List();
        Assert.IsFalse(movements.Contains(intention));
    }

    IEntity CreateEntity(string id) {
        IEntity entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Id).Returns(id);
        return entity;
    }

    IBattleCollection BattleCollectionWithEntityOnABattle(IEntity entity) {
        var battles = CreateBattleCollection();
        battles.TryAdd(NewBattleWithEntity(entity));
        return battles;
    }

    MovementIntentionsWorker CreateWorkerWithState(IGameHubState state) => new(
        FakeHubContext(),
        state,
        FakeLogger()
    );

    [TestMethod]
    public void When_Move_A_Entity_Then_Notify_The_Battle_Group() {
        string entityId = "entityId";
        var battle = NewBattleWithEntity(CreateEntity(entityId));
        var battles = CreateBattleCollection();
        battles.TryAdd(battle);
        var movementIntentions = CreateMoveIntentionsCollection();
        movementIntentions.TryAdd(new MovementIntention() {
            entityIdentifier = entityId,
            cell = new(7, 7)
        });
        IGameHubState state = new GameHubStateBuilder()
            .WithBattleCollection(battles)
            .WithMovementIntentionCollection(movementIntentions)
            .Build();
        IGameHubClient groupClient = A.Fake<IGameHubClient>();
        var hub = FakeHubContext();
        A.CallTo(() => hub.Clients.Group(battle.Id.ToString()))
            .Returns(groupClient);
        MovementIntentionsWorker worker = new (hub, state, FakeLogger());
        worker.MoveEntities();
        A.CallTo(
            () => groupClient.EntityMove(entityId, A<int>.Ignored, A<int>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    IBattleCollection CreateBattleCollection() =>
        new BattleCollection();

    IBattle NewBattleWithEntity(IEntity entity) {
        IBattle battle = new Duel(Guid.NewGuid(), GameBoard.WithDefaultSize());
        battle.AddEntity(entity);
        return battle;
    }

    IMovementIntentionCollection CreateMoveIntentionsCollection() =>
        new MovementIntentionCollection();

    IHubContext<GameHub, IGameHubClient> FakeHubContext() =>
        A.Fake<IHubContext<GameHub, IGameHubClient>>();

    ILogger<MovementIntentionsWorker> FakeLogger() => 
        A.Fake<ILogger<MovementIntentionsWorker>>();
}