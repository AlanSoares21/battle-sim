using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.DbTests;

[TestClass]
public class ConverterTests
{
    /*
        _ - Converte corretamente entidades do db para entidades do jogo
        _ - Cria equipamentos corretamente
        _ - Cria entidade com os equipamentos corretos
    */

    [TestMethod]
    public void Convert_Entity()
    {
        var entity = new Entity() { 
            Id = "entityOne",
            Damage = 10,
            DefenseAbsorption = 0.1,
            HealthRadius = 50
        };
        IGameDbConverter converter = CreateConverter();
        var entityConverted = converter.Entity(entity);
        EntityMatch(entity, entityConverted);
    }

    void EntityMatch(Entity source, IEntity converted)
    {
        Assert.AreEqual(source.Id, converted.Id);
        Assert.AreEqual(source.HealthRadius, converted.State.HealthRadius);
        Assert.AreEqual(source.Damage, 
            converted.OffensiveStats.Damage);
        Assert.AreEqual(source.DefenseAbsorption, 
            converted.DefensiveStats.DefenseAbsorption);
    }

    [TestMethod]
    public void Return_Entity_With_Skills() 
    {
        string skillName = "mySkillName";
        var entity = new Entity() { 
            Id = "entityOne", 
            Skills = new() { skillName } 
        };
        var skillProvider = A.Fake<ISkillProvider>();
        A.CallTo(() => skillProvider.Exists(skillName))
            .Returns(true);
        IGameDbConverter converter = CreateConverter(skillProvider);
        converter.Entity(entity);
        A.CallTo(() => skillProvider.Exists(skillName))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => skillProvider.Get(skillName))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void Return_Default_Entity()
    {
        IGameDbConverter converter = CreateConverter(
            new SkillProvider());
        var entity = converter.DefaultEntity("someEntityId");
        EntityIsDefault(entity);
    }

    void EntityIsDefault(IEntity entity)
    {
        OffensiveAttributesAreDefault(entity.OffensiveStats);
        DefensiveAttributesAreDefault(entity.DefensiveStats);
        StateAttributesAreDefault(entity.State);
        IsTheDefaultSkillSet(entity.Skills);
    }

    void OffensiveAttributesAreDefault(IOffensiveAttributes attributes) 
    {
        Assert.AreEqual(10, attributes.Damage);
    }

    void DefensiveAttributesAreDefault(IDefensiveAttributes attributes) 
    {
        Assert.AreEqual(0.1, attributes.DefenseAbsorption);
    }

    void StateAttributesAreDefault(IStateAttributes attributes) 
    {
        Assert.IsTrue(new Coordinate(0,0).IsEqual(attributes.CurrentHealth));
        Assert.AreEqual(25, attributes.HealthRadius);
    }

    void IsTheDefaultSkillSet(List<ISkillBase> skillsSet)
    {
        List<string> defaultSkills = GameDbConverter.DefaultSkills;
        Assert.AreEqual(defaultSkills.Count, skillsSet.Count, 
            $"Expect {defaultSkills.Count} skills, but the entity has {skillsSet.Count} skills.");
        bool[] skillIsPresent = new bool[defaultSkills.Count];
        foreach (var skill in skillsSet)
        {
            for (int i = 0; i < defaultSkills.Count; i++)
            {
                if (skill.Name == defaultSkills[i])
                    skillIsPresent[i] = true;
            }
        }
        for (int i = 0; i < skillIsPresent.Length; i++)
            Assert.IsTrue(skillIsPresent[i], $"Skill {defaultSkills[i]} is missing.");    
    }


    IGameDbConverter CreateConverter() =>
        CreateConverter(A.Fake<ISkillProvider>());

    IGameDbConverter CreateConverter(ISkillProvider provider) =>
        new GameDbConverter(
            provider, 
            A.Fake<ILogger<GameDbConverter>>());
}