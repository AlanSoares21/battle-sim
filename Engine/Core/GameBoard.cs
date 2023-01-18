using System;
using System.Collections.Generic;
using System.Linq;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine;

public class GameBoard : IBoard
{
    public static uint DEFAULT_BOARD_WIDTH = 8;
    public static uint DEFAULT_BOARD_HEIGHT = 8;
    public static GameBoard WithDefaultSize() => 
        new GameBoard(
            DEFAULT_BOARD_WIDTH,
            DEFAULT_BOARD_HEIGHT
        );
    
    Dictionary<string, Coordinate> EntitiesPosition;
    public GameBoard(uint boardWidth, uint boardHeight) {    
        Width = (int)boardWidth;
        Height = (int)boardHeight;
        EntitiesPosition = new();
    }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Coordinate GetEntityPosition (string identifier) => 
        EntitiesPosition[identifier];

    public List<string> GetEntities () => 
        EntitiesPosition
            .Keys
            .ToList();

    public Coordinate GetOppositeCoordinate(Coordinate coordinate) {
        if (!IsCoordinateValid(coordinate))
            throw new Exception("Can not get opposite coordinate from the invalid coordinate " + coordinate);
        int oppositeX = (coordinate.X + 1 - this.Width) * -1;
        int oppositeY = (coordinate.Y + 1 - this.Height) * -1;
        return new(oppositeX, oppositeY);
    }

    public void Place(string identifier, Coordinate coordinate)
    {
        if (!IsCoordinateValid(coordinate))
            throw new Exception($"Can not place entity {identifier} on coordinate the invalid coordinate {coordinate}");
        if (EntitiesPosition.ContainsKey(identifier))
            throw new Exception($"Entity {identifier} already is on the board");
        EntitiesPosition.Add(identifier, coordinate);
    }

    public void Move(string identifier, Coordinate coordinate)
    {
        if (!EntitiesPosition.ContainsKey(identifier))
            throw new Exception($"Entity {identifier} can not be moved because it is not in the board.");
        if (!IsCoordinateValid(coordinate))
            throw new Exception($"Can not move entity {identifier} to the invalid coordinate {coordinate}");
        EntitiesPosition[identifier] = coordinate;
    }

    public bool IsCoordinateValid(Coordinate coordinate) =>
        coordinate.X < Width && coordinate.X >= 0
        &&
        coordinate.Y < Height && coordinate.Y >= 0;
}