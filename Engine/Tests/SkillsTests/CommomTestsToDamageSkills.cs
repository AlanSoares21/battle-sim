using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Engine.Skills;

namespace BattleSimulator.Engine.Tests.SkillsTests;

[TestClass]
public class CommomTestsToDamageSkills
{
    [TestMethod]
    [DataRow("basicNegativeDamageOnX")]
    [DataRow("basicNegativeDamageOnY")]
    [DataRow("basicPositiveDamageOnY")]
    [DataRow("basicPositiveDamageOnX")]
    public void Dont_Execute_Skill_When_The_Target_Is_Away_From_The_Skill_Range(
        string skillName
    ) {
        var source = NewEntity("sourceId");
        var target = NewEntity("targetId");
        Coordinate expectedLifeAfterSkill = new(0, 0);
        var battle = BattleToTest();
        battle.AddEntity(target);
        battle.AddEntity(source);
        var skill = GetSkill(skillName);
        skill.Exec(target, source, battle);
        Assert.AreEqual(expectedLifeAfterSkill, target.State.CurrentHealth);
    }

    [TestMethod]
    [DataRow("basicNegativeDamageOnX")]
    [DataRow("basicNegativeDamageOnY")]
    [DataRow("basicPositiveDamageOnY")]
    [DataRow("basicPositiveDamageOnX")]
    public void Dont_Execute_Skill_When_The_Source_Dont_Have_Mana_Enough(
        string skillName
    ) {
        var source = NewEntity("sourceId");
        source.State.Mana = 0;
        var target = NewEntity("targetId");
        Coordinate expectedLifeAfterSkill = new(0, 0);
        var battle = BattleToTest(target, source);
        var skill = GetSkill(skillName);
        skill.Exec(target, source, battle);
        Assert.AreEqual(expectedLifeAfterSkill, target.State.CurrentHealth);
    }

    //TODO:: after exec the skill, reduce the current mana ammount of the source

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
        for (int i = 0; i < entities.Length; i++)
        {
            battle.AddEntity(entities[i], new Coordinate(0, i));
        }
        return battle;
    }
    IEntity NewEntity(string id) {
        var player = new Player(id);
        player.State.Mana = player.State.MaxMana / 2;
        return player;
    }
}