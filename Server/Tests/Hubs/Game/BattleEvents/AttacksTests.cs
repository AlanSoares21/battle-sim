using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class AttacksTests {
    
    [TestMethod]
    public void Dont_Register_Attacks_If_The_Source_Isent_In_Battle() {
        string callerId = "callerId";
        string targetId = "targetId";
        var battles = A.Fake<IBattleCollection>();
        A.CallTo(() => battles.GetBattleIdByEntity(An<string>.Ignored))
            .Throws<KeyNotFoundException>();
        var attacksRequested = A.Fake<IAttacksRequestedList>();
        IBattleEventsHandler eventsHandler = new BattleEventsHandlerBuilder()
            .WithBattles(battles)
            .WithAttackList(attacksRequested)
            .Build();
        eventsHandler.Attack(targetId, callerId);
        A.CallTo(() => battles.GetBattleIdByEntity(callerId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => attacksRequested.RegisterAttack(callerId, targetId))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public void Dont_Register_Attacks_If_The_Attack_Can_Not_Be_Executed() {
        IEntity caller = Utils.FakeEntity("callerId");
        IEntity target = Utils.FakeEntity("targetId");
        var battles = new BattleCollection();
        var battle = Utils.CreateDuel();
        battle.AddEntity(caller, new(0, 0));
        battle.AddEntity(target, new(2, 0));
        battles.TryAdd(battle);
        var attacksRequested = A.Fake<IAttacksRequestedList>();
        
        IBattleEventsHandler eventsHandler = new BattleEventsHandlerBuilder()
            .WithBattles(battles)
            .WithAttackList(attacksRequested)
            .Build();

        eventsHandler.Attack(target.Id, caller.Id);

        A.CallTo(() => attacksRequested.RegisterAttack(caller.Id, target.Id))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public void Register_Attacks() {
        string callerId = "callerId";
        string targetId = "targetId";
        var battles = Utils.BattleCollectionWithBattleFor(
            callerId, 
            targetId
        );
        var attacksRequested = A.Fake<IAttacksRequestedList>();
        IBattleEventsHandler eventsHandler = new BattleEventsHandlerBuilder()
            .WithBattles(battles)
            .WithAttackList(attacksRequested)
            .Build();
        eventsHandler.Attack(targetId, callerId);
        A.CallTo(() => attacksRequested.RegisterAttack(callerId, targetId))
            .MustHaveHappenedOnceExactly();
    }
}