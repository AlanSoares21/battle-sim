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
public class GameDbTests
{
    [TestMethod]
    public void Return_Entities_Correct() 
    {
        var entity = new Entity() { Id = "entityOne" };
        string equipId = Utils.DefaultEquips[0].Id;
        AddEquipToEntity(entity, equipId);
        DbStructure dbStructure = new();
        AddEntitiesInDbStructure(dbStructure, entity);
        var serializer = SerializerWithDbStructre(dbStructure);
        IGameDb gameDb = CreateDb(serializer);
        var result = gameDb.SearchEntity(entity.Id);
        A.CallTo(() => 
            serializer.DeserializeFile<DbStructure>(An<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result.Id);
        Assert.IsTrue(result.Equips.Exists(e => e.EquipId == equipId));
    }
    
    [TestMethod]
    public void Return_Equips_Correct() 
    {
        DbStructure dbStructure = new();
        AddEquipsInDbStructure(dbStructure, Utils.DefaultEquips);
        var serializer = SerializerWithDbStructre(dbStructure);
        IGameDb gameDb = CreateDb(serializer);
        var result = gameDb.GetEquips();
        A.CallTo(() => 
            serializer.DeserializeFile<DbStructure>(An<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        Assert.IsNotNull(result);
        Assert.IsTrue(Utils.DefaultEquips.All(e1 => 
            result.Exists(e2 => e2.Id == e1.Id)));
    }

    void AddEquipsInDbStructure(DbStructure db, params Equip[] equips)
    {
        db.Equips = equips.ToList();
    }

    IGameDb CreateDb(IJsonSerializerWrapper serializer) => 
        CreateDb(serializer, A.Fake<ISkillProvider>());


    [TestMethod]
    public void Update_Entity()
    {
        var firstEntity = Utils.NewDbEntity("entity");
        firstEntity.Damage = 10;
        AddEquipToEntity(firstEntity, Utils.DefaultEquips[0].Id);
        DbStructure dbStructure = new();
        AddEntitiesInDbStructure(dbStructure, firstEntity);
        IGameDb db = CreateDb(
            SerializerWithDbStructre(dbStructure), 
            new SkillProvider()
        );
        var newEntity = Utils.NewDbEntity(firstEntity.Id);
        newEntity.Damage = 11;
        newEntity.HealthRadius = 3;
        newEntity.DefenseAbsorption = 30;
        AddEquipToEntity(firstEntity, Utils.DefaultEquips[1].Id);
        db.UpdateEntity(newEntity);
        var dbEntity = db.SearchEntity(firstEntity.Id);
        if (dbEntity is null)
            Assert.Fail($"{firstEntity.Id} not found on db");
        EntitiesAreEqual(newEntity, dbEntity);
    }

    void AddEntitiesInDbStructure(
        DbStructure db, 
        params Entity[] entities)
    {
        foreach (var entity in entities)
        {
            db.Entities.Add(entity);
            foreach (var equip in entity.Equips)
            {
                db.EntitiesEquips.Add(equip);
            }
        }   
    }

    [TestMethod]
    public void Register_Entity()
    {
        var entity = Utils.NewDbEntity("entity");
        entity.Damage = 10;
        AddEquipToEntity(entity, Utils.DefaultEquips[0].Id);
        IGameDb db = CreateDb(
            SerializerWithDbStructre(new()), 
            new SkillProvider()
        );
        db.AddEntity(entity);
        var dbEntity = db.SearchEntity(entity.Id);
        if (dbEntity is null)
            Assert.Fail($"{entity.Id} not registered on db");
        EntitiesAreEqual(entity, dbEntity);
    }

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

    [TestMethod]
    public void Return_Equip_On_Searching_By_Id()
    {
        DbStructure dbStructure = new() {
            Equips = Utils.DefaultEquips.ToList()
        };
        IGameDb db = CreateDb(
            SerializerWithDbStructre(dbStructure), 
            new SkillProvider()
        );
        var dbEquip = db.SearchEquip(Utils.DefaultEquips[0].Id);
        if (dbEquip is null)
            Assert.Fail($"{Utils.DefaultEquips[0].Id} not registered on db");
        Assert.AreEqual(Utils.DefaultEquips[0], dbEquip);
    }

    [TestMethod]
    public void Return_Null_When_Searching_An_Equip_That_Dont_Exists()
    {
        IGameDb db = CreateDb(
            SerializerWithDbStructre(new()), 
            new SkillProvider()
        );
        var dbEquip = db.SearchEquip("thisEquipDontExists");
        Assert.IsNull(dbEquip);
    }
    
    IJsonSerializerWrapper SerializerWithDbStructre(
        DbStructure db) 
    {
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