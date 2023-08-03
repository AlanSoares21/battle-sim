using System;
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
    [DataRow(MoveDirection.Up, 3, 4)]
    [DataRow(MoveDirection.Right, 4, 3)]
    [DataRow(MoveDirection.Down, 3, 2)]
    [DataRow(MoveDirection.Left, 2, 3)]
    public void Move_Entity_From_Middle_Cell_To_Coordinate(
        MoveDirection direction, int expectedX, int expectedY
    ) {
        Coordinate middle = new(3, 3);
        string entityIdentifier = "entityOne";
        IEntity entity = Utils.FakeEntity(entityIdentifier);
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(entity, middle);
        battle.Move(entity, direction);
        Coordinate cell = battle
            .Board
            .GetEntityPosition(entity.Id);
        Assert.AreEqual(expectedX, cell.X,
            "A coordenada X não está correta");
        Assert.AreEqual(expectedY, cell.Y,
            "A coordenada Y não está correta");
    }

    [TestMethod]
    [DataRow(MoveDirection.Up)]
    [DataRow(MoveDirection.Right)]
    [DataRow(MoveDirection.Down)]
    [DataRow(MoveDirection.Left)]
    public void Throws_Excepiton_When_Move_Entity_To_An_Invalid_Cell(
        MoveDirection direction
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
        Assert.ThrowsException<Exception>(() => 
            battle.Move(entity, direction)
        );
    }

    [TestMethod]
    public void Throws_Excepiton_When_Move_An_Entity_That_Is_Not_In_The_Board() {
        const MoveDirection direction = MoveDirection.Up;
        const string entityIdentifier = "entityOne";
        IEntity entity = Utils.FakeEntity(entityIdentifier);
        IBattle battle = Utils.CreateDuel();
        Assert.ThrowsException<Exception>(() => 
            battle.Move(entity, direction)
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
        firstEntity.State.Mana = manaBeforeRecover;
        var secondEntity = Utils.FakeEntity("secondEntity");
        secondEntity.State.Mana = manaBeforeRecover;
        IBattle battle = Utils.CreateDuelWithEntities(firstEntity, secondEntity);
        await battle.RecoverMana();
        int expectedManaAfterRecover = manaBeforeRecover + 5;
        Assert.AreEqual(expectedManaAfterRecover, firstEntity.State.Mana);
        Assert.AreEqual(expectedManaAfterRecover, secondEntity.State.Mana);
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