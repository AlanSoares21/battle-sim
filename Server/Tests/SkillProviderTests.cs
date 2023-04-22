using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests;

[TestClass]
public class SkillProviderTests 
{
    [TestMethod]
    public void Returns_False_For_Invalid_Skill_Names() 
    {
        string invalidSkill = "invalidSkillName";
	    ISkillProvider provider = CreateProvider();
        Assert.IsFalse(provider.Exists(invalidSkill));
    }

    [TestMethod]
    [DataRow("basicNegativeDamageOnX")]
    public void Returns_True_For_Valid_Skill_Names(string skillName) 
    {
	    ISkillProvider provider = CreateProvider();
        Assert.IsTrue(provider.Exists(skillName));
    }

    [TestMethod]
    public void Throws_Exception_When_Try_Get_Invalid_Skill() 
    {
        string invalidSkill = "invalidSkillName";
	    ISkillProvider provider = CreateProvider();
        Assert.ThrowsException<KeyNotFoundException>(
            () => provider.Get(invalidSkill));
    }

    [TestMethod]
    [DataRow("basicNegativeDamageOnX")]
    public void Returns_The_Rigth_Skill(string skillName) 
    {
	    ISkillProvider provider = CreateProvider();
        ISkillBase skill = provider.Get(skillName);
        Assert.AreEqual(skillName, skill.Name);
    }

    ISkillProvider CreateProvider() => new SkillProvider();
}
