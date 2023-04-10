using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Hubs.EventHandling;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class EventHandlerWorkerTests {
    /*
    [TestMethod]
    public void Apply_Skill_Effect() 
    {
        var skill = Utils.FakeSkill("skillName");
        var caller = Utils.FakeEntity(
            "callerId",
            Utils.NewSkillSet(skill)
        );
        var target = Utils.FakeEntity("targetId");
        var battles = Utils.BattleCollectionWithBattleFor(
            caller, 
            target
        );
        var eventsQueue = A.Fake<IEventsQueue>();
        IBattleEventsHandler eventsHandler = new BattleEventsHandlerBuilder()
            .WithBattles(battles)
            .WithEventsQueue(eventsQueue)
            .Build();
        eventsHandler.UseSkill(target.Id, caller.Id, skill.Name);
        A.CallTo(() => eventsQueue.Dequeue()).MustHaveHappenedOnceExactly();
        A.CallTo(() => skill.Exec(target, caller, A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
    */
}