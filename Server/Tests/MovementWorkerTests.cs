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
    public async Task Use_Intervals_From_Config() 
    {
        IServerConfig config = FakeConfig();
        MovementIntentionsWorker worker = new (
            CreateBattleCollection(),
            FakeLogger(),
            config
        );
        await ExecuteOnce(worker);
        A.CallTo(() => config.MovementWorkerIntervalInMiliseconds)
            .MustHaveHappened();
        A.CallTo(() => config.IntervalToMoveEntitiesInSeconds)
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task Call_Move_Method() {
        var battle = A.Fake<IBattle>();
        var battleCollection = A.Fake<IBattleCollection>();
        A.CallTo(() => battleCollection.ListAll())
            .Returns(new List<IBattle>() { battle });
        MovementIntentionsWorker worker = CreateWorker(
            battleCollection,
            FakeConfig()
        );
        await ExecuteOnce(worker);

        A.CallTo(() => battle.MoveEntities())
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Only_Exec_Move_Each_Two_Seconds() {
        IServerConfig config = FakeConfig();
        A.CallTo(() => config.IntervalToMoveEntitiesInSeconds)
            .Returns(2);
        var battle = A.Fake<IBattle>();
        A.CallTo(() => battle.EntitiesMovedAt)
            .Returns(DateTime.MaxValue);
        var battleCollection = A.Fake<IBattleCollection>();
        A.CallTo(() => battleCollection.ListAll())
            .Returns(new List<IBattle>() { battle });
        MovementIntentionsWorker worker = CreateWorker(
            battleCollection,
            config
        );
        await ExecuteOnce(worker);

        A.CallTo(() => battle.MoveEntities())
            .MustNotHaveHappened();
    }
    
    IBattleCollection CreateBattleCollection() =>
        new BattleCollection();

    IBattle NewBattleWithEntity(IEntity entity) {
        IBattle battle = new Duel(
            Guid.NewGuid(), 
            GameBoard.WithDefaultSize(),
            A.Fake<ICalculator>(),
            A.Fake<IEventsObserver>());
        battle.AddEntity(entity);
        return battle;
    }

    IServerConfig FakeConfig() {
        var config = A.Fake<IServerConfig>();
        A.CallTo(() => config.MovementWorkerIntervalInMiliseconds)
            .Returns(1);
        return config;
    }

    MovementIntentionsWorker CreateWorker(
        IBattleCollection battleCollection, 
        IServerConfig config) => 
        new MovementIntentionsWorker(
            battleCollection,
            FakeLogger(),
            config
        );

    ILogger<MovementIntentionsWorker> FakeLogger() => 
        A.Fake<ILogger<MovementIntentionsWorker>>();
    
    async Task ExecuteOnce(MovementIntentionsWorker worker) 
    {
        var tokenSource = new CancellationTokenSource();
        var t = worker.StartAsync(tokenSource.Token);
        tokenSource.Cancel();
        await t;
    }
}