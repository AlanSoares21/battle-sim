
using System;

namespace BattleSimulator.Engine.Tests.EquipsTests;

[TestClass]
public class LineFunctionTests
{
    [TestMethod]
    public void Create_Line_With_Two_Coordinates()
    {
        Coordinate firstCoordinate = new(0,1);
        Coordinate secondCoordinate = new(1,3);
        LineFunction line = new(firstCoordinate, secondCoordinate);
        Assert.AreEqual(2, line.a);
        Assert.AreEqual(1, line.c);
        Assert.AreEqual(-1, line.b);
        Assert.IsFalse(line.horizontal);
        Assert.IsFalse(line.vertical);
    }

    [TestMethod]
    public void Create_Vertical_Line_With_Two_Coordinates()
    {
        Coordinate firstCoordinate = new(1, 1);
        Coordinate secondCoordinate = new(firstCoordinate.X, 3);
        LineFunction line = new(firstCoordinate, secondCoordinate);
        Assert.IsTrue(line.vertical);
        Assert.AreEqual(firstCoordinate.X, line.x);
    }

    [TestMethod]
    public void Create_Horizontal_Line_With_Two_Coordinates()
    {
        Coordinate firstCoordinate = new(1,3);
        Coordinate secondCoordinate = new(2,firstCoordinate.Y);
        LineFunction line = new(firstCoordinate, secondCoordinate);
        Assert.IsTrue(line.horizontal);
        Assert.AreEqual(firstCoordinate.Y, line.c);
    }

    
    [TestMethod]
    [DataRow(0,1, 0, 0)]
    [DataRow(3,0, 3.130495168, 0.000000001)]
    [DataRow(2,6, 0.447213595, 0.000000001)]
    public void Calculate_Distance(int x, int y, double expected, double delta)
    {
        LineFunction line = new(new(0,1), new(1,3));
        Assert.AreEqual(expected, line.Distance(new(x, y)), delta);
    }

    [TestMethod]
    [DataRow(2,2, 0)]
    [DataRow(4,1, 2)]
    [DataRow(-1,1, 3)]
    public void Calculate_Distance_From_A_Vertical_Line(int x, int y, double expected)
    {
        LineFunction line = new(new(2,1), new(2,3));
        Assert.AreEqual(expected, line.Distance(new(x, y)));
    }

    [TestMethod]
    [DataRow(2,2, 0)]
    [DataRow(1,4, 2)]
    [DataRow(1,-1, 3)]
    public void Calculate_Distance_From_A_Horizontal_Line(int x, int y, double expected)
    {
        LineFunction line = new(new(1,2), new(3,2));
        Assert.AreEqual(expected, line.Distance(new(x, y)));
    }
    
    [TestMethod]
    [DataRow(3,0, 3.130495168, 0.000000001)]
    [DataRow(2,6, -0.447213595, 0.000000001)]
    public void Calculate_Not_Absolute_Distance(int x, int y, double expected, double delta)
    {
        LineFunction line = new(new(0,1), new(1,3));
        Assert.AreEqual(expected, line.DistanceNotAbs(new(x, y)), delta);
    }

    [TestMethod]
    [DataRow(4,1, 2)]
    [DataRow(-1,1, -3)]
    public void Calculate_Not_Absolute_Distance_From_A_Vertical_Line(
        int x, int y, double expected)
    {
        LineFunction line = new(new(2,1), new(2,3));
        Assert.AreEqual(expected, line.DistanceNotAbs(new(x, y)));
    }

    [TestMethod]
    [DataRow(1,4, 2)]
    [DataRow(1,-1, -3)]
    public void Calculate_Not_Absolute_Distance_From_A_Horizontal_Line(
        int x, int y, double expected)
    {
        LineFunction line = new(new(1,2), new(3,2));
        Assert.AreEqual(expected, line.DistanceNotAbs(new(x, y)));
    }


    [TestMethod]
    [DataRow(-5,-6, -6,-5, 3, -6.757359312880714)]
    [DataRow(-5,6, -6,5, 3, 6.757359312880714)]
    [DataRow(5,6, 6,5, 3, 6.757359312880714)]
    [DataRow(5,-6, 6,-5, 3, -6.757359312880714)]
    public void Create_Line_More_Close_To_Origin(
        int x1, int y1, 
        int x2, int y2, 
        double distance, 
        double cExpected)
    {
        LineFunction line = new(new(x1,y1), new(x2,y2));
        var farLine = line.LineMoreCloseToOrigin(distance);
        Assert.AreEqual(cExpected, farLine.c);
    }
}