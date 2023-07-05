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

    // todo:: serializa e recupera equips corretamente

    [TestMethod]
    public void Update_Entity()
    {
        var firstEntity = Utils.NewDbEntity("entity");
        firstEntity.Damage = 10;
        AddEquipToEntity(firstEntity, DefaultEquips[0].Id);
        IGameDb db = CreateDb(
            SerializerWithEntities(firstEntity), 
            new SkillProvider()
        );
        var newEntity = Utils.NewDbEntity(firstEntity.Id);
        newEntity.Damage = 11;
        newEntity.HealthRadius = 3;
        newEntity.DefenseAbsorption = 30;
        AddEquipToEntity(firstEntity, DefaultEquips[1].Id);
        db.UpdateEntity(newEntity);
        var dbEntity = db.SearchEntity(firstEntity.Id);
        if (dbEntity is null)
            Assert.Fail($"{firstEntity.Id} not found on db");
        EntitiesAreEqual(newEntity, dbEntity);
    }

    Equip[] DefaultEquips = new Equip[] 
    {
        new Equip() {
            Id = "equip01"
        },
        new Equip() {
            Id = "equip02"
        }
    };

    void AddEquipToEntity(Entity entity, string equipId)
    {
        entity.Equips.Add(new EntityEquip() {
            EntityId = entity.Id,
            EquipId = equipId
        });
    }

    void EntitiesAreEqual(Entity first, Entity second)
    {
        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(first.HealthRadius, second.HealthRadius);
        Assert.AreEqual(first.Damage, second.Damage);
        Assert.AreEqual(first.DefenseAbsorption, second.DefenseAbsorption);
        Assert.IsTrue(first.Skills.All(s => second.Skills.Contains(s)));
        Assert.IsTrue(first.Equips.All(e1 => 
            second.Equips.Any(e2 => e1.EquipId == e2.EquipId)));
    }
    
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