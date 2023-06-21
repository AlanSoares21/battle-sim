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
    public double y;
    
    public double a;
    public double b;
    public double c;

    public LineFunction(Coordinate coord1, Coordinate coord2) 
    {
        if (coord1.Y == coord2.Y)
        {
            this.horizontal = true;
            this.y = coord1.Y;
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
        }
    }

    public double Distance(Coordinate coordinate) 
    {   
        if (this.horizontal)
            return Math.Abs(coordinate.Y - this.y);
        if (this.vertical)
            return Math.Abs(coordinate.X - this.x);
        return 
            Math.Abs(this.b *  coordinate.X + 1 * coordinate.Y + this.a) 
            / 
            Math.Sqrt(Math.Pow(this.b, 2)+1);
    }

    public LineFunction NewFarLine(double distance) 
    {
        if (this.horizontal) 
        {
            double newY = this.y + distance;
            return new LineFunction(new(0, newY), new(1, newY));
        }
        if (this.vertical) 
        {
            double newX = this.x + distance;
            return new LineFunction(new(newX, 0), new(newX, 1));
        }
        LineFunction middle = new(new (0, this.GetY(0)), new (1, this.GetY(1)));
        middle.b += distance;
        return middle;
    }

    public bool Parallel(LineFunction line) {
        return (this.horizontal && line.horizontal) ||
            (this.vertical && line.vertical) ||
            this.a == line.a;
    }

    public double GetY(double x) => this.a * x + this.b;

    public override string ToString()
    {
        if (horizontal)
            return $"Function (horizontal, y = {y})";
        if (vertical)
            return $"Function (vertical, x = {x})";
        return $"Function (a: {this.a}; b: {this.b})";
    }
}
