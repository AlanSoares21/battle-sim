using System;
using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Engine.Tests.EquipsTests;

[TestClass]
public class RectangleTests
{
    /*
        V - criar retangulo, passando os pontos
        V - calcula altura e base, 
        V - valida o tamanho de todos os lados do retangulo
        V - valida diagonal do retangulo
        calcula os coeficientes para as retas dos lados do retangulo

        verifica se um ponto esta dentro de sua area
        diz qual seu efeito 
    */
    [TestMethod]
    public void Throw_Exception_When_Try_Create_A_Rectangle_With_More_Than_4_Coordinates() 
    {
        Coordinate point1 = new(1,1), point2 = new(1,3),
        point3 = new(4,3), point4 = new(4,1), point5 = new(4,0);

        Assert.ThrowsException<Exception>(() => {
            Rectangle rectangle = new(new[] { point1, point2, point3, point4, point5 });
        }, "When creating rectangle with more than 4 coordinates, an exception should be throwed.");
    }

    [TestMethod]
    public void Calculate_Size_For_A_New_Rectangle() 
    {
        Coordinate pointA = new(1,1), pointB = new(1,3),
        pointC = new(4,3), pointD = new(4,1);

        double expectedHeight = 2;
        double expectedBase = 3;

        Rectangle rectangle = new(new[] {pointA, pointB, pointC, pointD});
        
        Assert.AreEqual(expectedHeight, rectangle.Height);
        Assert.AreEqual(expectedBase, rectangle.Base);
    }

    [TestMethod]
    [DataRow(new int[] {1, 1}, new int[] {1, 3}, new int[] {4, 3}, new int[] {5, 1})]
    [DataRow(new int[] {1, 1}, new int[] {0, 3}, new int[] {5, 3}, new int[] {4, 1})]
    [DataRow(new int[] {1, 1}, new int[] {1, 3}, new int[] {4, 1}, new int[] {4, 3})]
    // valid rectangle -> [DataRow(new int[] {1, 1}, new int[] {1, 3}, new int[] {4, 3}, new int[] {4, 1})]
    public void Throw_Exception_When_Try_Create_A_Rectangle_With_Invalid_Sizes(
        int[] coordinate1, int[] coordinate2, int[] coordinate3, int[] coordinate4
    ) 
    {
        Coordinate point1 = new(coordinate1[0], coordinate1[1]), 
        point2 = new(coordinate2[0], coordinate2[1]),
        point3 = new(coordinate3[0], coordinate3[1]), 
        point4 = new(coordinate4[0], coordinate4[1]);

        Assert.ThrowsException<Exception>(() => {
            Rectangle rectangle = new(new[] { point1, point2, point3, point4 });
        }, $"When creating rectangle with invalid size, an exception should be throwed. points: {point1} {point2} {point3} {point4}");
    }
}