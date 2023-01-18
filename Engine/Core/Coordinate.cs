using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine;

public struct Coordinate
{
    public Coordinate(int x, int y) {
        X = x;
        Y = y;
    }
    public int X { get; set; }

    public int Y { get; set; }

    public bool IsEqual(Coordinate coord) => 
        CoordinatesAreEqual(coord.X, coord.Y);
    public bool CoordinatesAreEqual(int x, int y) =>
        X == x && Y == y;

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}