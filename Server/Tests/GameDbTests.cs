using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests;

[TestClass]
public class GameDbTests
{
    [TestMethod]
    public void Return_Entities_Correct() 
    {
        var entity = new Entity() { Id = "entityOne" };
        List<Entity> entities = new() { entity };
        var serializer = SerializerWithEntities(entity);
        IGameDb gameDb = CreateDb(serializer);
        var result = gameDb.SearchEntity(entity.Id);
        A.CallTo(() => 
            serializer.DeserializeFile<List<Entity>>(An<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result.Id);
    }

    IGameDb CreateDb(IJsonSerializerWrapper serializer) => 
        CreateDb(serializer, A.Fake<ISkillProvider>());

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
        IGameDb gameDb = CreateDb(
            SerializerWithEntities(entity),
            skillProvider
        );
        gameDb.SearchEntity(entity.Id);
        A.CallTo(() => skillProvider.Exists(skillName))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => skillProvider.Get(skillName))
            .MustHaveHappenedOnceExactly();
    }

    IJsonSerializerWrapper SerializerWithEntities(params Entity[] entities) {
        var serializer = A.Fake<IJsonSerializerWrapper>();
        A.CallTo(() => 
            serializer.DeserializeFile<List<Entity>>(An<string>.Ignored))
            .Returns(entities.ToList());
        return serializer;
    }

    [TestMethod]
    public void When_Not_Found_Entity_Return_A_Default_Entity()
    {
        var entity = new Entity() { Id = "entityOne" };
        var skillProvider = A.Fake<ISkillProvider>();
        IGameDb gameDb = CreateDb(SerializerWithEntities(), new SkillProvider());
        var result = gameDb.GetEntityFor(entity.Id);
        Assert.AreEqual(entity.Id, result.Id);
        EntityIsDefault(result);
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
        List<string> defaultSkills = GameDb.DefaultSkills;
        Assert.AreEqual(defaultSkills.Count, skillsSet.Count);
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

    IGameDb CreateDb(
        IJsonSerializerWrapper serializer, 
        ISkillProvider skillProvider) => 
        new GameDb(
            serializer, 
            A.Fake<IServerConfig>(), 
            skillProvider, 
            A.Fake<ILogger<GameDb>>());
}