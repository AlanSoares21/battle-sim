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
    
    IGameDb CreateDb(
        IJsonSerializerWrapper serializer, 
        ISkillProvider skillProvider) => 
        new GameDb(
            serializer, 
            A.Fake<IServerConfig>(), 
            skillProvider, 
            A.Fake<ILogger<GameDb>>());
}