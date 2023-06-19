using System;

namespace BattleSimulator.Engine.Equipment;

public interface IEquip {
    bool IsInner(Coordinate coord);
}

public class Rectangle : IEquip
{
    public double Height { get; private set; } = 0;
    public double Base { get; private set; } = 0;
    public Coordinate[] Coordinates = new Coordinate[4];

    public Rectangle(Coordinate[] coordinates) 
    {
        if (coordinates.Length > 4)
            throw new Exception("Tried to create a rectangle with more than 4 coordinates");
        setCoordinates(coordinates);
    }
    void setCoordinates(Coordinate[] points) 
    {
        Coordinates = points;
        double distancePoint1And2 = distance(points[0], points[1]);
        double distancePoint1And4 = distance(points[0], points[3]);
        bool secondPointDefineBase = distancePoint1And2 > distancePoint1And4;
        if (secondPointDefineBase) 
        {
            Base = distancePoint1And2;
            Height = distancePoint1And4;
        }
        else 
        {
            Base = distancePoint1And4;
            Height = distancePoint1And2;
        }
        double diameter = distance(points[0], points[2]);

        if (diameter <= Base)
            throw new Exception($"Diameter({diameter}) is equal or lower than the base({Base}) of the rectangle {points[0]} {points[1]} {points[2]} {points[3]} ");

        double distancePoint3And4 = distance(points[2], points[3]);
        if (distancePoint1And2 != distancePoint3And4)
            throw new Exception($"Distance between {points[0]} and {points[1]}(value: {distancePoint1And2}) is not equal to distance between {points[2]} and {points[3]}(value: {distancePoint3And4})");
        
        double distancePoint3And2 = distance(points[2], points[1]);
        if (distancePoint1And4 != distancePoint3And2)
            throw new Exception($"Distance between {points[0]} and {points[3]}(value: {distancePoint1And4}) is not equal to distance between {points[2]} and {points[1]}(value: {distancePoint3And2})");
    }
    double size = 0;
    (float a, float b)[] funcComponents = new (float a, float b)[4];

    (float a, float b) getFunc(Coordinate coord1, Coordinate coord2) {
        float b = (coord2.Y - coord1.Y) / (coord2.X - coord1.X);
        float a = coord1.Y - coord1.X * b;
        return (a, b);
    }

    double distance(Coordinate coord1, Coordinate coord2) {
        return Math.Sqrt(
            Math.Pow(coord1.X - coord2.X, 2)
            +
            Math.Pow(coord1.Y - coord2.Y, 2)
        );
    }

    public  bool IsInner(Coordinate coord)
    {
        for (int i = 0; i < funcComponents.Length; i++)
        {
            if (distancia(coord, i) > size)
                return false;
        }
        return true;
    }

    double distancia(Coordinate coordinate, int funcIndex) {
        var func = funcComponents[funcIndex];
        return 
            Math.Abs(func.b *  coordinate.X + 1 * coordinate.Y + func.a) 
            / 
            Math.Sqrt(Math.Pow(func.b, 2)+1);
    }
}