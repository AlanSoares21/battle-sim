
using System;

namespace BattleSimulator.Engine.Tests.EquipsTests;

[TestClass]
public class LineFunctionTests
{
    /*
        V - cria reta entre dois pontos
        V - trata retas verticais
        V - trata retas horizontais

        v - calcula corretamente a distancia para retas comuns
        v - calcula corretamente a distancia para retas horizontais
        v - calcula corretamente a distancia para retas verticais

        _ - calcula corretamente a distancia para retas comuns, mas nao retorna o valor absoluto
        _ - calcula corretamente a distancia para retas horizontais, mas nao retorna o valor absoluto
        _ - calcula corretamente a distancia para retas verticais, mas nao retorna o valor absoluto
    */
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
        Assert.AreEqual(-1, line.b);
        Assert.AreEqual(0, line.a);
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
    
}