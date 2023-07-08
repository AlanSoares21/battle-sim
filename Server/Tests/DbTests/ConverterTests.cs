using BattleSimulator.Engine;
using BattleSimulator.Engine.Equipment;
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
    public void Throw_Exception_When_Try_Convert_Equip_That_Dont_Exists() 
    {
        string equipId = Utils.DefaultEquips[1].Id;
        var equip = new EntityEquip() {
            EquipId = equipId,
            Coordinates = new() {
                new(0, 1),
                new(0,2),
                new(4,2),
                new(4,1)
            }
        };
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEquip(equipId))
            .Returns(null);
        IGameDbConverter converter = CreateConverter(db);
        Assert.ThrowsException<Exception>(() => converter.Equip(equip));
    }

    [TestMethod]
    public void Convert_Equip() 
    {
        Equip equip = Utils.DefaultEquips[1];
        var entityEquip = new EntityEquip() {
            EquipId = equip.Id,
            Coordinates = new() {
                new(0,1),
                new(0,2),
                new(4,2),
                new(4,1)
            }
        };
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEquip(equip.Id))
            .Returns(Utils.DefaultEquips[1]);
        IGameDbConverter converter = CreateConverter(db);
        var gameEquip = converter.Equip(entityEquip);
        A.CallTo(() => db.SearchEquip(equip.Id))
            .MustHaveHappenedOnceExactly();
        Assert.AreEqual(equip.Effect, gameEquip.Effect);
        Assert.AreEqual(entityEquip.Coordinates.Count, 
            gameEquip.Position.Coordinates.Length);
        for (int i = 0; i < entityEquip.Coordinates.Count; i++)
        {
            Assert.AreEqual(
                entityEquip.Coordinates[i], 
                gameEquip.Position.Coordinates[i], 
                $"cordinate {entityEquip.Coordinates[i]} is not equal to {gameEquip.Position.Coordinates[i]} in position {i}");
        }
    }

    IGameDbConverter CreateConverter(IGameDb db) =>
        new GameDbConverter(
            A.Fake<ISkillProvider>(), 
            A.Fake<ILogger<GameDbConverter>>(),
            db);

    IGameDbConverter CreateConverter() =>
        CreateConverter(A.Fake<ISkillProvider>());

    IGameDbConverter CreateConverter(ISkillProvider provider) =>
        new GameDbConverter(
            provider, 
            A.Fake<ILogger<GameDbConverter>>(),
            A.Fake<IGameDb>());
}