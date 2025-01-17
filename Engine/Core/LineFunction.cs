using System;

namespace BattleSimulator.Engine;

/*
    f(x) = ax + c
    0 = ax + by + c
*/
public struct LineFunction {
    public bool vertical;
    public double x;
    public bool horizontal;
    
    public double a;
    public double b;
    public double c;
    double distanceConstant;

    public LineFunction(Coordinate coord1, Coordinate coord2) 
    {
        if (coord1.Y == coord2.Y)
        {
            this.horizontal = true;
            this.c = coord1.Y;
        }
        else if (coord1.X == coord2.X) 
        {
            this.vertical = true;
            this.x = coord1.X;
        }
        else 
        {
            this.a = (coord2.Y - coord1.Y) / (coord2.X - coord1.X);
            this.c = coord1.Y - coord1.X * this.a;
            this.b = (-(coord1.X * this.a) - this.c) / coord1.Y;
            this.distanceConstant = Math.Sqrt(Math.Pow(this.a, 2) + Math.Pow(this.b, 2));
        }
    }

    public double Distance(Coordinate coordinate) 
    {   
        return Math.Abs(DistanceNotAbs(coordinate));
    }

    public double DistanceNotAbs(Coordinate coordinate) 
    {   
        if (this.horizontal)
            return coordinate.Y - this.c;
        if (this.vertical)
            return coordinate.X - this.x;
        return (this.a *  coordinate.X + this.b * coordinate.Y + this.c) 
            / 
            this.distanceConstant;
    }

    public LineFunction LineMoreCloseToOrigin(double distance) 
    {
        if (this.horizontal) 
        {
            double newY;
            if (this.c > 0)
                newY = this.c - distance;
            else
                newY = this.c + distance;
            return new LineFunction(new(0, newY), new(1, newY));
        }
        if (this.vertical) 
        {
            double newX;
            if (this.x > 0)
                newX = this.x - distance;
            else
                newX = this.x + distance;
            return new LineFunction(new(newX, 0), new(newX, 1));
        }
        LineFunction newLine = new(new (0, this.GetY(0)), new (1, this.GetY(1)));
        if (this.c > 0)
            newLine.c -= distance * this.distanceConstant;
        else
            newLine.c += distance * this.distanceConstant;
        return newLine;
    }

    public bool Parallel(LineFunction line) {
        return (this.horizontal && line.horizontal) ||
            (this.vertical && line.vertical) ||
            this.a == line.a;
    }

    public double GetY(double x) => this.a * x + this.c;

    public override string ToString()
    {
        if (horizontal)
            return $"Function (horizontal, c = {c})";
        if (vertical)
            return $"Function (vertical, x = {x})";
        return $"Function f(x) = {a}x + {c}; b: {b}";
    }
}
