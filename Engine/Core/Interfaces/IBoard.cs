using System.Collections.Generic;

namespace BattleSimulator.Engine.Interfaces;

public interface IBoard {
    int Width { get; }
    int Height { get; }
    bool IsCoordinateValid(Coordinate coordinate);
    Coordinate GetEntityPosition(string identifier);
    List<string> GetEntities();
    Coordinate GetOppositeCoordinate(Coordinate coordinate);
    void Place(string identifier, Coordinate coordinate);
    void Move(string identifier, Coordinate coordinate);
}