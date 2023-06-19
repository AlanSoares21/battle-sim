using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Engine.Skills;

namespace BattleSimulator.Engine.Tests.SkillsTests;

[TestClass]
public class CommomTestsToDamageSkills
{
    [TestMethod]
    [DataRow(-9, 0, "basicNegativeDamageOnX")]
    [DataRow(0, -9, "basicNegativeDamageOnY")]
    [DataRow(0, 9, "basicPositiveDamageOnY")]
    [DataRow(9, 0, "basicPositiveDamageOnX")]
    public void After_Execute_Skill_Change_Target_Current_Life(
        int expectedX,
        int expectedY,
        string skillName
    ) 
    {
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        Coordinate expectedLifeAfterSkill = new(expectedX, expectedY);
        var battle = BattleToTest(source, target);
        var skill = GetSkill(skillName);
        skill.Exec(target, source, battle);
        Assert.AreEqual(expectedLifeAfterSkill, target.State.CurrentHealth);
    }
    
    [TestMethod]
    [DataRow("basicNegativeDamageOnX")]
    [DataRow("basicNegativeDamageOnY")]
    [DataRow("basicPositiveDamageOnY")]
    [DataRow("basicPositiveDamageOnX")]
    public void After_Execute_Skill_Notify_Damage(string skillName) 
    {
        var notifier = A.Fake<IEventsObserver>();
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        var battle = BattleToTest(notifier, source, target);
        var skill = GetSkill(skillName);
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
    [DataRow(-9, 0, "basicNegativeDamageOnX")]
    [DataRow(0, -9, "basicNegativeDamageOnY")]
    [DataRow(0, 9, "basicPositiveDamageOnY")]
    [DataRow(9, 0, "basicPositiveDamageOnX")]
    public void When_Notify_Skill_Damage_Send_The_Current_Target_Health(
        int expectedX,
        int expectedY,
        string skillName
    ) 
    {
        var notifier = A.Fake<IEventsObserver>();
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        Coordinate expectedLifeAfterSkill = new(expectedX, expectedY);
        var battle = BattleToTest(notifier, source, target);
        var skill = GetSkill(skillName);
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

    ISkillBase GetSkill(string name) {
        if (name == "basicNegativeDamageOnX")
            return new BasicNegativeDamageOnX();
        if (name == "basicNegativeDamageOnY")
            return new BasicNegativeDamageOnY();
        if (name == "basicPositiveDamageOnX")
            return new BasicPositiveDamageOnX();
        if (name == "basicPositiveDamageOnY")
            return new BasicPositiveDamageOnY();
        Assert.Fail($"Skill {name} is not registred, on GetSkill method");
        return null;
    }
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