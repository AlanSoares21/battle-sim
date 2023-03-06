using System;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine.Tests.DuelTests;

[TestClass]
public class FunctionalTests
{
    [TestMethod]
    public void Places_Entity_On_The_Board() {
        string entityIdentifier = "id";
        IEntity entity = FakeEntityWithIdentifier(entityIdentifier);
        IBattle battle = CreateDuel();
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
        firstEntity = FakeEntityWithIdentifier(firstEntityIdentifier);
        secondEntity = FakeEntityWithIdentifier(secondEntityIdentifier);
        IBattle battle = CreateDuel();
        battle.AddEntity(firstEntity);
        battle.AddEntity(secondEntity);
        Coordinate firstEntityCell = battle
            .Board
            .GetEntityPosition(firstEntity.Identifier);
        Coordinate secondEntityCell = battle
            .Board
            .GetEntityPosition(secondEntity.Identifier);
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
        firstEntity = FakeEntityWithIdentifier(firstEntityIdentifier);
        secondEntity = FakeEntityWithIdentifier(secondEntityIdentifier);
        thirdEntity = FakeEntityWithIdentifier(thirdEntityIdentifier);
        IBattle battle = CreateDuelWithEntities(
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
        IEntity entity = FakeEntityWithIdentifier(entityIdentifier);
        IBattle battle = CreateDuel();
        battle.AddEntity(entity, middle);
        battle.Move(entity, direction);
        Coordinate cell = battle
            .Board
            .GetEntityPosition(entity.Identifier);
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
        IEntity entity = FakeEntityWithIdentifier(entityIdentifier);
        IBattle battle = new Duel(
            Guid.NewGuid(),
            new GameBoard(boardWidth, boardHeight)
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
        IEntity entity = FakeEntityWithIdentifier(entityIdentifier);
        IBattle battle = CreateDuel();
        Assert.ThrowsException<Exception>(() => 
            battle.Move(entity, direction)
        );
    }

    [TestMethod]
    public void Return_True_When_Entity_Is_In_Battle() {
        IEntity entityInBattle = 
            FakeEntityWithIdentifier("entityInBattleId");
        IBattle battle = CreateDuelWithEntities(entityInBattle);
        Assert.IsTrue(
            battle.EntityIsIntheBattle(entityInBattle.Identifier)
        );
    }

    [TestMethod]
    public void Return_False_When_Entity_Is_Not_In_Battle() {
        IEntity entityInBattle, entityNotInBattle; 
        entityInBattle = FakeEntityWithIdentifier("entityInBattleId");
        entityNotInBattle = 
            FakeEntityWithIdentifier("entityNotInBattleId");
        IBattle battle = CreateDuelWithEntities(entityInBattle);
        Assert.IsFalse(
            battle.EntityIsIntheBattle(entityNotInBattle.Identifier)
        );
    }

    IEntity FakeEntityWithIdentifier(string identifier) {
        IEntity entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Identifier).Returns(identifier);
        return entity;
    }

    IBattle CreateDuelWithEntities(params IEntity[] entities) 
    {
        IBattle duel = CreateDuel();
        foreach (var entity in entities)
            duel.AddEntity(entity);
        return duel;
    }

    IBattle CreateDuel() => 
        new Duel(Guid.NewGuid(), GameBoard.WithDefaultSize());
}