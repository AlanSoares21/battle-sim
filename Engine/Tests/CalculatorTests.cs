using Microsoft.VisualStudio.TestTools.UnitTesting;
using BattleSimulator.Engine.Tests.StubClasses;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Tests;

[TestClass]
public class CalculatorTests
{
    [TestMethod]
    [DataRow(100, 0.5, 50)]
    public void Reduce_Damage_By_Defense_Absorption_Percentage(
        int damage, 
        double defenseAbsorption, 
        int expected) 
    {
        IOffensiveAttributes attackProps = new StubAttackProps();
        IDefensiveAttributes defenseProps = new StubDefenseProps();
        ICalculator battleCalcs = _CreateBattleCalculator();
        Assert.AreEqual(
            expected, 
            battleCalcs.Damage(
                damage,
                defenseAbsorption,
                attackProps, 
                defenseProps)
            );
    }
    
    ICalculator _CreateBattleCalculator() => new Calculator();
}