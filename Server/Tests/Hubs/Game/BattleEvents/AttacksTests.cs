using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

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
    public void Register_Attacks() {
        string callerId = "callerId";
        string targetId = "targetId";
        var battles = BattleCollectionWithBattleFor(
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

    IBattleCollection BattleCollectionWithBattleFor(
        string firstEntityId, 
        string secondEntityId) 
    {
        var battles = new BattleCollection();
        var battle = CreateFakeBattleWith(firstEntityId, secondEntityId);
        battles.TryAdd(battle);
        return battles;
    }

    IBattle CreateFakeBattleWith(
        string firstEntityId, 
        string secondEntityId)
    {
        IBattle battle = A.Fake<IBattle>();
        A.CallTo(() => battle.Entities)
            .Returns(new List<IEntity>() {
                Utils.FakeEntity(firstEntityId),
                Utils.FakeEntity(secondEntityId)
            });
        return battle;
    }
}