using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Engine.Skills;

namespace BattleSimulator.Engine.Tests.SkillsTests;

[TestClass]
public class BasicNegativeDamageOnXTests 
{
    [TestMethod]
    public void After_Execute_Skill_Change_Target_Current_Life() 
    {
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        Coordinate expectedLifeAfterSkill = 
            new(16, target.State.CurrentHealth.Y);
        var battle = BattleToTest(source, target);
        var skill = CreateSkill();
        skill.Exec(target, source, battle);
        Assert.AreEqual(expectedLifeAfterSkill, target.State.CurrentHealth);
    }
    
    [TestMethod]
    public void After_Execute_Skill_Notify_Damage() 
    {
        var notifier = A.Fake<IEventsObserver>();
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        var battle = BattleToTest(notifier, source, target);
        var skill = CreateSkill();
        skill.Exec(target, source, battle);
        A.CallTo(() => notifier
            .SkillDamage(
                skill.Name, 
                source.Id, 
                target.Id,
                A<Coordinate>.Ignored)
            )
            .MustHaveHappenedOnceExactly();
    }
    
    [TestMethod]
    public void When_Notify_Skill_Damage_Send_The_Current_Target_Health() 
    {
        var notifier = A.Fake<IEventsObserver>();
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        Coordinate expectedLifeAfterSkill = 
            new(16, target.State.CurrentHealth.Y);
        var battle = BattleToTest(notifier, source, target);
        var skill = CreateSkill();
        skill.Exec(target, source, battle);
        A.CallTo(() => notifier
            .SkillDamage(
                A<string>.Ignored, 
                A<string>.Ignored, 
                A<string>.Ignored,
                A<Coordinate>.That.Matches(v => v.IsEqual(expectedLifeAfterSkill)))
            )
            .MustHaveHappenedOnceExactly();
    }

    ISkillBase CreateSkill() => new BasicNegativeDamageOnX();
    IBattle BattleToTest(params IEntity[] entities) => 
        BattleToTest(A.Fake<IEventsObserver>(), entities);
    
    IBattle BattleToTest(IEventsObserver notifier, params IEntity[] entities) 
    {
        var battle = new Duel(
            System.Guid.NewGuid(),
            GameBoard.WithDefaultSize(),
            new Calculator(),
            notifier);
        foreach(var entity in entities)
            battle.AddEntity(entity);
        return battle;
    }

    IEntity NewEntity(string id) => new Player(id);
}