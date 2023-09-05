using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Tests.Builders;

namespace BattleSimulator.Engine.Tests.DuelTests;

[TestClass]
public class FunctionalTests
{
    [TestMethod]
    public void Places_Entity_On_The_Board() {
        string entityIdentifier = "id";
        IEntity entity = Utils.FakeEntity(entityIdentifier);
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(entity);
        Assert.IsTrue(EntityIsInBoard(entityIdentifier, battle.Board));
    }

    bool EntityIsInBoard(string identifier, IBoard board) =>
        board.GetEntities().Contains(identifier);

    [TestMethod]
    [DataRow("entity1", "entity2")]
    public void Places_Each_Entity_On_Opposite_Sides_Of_The_Board(
        string firstEntityIdentifier, 
        string secondEntityIdentifier
    ) {
        IEntity firstEntity, secondEntity;
        firstEntity = Utils.FakeEntity(firstEntityIdentifier);
        secondEntity = Utils.FakeEntity(secondEntityIdentifier);
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(firstEntity);
        battle.AddEntity(secondEntity);
        Coordinate firstEntityCell = battle
            .Board
            .GetEntityPosition(firstEntity.Id);
        Coordinate secondEntityCell = battle
            .Board
            .GetEntityPosition(secondEntity.Id);
        Coordinate oppositeCell = battle
            .Board
            .GetOppositeCoordinate(firstEntityCell);
        Assert.IsTrue(oppositeCell.IsEqual(secondEntityCell));
    }

    [TestMethod]
    [DataRow("entity1", "entity2", "entity3")]
    public void Dont_Places_More_Than_Two_Entities_On_The_Board(
        string firstEntityIdentifier, 
        string secondEntityIdentifier,
        string thirdEntityIdentifier
    ) {
        IEntity firstEntity, secondEntity, thirdEntity; 
        firstEntity = Utils.FakeEntity(firstEntityIdentifier);
        secondEntity = Utils.FakeEntity(secondEntityIdentifier);
        thirdEntity = Utils.FakeEntity(thirdEntityIdentifier);
        IBattle battle = Utils.CreateDuelWithEntities(
            firstEntity,
            secondEntity,
            thirdEntity
        );
        int ammountEntitiesInTheBoard = battle
            .Board
            .GetEntities()
            .Count;
        Assert.AreEqual(2, ammountEntitiesInTheBoard);
    }

    [TestMethod]
    [DataRow(3,5, 3,4)]
    [DataRow(5,3, 4,3)]
    [DataRow(5,5, 4,4)]
    [DataRow(3,1, 3,2)]
    [DataRow(1,3, 2,3)]
    [DataRow(1,1, 2,2)]
    [DataRow(5,1, 4,2)]
    [DataRow(1,5, 2,4)]
    public async Task Move_Entity_From_Middle_Cell_To_Coordinate(
        int x, int y, int expectedX, int expectedY
    ) {
        Coordinate moveTo = new(x, y);
        Coordinate middle = new(3, 3);
        IEntity entity = Utils.FakeEntity("entityOne");
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(entity, middle);
        battle.RegisterMove(entity.Id, moveTo);
        await battle.MoveEntities();
        Coordinate cell = battle
            .Board
            .GetEntityPosition(entity.Id);
        Assert.AreEqual(expectedX, cell.X,
            "A coordenada X não está correta");
        Assert.AreEqual(expectedY, cell.Y,
            "A coordenada Y não está correta");
    }

    [TestMethod]
    public async Task When_Entity_Is_Moving_Update_Entity_Move(
    ) {
        Coordinate firstMove = new(0, 0);
        Coordinate secondMove = new(7, 0);
        Coordinate middle = new(3, 3);
        Coordinate expected = new(3, 1);
        IEntity entity = Utils.FakeEntity("entityOne");
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(entity, middle);
        battle.RegisterMove(entity.Id, firstMove);
        await battle.MoveEntities();
        battle.RegisterMove(entity.Id, secondMove);
        await battle.MoveEntities();
        Coordinate cell = battle
            .Board
            .GetEntityPosition(entity.Id);
        Assert.AreEqual(expected.X, cell.X,
            "A coordenada X não está correta");
        Assert.AreEqual(expected.Y, cell.Y,
            "A coordenada Y não está correta");
    }

    [TestMethod]
    public async Task Update_Entities_Moved_At_Propertie_When_Call_Move_Method()
    {
        var notifier = A.Fake<IEventsObserver>();
        IBattle battle = new DuelBuilder()
            .WithEventObserver(notifier)
            .Build();
        DateTime initialValue = battle.EntitiesMovedAt;
        await battle.MoveEntities();
        Assert.AreNotEqual(initialValue, battle.EntitiesMovedAt);
    }

    [TestMethod]
    public async Task Dont_Notify_The_Move_Event_If_Has_No_Movement_To_Exec() 
    {
        var notifier = A.Fake<IEventsObserver>();
        IBattle battle = new DuelBuilder()
            .WithEventObserver(notifier)
            .Build();
        await battle.MoveEntities();
        A.CallTo(() => notifier.Moved(An<Dictionary<string, Coordinate>>.Ignored))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task After_Move_Entities_Notify_Event() 
    {
        var notifier = A.Fake<IEventsObserver>();
        string entitiId = "someId";
        IBattle battle = new DuelBuilder()
            .WithEventObserver(notifier)
            .WithBoard(GameBoard.WithDefaultSize())
            .Build();
        battle.AddEntity(Utils.FakeEntity(entitiId));
        battle.RegisterMove(entitiId, new(3,3));
        await battle.MoveEntities();
        A.CallTo(() => notifier.Moved(An<Dictionary<string, Coordinate>>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    [DataRow(-1, 0)]
    [DataRow(0, -1)]
    [DataRow(-1, -1)]
    [DataRow(1, 0)]
    [DataRow(0, 1)]
    [DataRow(1, 1)]
    [DataRow(-1, 1)]
    [DataRow(1, -1)]
    public void Throws_Excepiton_When_Move_Entity_To_An_Invalid_Cell(
        double x, double y
    ) {
        const uint boardWidth = 1, boardHeight = 1;
        const string entityIdentifier = "entityOne";
        IEntity entity = Utils.FakeEntity(entityIdentifier);
        IBattle battle = new Duel(
            Guid.NewGuid(),
            new GameBoard(boardWidth, boardHeight),
            A.Fake<ICalculator>(),
            A.Fake<IEventsObserver>()
        );
        battle.AddEntity(entity);
        Coordinate invalidCell = new(x, y);
        Assert.ThrowsException<Exception>(() => 
            battle.RegisterMove(entity.Id, invalidCell)
        );
    }

    [TestMethod]
    public void Throws_Excepiton_When_Move_An_Entity_That_Is_Not_In_The_Board() {
        Coordinate moveTo = new(0, 0);
        const string entityIdentifier = "entityOne";
        IEntity entity = Utils.FakeEntity(entityIdentifier);
        IBattle battle = Utils.CreateDuel();
        Assert.ThrowsException<Exception>(() => 
            battle.RegisterMove(entity.Id, moveTo)
        );
    }

    [TestMethod]
    public void Return_True_When_Entity_Is_In_Battle() {
        IEntity entityInBattle = 
            Utils.FakeEntity("entityInBattleId");
        IBattle battle = Utils.CreateDuelWithEntities(entityInBattle);
        Assert.IsTrue(
            battle.EntityIsIntheBattle(entityInBattle.Id)
        );
    }

    [TestMethod]
    public void Return_False_When_Entity_Is_Not_In_Battle() {
        IEntity entityInBattle, entityNotInBattle; 
        entityInBattle = Utils.FakeEntity("entityInBattleId");
        entityNotInBattle = 
            Utils.FakeEntity("entityNotInBattleId");
        IBattle battle = Utils.CreateDuelWithEntities(entityInBattle);
        Assert.IsFalse(
            battle.EntityIsIntheBattle(entityNotInBattle.Id)
        );
    }

    [TestMethod]
    public async Task Recover_Entities_Mana() 
    {
        int manaBeforeRecover = 5;
        var firstEntity = Utils.FakeEntity("firstEntity");
        firstEntity.State.MaxMana = int.MaxValue;
        firstEntity.State.Mana = manaBeforeRecover;
        var secondEntity = Utils.FakeEntity("secondEntity");
        secondEntity.State.MaxMana = int.MaxValue;
        secondEntity.State.Mana = manaBeforeRecover;
        IBattle battle = Utils.CreateDuelWithEntities(firstEntity, secondEntity);
        await battle.RecoverMana();
        int expectedManaAfterRecover = manaBeforeRecover + 5;
        Assert.AreEqual(expectedManaAfterRecover, firstEntity.State.Mana);
        Assert.AreEqual(expectedManaAfterRecover, secondEntity.State.Mana);
    }

    [TestMethod]
    public async Task Dont_Let_Mana_Be_Greater_Than_The_Maximum() 
    {
        var firstEntity = Utils.FakeEntity("firstEntity");
        firstEntity.State.MaxMana = 100;
        firstEntity.State.Mana = firstEntity.State.MaxMana;
        var secondEntity = Utils.FakeEntity("secondEntity");
        secondEntity.State.MaxMana = 100;
        secondEntity.State.Mana = 99;
        IBattle battle = Utils.CreateDuelWithEntities(firstEntity, secondEntity);
        await battle.RecoverMana();
        Assert.AreEqual(firstEntity.State.MaxMana, firstEntity.State.Mana);
        Assert.AreEqual(secondEntity.State.MaxMana, secondEntity.State.Mana);
    }

    [TestMethod]
    public async Task After_Recover_Entities_Mana_Notify_Event() 
    {
        var notifier = A.Fake<IEventsObserver>();
        IBattle battle = new DuelBuilder()
            .WithEventObserver(notifier)
            .Build();
        await battle.RecoverMana();
        A.CallTo(() => notifier.ManaRecovered())
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task After_Recover_Entities_Mana_Fill_Field_Mana_Recovered_At() 
    {
        IBattle battle = new DuelBuilder()
            .Build();
        DateTime initialValue = battle.ManaRecoveredAt;
        await battle.RecoverMana();
        Assert.AreNotEqual(initialValue, battle.ManaRecoveredAt);
    }
}