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
    public async Task Use_Interval_From_Config() 
    {
        var config = A.Fake<IServerConfig>();
        A.CallTo(() => config.MovementWorkerIntervalInMiliseconds).Returns(1);
        MovementIntentionsWorker worker = new (
            CreateBattleCollection(),
            FakeLogger(),
            config
        );
        await ExecuteOnce(worker);
        A.CallTo(() => config.MovementWorkerIntervalInMiliseconds)
            .MustHaveHappened();
    }

    //TODO:: test if only executes move on interval
    
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

    MovementIntentionsWorker CreateWorker(
        BattleCollection battleCollection, 
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