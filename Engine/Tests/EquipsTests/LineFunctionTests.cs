
namespace BattleSimulator.Engine.Tests.EquipsTests;

[TestClass]
public class LineFunctionTests
{
    /*
        _ - cria reta entre dois pontos
        _ - trata retas verticais
        _ - trata retas horizontais

        _ - calcula corretamente a distancia para retas comuns
        _ - calcula corretamente a distancia para retas horizontais
        _ - calcula corretamente a distancia para retas verticais

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

    /*
    [TestMethod]
    [DataRow(0,1, 4/2)]
    public void Distance()
    {

    }
    */
}