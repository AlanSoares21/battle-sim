using System;

namespace BattleSimulator.Engine.Equipment;

public class Rectangle : IEquipFormat
{
    public double Height { get; private set; }
    public double HalfHeight { get; private set; }
    public double Base { get; private set; }
    public double HalfBase { get; private set; }
    public Coordinate[] Coordinates = new Coordinate[4];
    LineFunction BaseLine, HeightLine;

    public Rectangle(Coordinate[] coordinates) 
    {
        if (coordinates.Length > 4)
            throw new Exception("Tried to create a rectangle with more than 4 coordinates");
        setCoordinates(coordinates);
    }
    void setCoordinates(Coordinate[] points) 
    {
        Coordinates = points;
        double distancePoint1And2 = points[0].Distance(points[1]);
        double distancePoint1And4 = points[0].Distance(points[3]);
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
        HalfBase = Base/2;
        HalfHeight = Height/2;

        double diameter = points[0].Distance(points[2]);
        if (diameter <= Base)
            throw new Exception($"Diameter({diameter}) is equal or lower than the base({Base}) of the rectangle {points[0]} {points[1]} {points[2]} {points[3]} ");

        double distancePoint3And4 = points[2].Distance(points[3]);
        if (distancePoint1And2 != distancePoint3And4)
            throw new Exception($"Distance between {points[0]} and {points[1]}(value: {distancePoint1And2}) is not equal to distance between {points[2]} and {points[3]}(value: {distancePoint3And4})");
        
        double distancePoint3And2 = points[2].Distance(points[1]);
        if (distancePoint1And4 != distancePoint3And2)
            throw new Exception($"Distance between {points[0]} and {points[3]}(value: {distancePoint1And4}) is not equal to distance between {points[2]} and {points[1]}(value: {distancePoint3And2})");

        getSideFunctionsCoefficients(secondPointDefineBase);
    }

    void getSideFunctionsCoefficients(bool secondPointDefineBase) 
    {
        int baseStart = 0;
        int heightStart = 1;
        if (!secondPointDefineBase) 
        {
            baseStart = 1;
            heightStart = 0;
        }
        LineFunction baseLine = GetLineMoreFurtherFromOrigin(baseStart);
        LineFunction heightLine = GetLineMoreFurtherFromOrigin(heightStart);
        
        BaseLine = baseLine.LineMoreCloseToOrigin(this.HalfHeight);
        HeightLine = heightLine.LineMoreCloseToOrigin(this.HalfBase);
    }

    LineFunction GetLineMoreFurtherFromOrigin(int start)
    {
        Coordinate origin = new(0, 0);

        LineFunction firstLine = new LineFunction(Coordinates[start], Coordinates[start + 1]); 
        
        int next = start + 3;
        if (next == Coordinates.Length) 
            next = 0;
        LineFunction secondLine = new LineFunction(Coordinates[start + 2], Coordinates[next]); 

        if (firstLine.Distance(origin) > secondLine.Distance(origin))
            return firstLine;
        return secondLine;
    }

    public bool IsInner(Coordinate coord)
    {
        if (BaseLine.Distance(coord) > HalfHeight)
                return false;
        if (HeightLine.Distance(coord) > HalfBase)
            return false;
        return true;
    }    

    public Coordinate? Intersect(Coordinate start, Coordinate end)
    {
        double startEndDiffX = start.X - end.X;
        double startEndDiffY = start.Y - end.Y;
        for (int i = 0; i < 4; i++)
        {
            int next = i + 1;
            if (next == Coordinates.Length)
                next = 0;
            double nextCurrentDiffX = Coordinates[next].X - Coordinates[i].X;
            double nextCurrentDiffY = Coordinates[next].Y - Coordinates[i].Y;
            
            double startADiffX = start.X - Coordinates[i].X;
            double startADiffY = start.Y - Coordinates[i].Y;
            
            double determinante =  
                nextCurrentDiffX * startEndDiffY - startEndDiffX * nextCurrentDiffY;
            double determinanteAlfa =  
                startADiffX * startEndDiffY - startEndDiffX * startADiffY;
            double determinanteBeta = 
                nextCurrentDiffX * startADiffY - startADiffX * nextCurrentDiffY;
            
            var alfa = determinanteAlfa / determinante;
            var beta = determinanteBeta / determinante;

            if (1 >= alfa && alfa >= 0
                &&
                1 >= beta && beta >= 0)
            {
                return new Coordinate(
                    Coordinates[i].X + alfa * nextCurrentDiffX,
                    Coordinates[i].Y + alfa * nextCurrentDiffY
                );
            }
        }
        return null;
    }


}
