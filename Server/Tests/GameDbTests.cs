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
            serializer.DeserializeFile<DbStructure>(An<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result.Id);
    }

    IGameDb CreateDb(IJsonSerializerWrapper serializer) => 
        CreateDb(serializer, A.Fake<ISkillProvider>());

    Equip[] DefaultEquips = new Equip[] 
    {
        new Equip() {
            Id = "equip01"
        }
    };
    
    IJsonSerializerWrapper SerializerWithEntities(
        params Entity[] entities) 
    {
        DbStructure db = new();
        db.Entities = entities.ToList();
        db.Equips = DefaultEquips.ToList();
        foreach (var entity in entities)
        {
            foreach (var equip in entity.Equips)
            {
                db.EntitiesEquips.Add(equip);
            }
        }
        var serializer = A.Fake<IJsonSerializerWrapper>();
        A.CallTo(() => 
            serializer.DeserializeFile<DbStructure>(An<string>.Ignored))
            .Returns(db);
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