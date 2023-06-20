using System;

namespace BattleSimulator.Engine;

public struct Coordinate
{
    public Coordinate(double x, double y) {
        X = x;
        Y = y;
    }
    public double X { get; set; }

    public double Y { get; set; }

    public bool IsEqual(Coordinate coord) => 
        CoordinatesAreEqual(coord.X, coord.Y);
    public bool CoordinatesAreEqual(double x, double y) =>
        X == x && Y == y;

    public override string ToString()
    {
        return $"({X},{Y})";
    }

    public double Distance(Coordinate coord) {
        return Math.Sqrt(
            Math.Pow(this.X - coord.X, 2)
            +
            Math.Pow(this.Y - coord.Y, 2)
        );
    }
}