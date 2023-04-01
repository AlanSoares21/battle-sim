using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Hubs.EventHandling;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class SkillsTests
{
    [TestMethod]
    public void Dont_Enqueue_Skill_When_The_User_Dont_Have_A_Skill_With_That_Name() 
    {
        string skillName = "someRandomSkillName";
        string callerId = "callerId";
        string targetId = "targetId";
        var eventsQueue = A.Fake<IEventsQueue>();
        IBattleEventsHandler eventsHandler = new BattleEventsHandlerBuilder()
            .WithEventsQueue(eventsQueue)
            .Build();
        eventsHandler.UseSkill(targetId, callerId, skillName);
        A.CallTo(() => eventsQueue.Enqueue(A<IGameEvent>.Ignored))
            .WithAnyArguments()
            .MustNotHaveHappened();
    }

    [TestMethod]
    public void Enqueue_Skill_Event() {
        string skillName = "someSkillName";
        
        var caller = Utils.FakeEntity("callerId");
        A.CallTo(() => caller.Skills)
            .Returns(NewSkillSet(skillName));
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
        eventsHandler.UseSkill(target.Id, caller.Id, skillName);
        A.CallTo(() => eventsQueue.Enqueue(A<IGameEvent>.That.Matches(
            e => 
                e.Source == caller.Id 
                && e.Target == target.Id 
                && e.Type == GameEventType.Skill
                && e.Skill != null
                && e.Skill.Name == skillName
        )))
            .MustHaveHappenedOnceExactly();
    }

    List<ISkillBase> NewSkillSet(params string[] names) {
        List<ISkillBase> value = new();
        foreach (var name in names)
        {
            var skill = A.Fake<ISkillBase>();
            A.CallTo(() => skill.Name)
                .Returns(name);
            value.Add(skill);
        }
        return value;
    }
}