using BattleSimulator.Engine;
using BattleSimulator.Server.Hubs.EventHandling;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class EventsObserverTests
{
    [TestMethod]
    public void Call_Skill_Damage_Subscribers_When_Execute_Skill_Damage_Method() 
    {
        var listener = A.Fake<Action<string, string, string, Coordinate>>();
        var observer = CreateObserver();
        observer.SubscribeToSkillDamage(listener);
        string skillName = "someSkillName";
        string sourceId = "someSourceId";
        string targetId = "someTargetId";
        Coordinate currentHealth = new(0, 0);
        observer.SkillDamage(skillName, sourceId, targetId, currentHealth);
        A.CallTo(() => listener(skillName, sourceId, targetId, currentHealth))
            .MustHaveHappenedOnceExactly();
    }

    EventsObserver CreateObserver() 
    {
        return new EventsObserver();
    }
}