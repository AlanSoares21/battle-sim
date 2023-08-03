using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Tests.Builders;
using BattleSimulator.Server.Workers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests;

[TestClass]
public class ManaRecoveryTests
{
    [TestMethod]
    public async Task Use_Interval_From_Config() 
    {
        var config = ManaRecoveryWorkerBuilder.FakeServerConfig();
        ManaRecoveryWorker worker = new ManaRecoveryWorkerBuilder()
            .WithConfig(config)
            .Build();
        await ExecuteOnce(worker);
        A.CallTo(() => config.ManaRecoveryWorkerIntervalInMiliseconds)
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task Call_Battle_Recover_Mana() 
    {
        var battle = A.Fake<IBattle>();
        var battleCollection = A.Fake<IBattleCollection>();
        A.CallTo(() => battleCollection.ListAll())
            .Returns(new List<IBattle>() { battle });
        ManaRecoveryWorker worker = new ManaRecoveryWorkerBuilder()
            .WithBattleCollection(battleCollection)
            .Build();
        await ExecuteOnce(worker);
        A.CallTo(() => battle.RecoverMana())
            .MustHaveHappenedOnceExactly();
    }

    // todo:: não recupera mana, quando faz menos de 5 segundos que a entidade recebeu mana
    [TestMethod]
    public async Task Only_Recover_Mana_Each_Five_Seconds() {
        var battle = A.Fake<IBattle>();
        A.CallTo(() => battle.ManaRecoveredAt)
            .Returns(DateTime.MaxValue);
        var battleCollection = A.Fake<IBattleCollection>();
        A.CallTo(() => battleCollection.ListAll())
            .Returns(new List<IBattle>() { battle });
        ManaRecoveryWorker worker = new ManaRecoveryWorkerBuilder()
            .WithBattleCollection(battleCollection)
            .Build();
        await ExecuteOnce(worker);

        A.CallTo(() => battle.RecoverMana())
            .MustNotHaveHappened();
    }
    
    async Task ExecuteOnce(ManaRecoveryWorker worker) 
    {
        var tokenSource = new CancellationTokenSource();
        var t = worker.StartAsync(tokenSource.Token);
        tokenSource.Cancel();
        await t;
    }
}