using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Hubs.Game.State;

[TestClass]
public class BattleCollectionTests
{
    [TestMethod]
    public void Add_Battle() {
        IBattle battle = A.Fake<IBattle>();
        IBattleCollection battleCollection = new BattleCollection();
        bool result = battleCollection.TryAdd(battle);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Get_Battle() {
        Guid myBattleGuid = Guid.Empty;
        IBattle battle = FakeBattle(myBattleGuid);
        IBattleCollection battleCollection = new BattleCollection();
        battleCollection.TryAdd(battle);
        var battleAdded = battleCollection.Get(myBattleGuid);
        Assert.AreEqual(battle, battleAdded);
    }

    [TestMethod]
    public void When_Try_Add_A_Battle_With_Duplicated_Id_Return_False() {
        Guid battleId = Guid.Empty;
        IBattle firstBattle = FakeBattle(battleId);
        IBattle secondBattle = FakeBattle(battleId);
        IBattleCollection battleCollection = new BattleCollection();
        battleCollection.TryAdd(firstBattle);
        bool result = battleCollection.TryAdd(secondBattle);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Remove_Battle() {
        IBattle battle = A.Fake<IBattle>();
        IBattleCollection battleCollection = new BattleCollection();
        battleCollection.TryAdd(battle);
        bool result = battleCollection.TryRemove(battle);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Try_Get_A_Removed_Battle_Throws_Exception() {
        Guid battleId = Guid.Empty;
        IBattle battle = FakeBattle(battleId);
        IBattleCollection battleCollection = new BattleCollection();
        battleCollection.TryAdd(battle);
        battleCollection.TryRemove(battle);
        Assert.ThrowsException<KeyNotFoundException>(() => 
            battleCollection.Get(battleId));
    }

    IBattle FakeBattle(Guid id) {
        IBattle battle = A.Fake<IBattle>();
        A.CallTo(() => battle.Id).Returns(id);
        return battle;
    }

    [TestMethod]
    public void Return_BattleId_Searching_By_Entity_Identifier() {
        string entityIdentifier = "someIdentifier";
        Guid battleId = Guid.NewGuid();
        IBattleCollection battleCollection = new BattleCollection();
        IBattle battle = FakeBattleWithFakeEntities(entityIdentifier);
        A.CallTo(() => battle.Id).Returns(battleId);
        battleCollection.TryAdd(battle);
        Guid recoverdBattleId = battleCollection
            .GetBattleIdByEntity(entityIdentifier);
        Assert.AreEqual(battleId, recoverdBattleId);
    }

    [TestMethod]
    public void Throws_Exception_When_Dont_Find_BattleId_Searching_By_Entity_Identifier() {
        string entityInBattle = "entityInBattleIdentifier";
        string entityOutOfBattle = "entityOutOfBattleIdentifier";
        IBattleCollection battleCollection = new BattleCollection();
        IBattle battle = FakeBattleWithFakeEntities(entityInBattle);
        battleCollection.TryAdd(battle);
        Assert.ThrowsException<KeyNotFoundException>(() => 
            battleCollection.GetBattleIdByEntity(entityOutOfBattle));
    }

    IBattle FakeBattleWithFakeEntities(params string[] identifiers) {
        List<IEntity> entities = ListWithFakeEntities(identifiers);
        return FakeBattleWithEntities(entities);
    }
    List<IEntity> ListWithFakeEntities(params string[] identifiers) {
        List<IEntity> entities = new();
        foreach (string identifier in identifiers) 
            entities.Add(FakeEntity(identifier));
        return entities;
    }

    IEntity FakeEntity(string identifier) {
        IEntity entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Identifier).Returns(identifier);
        return entity;
    }
    IBattle FakeBattleWithEntities(List<IEntity> entities) {
        IBattle battle = A.Fake<IBattle>();
        A.CallTo(() => battle.Entities).Returns(entities);
        return battle;
    }
}