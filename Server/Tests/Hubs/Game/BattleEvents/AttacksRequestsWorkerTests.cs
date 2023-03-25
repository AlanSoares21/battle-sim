using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Workers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class AttacksRequestsWorkerTests
{
    [TestMethod]
    public void When_Execute_Attack_Notify_Clients() 
    {
        IEntity source = Utils.FakeEntity("sourceId");
        IEntity target = Utils.FakeEntity("targetId");
        IBattle battle = CreateDuel();
        battle.AddEntity(source, new(0,0));
        battle.AddEntity(target, new(0,0));
        IAttacksRequestedList attackList = new AttacksRequestedList();
        attackList.RegisterAttack(source.Id, target.Id);
        var client = A.Fake<IGameHubClient>();
        AttacksHandlerWorker worker = new(
            HubWithClientForThisBattle(battle, client),
            attackList,
            BattleCollectionWithBattle(battle),
            FakeLogger());
        worker.Handle();
        A.CallTo(() => 
            client.Attack(source.Id, target.Id, A<Coordinate>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void When_Execute_Attack_Remove_Register() 
    {
        IEntity source = Utils.FakeEntity("sourceId");
        IEntity target = Utils.FakeEntity("targetId");
        IBattle battle = CreateDuel();
        battle.AddEntity(source, new(0,0));
        battle.AddEntity(target, new(0,0));
        IAttacksRequestedList attackList = new AttacksRequestedList();
        attackList.RegisterAttack(source.Id, target.Id);
        var client = A.Fake<IGameHubClient>();
        AttacksHandlerWorker worker = new(
            HubWithClientForThisBattle(battle, client),
            attackList,
            BattleCollectionWithBattle(battle),
            FakeLogger());
        worker.Handle();
        Assert.IsTrue(attackList.ListAttacks().Count() == 0);
    }

    IBattle CreateDuel() =>
        new Duel(
            Guid.NewGuid(),
            GameBoard.WithDefaultSize(),
            new Calculator());

    IHubContext<GameHub, IGameHubClient> HubWithClientForThisBattle(
        IBattle battle, 
        IGameHubClient client)
    {
        var hub = FakeHub();
        A.CallTo(() => hub.Clients.Group(battle.Id.ToString()))
            .Returns(client);
        return hub;
    }

    IHubContext<GameHub, IGameHubClient> FakeHub() => 
        A.Fake<IHubContext<GameHub, IGameHubClient>>();

    IBattleCollection BattleCollectionWithBattle(IBattle battle) 
    {
        IBattleCollection battles = new BattleCollection();
        battles.TryAdd(battle);
        return battles;
    }

    ILogger<AttacksHandlerWorker> FakeLogger() =>
        A.Fake<ILogger<AttacksHandlerWorker>>();
}