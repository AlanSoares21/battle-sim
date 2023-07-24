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
        Coordinate[] equipCoordinates = new Coordinate[4] {
            new(1,1),
            new(2,1),
            new(2,2),
            new(1,2)
        };
        AddBarrierEquipToEntity(entity, equipCoordinates);
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
        Assert.AreEqual(entity.Equips.Count, result.Equips.Count);
        AssertEquips(entity.Equips, result.Equips);
    }

    IGameDb CreateDb(IJsonSerializerWrapper serializer) => 
        CreateDb(serializer, A.Fake<ISkillProvider>());


    [TestMethod]
    public void Update_Entity()
    {
        var firstEntity = Utils.NewDbEntity("entity");
        firstEntity.Damage = 10;
        Coordinate[] equipCoordinates = new Coordinate[4] {
            new(1,1),
            new(2,1),
            new(2,2),
            new(1,2)
        };
        AddBarrierEquipToEntity(firstEntity, equipCoordinates);
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
        Coordinate[] newEquipCoordinates = new Coordinate[4] {
            new(-1,-1),
            new(-2,-1),
            new(-2,-2),
            new(-1,-2)
        };
        AddBarrierEquipToEntity(firstEntity, newEquipCoordinates);
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
            db.Entities.Add(entity);   
    }

    [TestMethod]
    public void Register_Entity()
    {
        var entity = Utils.NewDbEntity("entity");
        entity.Damage = 10;
        Coordinate[] equipCoordinates = new Coordinate[4] {
            new(1,1),
            new(2,1),
            new(2,2),
            new(1,2)
        };
        AddBarrierEquipToEntity(entity, equipCoordinates);
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

    void AddBarrierEquipToEntity(Entity entity, params Coordinate[] coordinates)
    {
        entity.Equips.Add(new Equip() {
            Effect = Engine.Equipment.EquipEffect.Barrier,
            Shape = EquipShape.Rectangle,
            Coordinates = coordinates.ToList()
        });
    }

    void EntitiesAreEqual(Entity first, Entity second)
    {
        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(first.HealthRadius, second.HealthRadius);
        Assert.AreEqual(first.Damage, second.Damage);
        Assert.AreEqual(first.DefenseAbsorption, second.DefenseAbsorption);
        Assert.IsTrue(first.Skills.All(s => second.Skills.Contains(s)));
        AssertEquips(first.Equips, second.Equips);
    }

    /*
        Obs: four loops are used to check if all coordinates, off all equips from source
        match with at least one equip in target
    */
    void AssertEquips(List<Equip> source, List<Equip> target) {
        for (int i = 0; i < source.Count; i++)
        {
            var equip = source[i];
            for (int j = 0; j < target.Count; j++)
            {
                Assert.IsTrue(
                    equip.Coordinates.All(c  => 
                        target[j].Coordinates.Any(tc => tc.Equals(c))
                    ),
                    $"Source equip {i} dont have any equip with the same coordinates in the target list"
                );
                target.RemoveAt(j);
            }
        }
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