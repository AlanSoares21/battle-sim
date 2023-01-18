using System;
using BattleSimulator.Engine.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BattleSimulator.Engine.Tests;

[TestClass]
public class GameBoardTests {

    [TestMethod]
    public void Create_Default_Board() {
        const int heigthExpected = 8;
        const int widthExpected = 8;
        IBoard board = CreateBoard();
        Assert.AreEqual(heigthExpected, board.Height);
        Assert.AreEqual(widthExpected, board.Width);
    }

    [TestMethod]
    public void Return_True_To_A_Valid_Coordinate() {
        Coordinate validCoordinate = new(0, 0);
        var board = new GameBoard(1, 1);
        Assert.IsTrue(board.IsCoordinateValid(validCoordinate));
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(-1, -1)]
    [DataRow(1, 0)]
    [DataRow(0, 1)]
    public void Return_False_When_Validate_A_Coordinate_Out_Of_Board_Limit(
        int invalidX, int invalidY
    ) {
        var board = new GameBoard(1, 1);
        Coordinate invalidCoordinate = new(invalidX, invalidY);
        Assert.IsFalse(board.IsCoordinateValid(invalidCoordinate));
    }

    [TestMethod]
    [DataRow(0, 0, 7, 7)]
    [DataRow(7, 7, 0, 0)]
    [DataRow(7, 0, 0, 7)]
    [DataRow(0, 7, 7, 0)]
    [DataRow(3, 3, 4, 4)]
    public void Get_Correct_Opposite_Coordinate_When_The_Size_Is_Even(
        int cellX, int cellY, 
        int oppositeX, int oppositeY
    ) {
        uint boardWidth = 8, boardHeight = 8;
        IBoard board = new GameBoard(boardWidth, boardHeight);
        Coordinate oppositeCoordinate = board
            .GetOppositeCoordinate(new(cellX, cellY));
        Assert.AreEqual(oppositeX, oppositeCoordinate.X);
        Assert.AreEqual(oppositeY, oppositeCoordinate.Y);
    }

    [TestMethod]
    [DataRow(0, 0, 8, 8)]
    [DataRow(8, 8, 0, 0)]
    [DataRow(0, 8, 8, 0)]
    [DataRow(8, 0, 0, 8)]
    [DataRow(3, 3, 5, 5)]
    [DataRow(4, 4, 4, 4)]
    public void Get_Correct_Opposite_Coordinate_When_The_Size_Is_Odd(
        int cellX, int cellY, 
        int oppositeX, int oppositeY
    ) {
        uint boardWidth = 9, boardHeight = 9;
        IBoard board = new GameBoard(boardWidth, boardHeight);
        Coordinate oppositeCoordinate = board
            .GetOppositeCoordinate(new(cellX, cellY));
        Assert.AreEqual(oppositeX, oppositeCoordinate.X);
        Assert.AreEqual(oppositeY, oppositeCoordinate.Y);
    }
    
    [TestMethod]
    public void Throws_Exception_When_Try_Get_Opposite_Coordinate_For_A_OffIndex_Coordinates() {
        Coordinate invalidCoordinate = new(8, 8);
        uint boardWidth = 8, boardHeight = 8;
        IBoard board = new GameBoard(boardWidth, boardHeight);
        Assert.ThrowsException<Exception>(() => 
            board.GetOppositeCoordinate(invalidCoordinate)
        );
    }

    [TestMethod]
    public void Places_Entity_On_Coordinate() {
        string identifier = "entityOne";
        Coordinate cell = new(3, 4);
        IBoard board = CreateBoard();
        board.Place(identifier, cell);
        Coordinate cellWithEntity = board.GetEntityPosition(identifier);
        Assert.IsTrue(cellWithEntity.IsEqual(cell),
            "A entidade não foi posicionada na célula correta");
    }

    [TestMethod]
    public void Throws_Exception_When_Try_Place_Entity_On_An_Invalid_Coordinate() {
        string identifier = "entityOne";
        IBoard board = new GameBoard(1, 1);
        Coordinate invalidCell = new(1, 1);
        Assert.ThrowsException<Exception>(
            () => board.Place(identifier, invalidCell),
            "A entidade foi posicionada em uma célula inválida");
    }

    
    [TestMethod]
    public void Get_Coordinate_With_Entity() {
        string entityIdentifier = "entityOne";
        IBoard board = CreateBoard();
        Coordinate firstCoordinate = new(0, 0);
        board.Place(entityIdentifier, firstCoordinate);
        Coordinate cellWithEntity = board
            .GetEntityPosition(entityIdentifier);
        Assert.IsTrue(firstCoordinate.IsEqual(cellWithEntity));
    }

    [TestMethod]
    public void List_All_Entities_Placed_In_The_Board() {
        string identifierOne = "entityOne";
        string identifierTwo = "entityTwo";
        IBoard board = CreateBoard();
        board.Place(identifierOne, new(0, 0));
        board.Place(identifierTwo, new(1, 1));
        var entities = board.GetEntities();
        Assert.IsTrue(entities.Contains(identifierOne));
        Assert.IsTrue(entities.Contains(identifierTwo));
    }

    [TestMethod]
    public void Throw_Exception_When_Try_Place_A_Duplicated_Entity() {
        string identifierOne = "entityOne";
        string identifierDuplicated = identifierOne;
        IBoard board = CreateBoard();
        board.Place(identifierOne, new(0, 0));
        Assert.ThrowsException<Exception>(
            () => board.Place(identifierDuplicated, new(0, 0)),
            "Entidades duplicadas estão sendo posicionadas no tabuleiro"
        );
    }

    [TestMethod]
    [DataRow(3, 4)]
    [DataRow(4, 3)]
    [DataRow(3, 2)]
    [DataRow(2, 3)]
    public void Move_Entity_From_Middle_Cell_To_Coordinate(
        int expectedX, int expectedY) 
    {
        Coordinate middle = new(3, 3);
        string identifier = "entityOne";
        IBoard board = CreateBoard();
        board.Place(identifier, middle);
        board.Move(identifier, new(expectedX, expectedY));
        Coordinate cell = board.GetEntityPosition(identifier);
        Assert.AreEqual(
            expectedX, 
            cell.X,
            "A coordenada X não está correta");
        Assert.AreEqual(
            expectedY, 
            cell.Y,
            "A coordenada Y não está correta");
    }

    [TestMethod]
    public void Throws_Exception_When_Try_Move_An_Entity_Not_Placed_In_The_Board() 
    {
        string identifier = "entityOutOfTheBoard";
        IBoard board = CreateBoard();
        Assert.ThrowsException<Exception>(
            () => board.Move(identifier, new(0, 0)),
            "A entidade foi movida apesar de não ter sido posicionada");
    }

    IBoard CreateBoard() => GameBoard.WithDefaultSize();
    
    [TestMethod]
    public void Throws_Exception_When_Try_Move_An_Entity_To_An_Invalid_Coordinate() 
    {
        string identifier = "entityOutOfTheBoard";
        IBoard board = new GameBoard(1, 1);
        board.Place(identifier, new(0, 0));
        Assert.ThrowsException<Exception>(
            () => board.Move(identifier, new(1, 1)),
            "A entidade foi movida para uma célula inválida");
    }

}