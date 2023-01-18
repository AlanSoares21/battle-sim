using Microsoft.VisualStudio.TestTools.UnitTesting;
using BattleSimulator.Engine.Tests.StubClasses;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Tests
{
    [TestClass]
    public class CalculatorTests
    {
        [TestMethod]
        [DataRow(100, 0.5, 50)]
        public void Reduce_Damage_By_Defense_Absorption_Percentage(int damage, double defenseAbsorption, int expected) {
            IOffensiveAttributes attackProps = new StubAttackProps();
            IDefensiveAttributes defenseProps = new StubDefenseProps();
            bool isAutoAttack = false;
            ICalculator battleCalcs = _CreateBattleCalculator();
            Assert.AreEqual(
                expected, 
                battleCalcs.Damage(
                    damage,
                    defenseAbsorption,
                    isAutoAttack, 
                    AttackTarget.MobToMob, 
                    attackProps, 
                    defenseProps)
                );
        }

        [TestMethod]
        [DataRow(1000, 0.25, 0.5, 750)]
        public void Calculate_Damage_With_Penetration(int damage, double penetration, double defenseAbsorption, int expected) {
            IOffensiveAttributes attackProps = new StubAttackProps() { Penetration = penetration };
            IDefensiveAttributes defenseProps = new StubDefenseProps();
            bool isAutoAttack = false;
            ICalculator battleCalcs = _CreateBattleCalculator();
            Assert.AreEqual(
                expected, 
                battleCalcs.Damage(
                    damage,
                    defenseAbsorption,
                    isAutoAttack, 
                    AttackTarget.MobToMob, 
                    attackProps, 
                    defenseProps)
                );
        }

        [TestMethod]
        [DataRow(1000, 0.50, 0.50, 1000)]
        [DataRow(1000, 0.51, 0.50, 1000)]
        public void Calculate_Damage_When_Penetration_Is_Greater_Or_Equal_To_Defense_Absorption
            (int damage, double penetration, double defenseAbsorption, int expected) 
        {
            IOffensiveAttributes attackProps = new StubAttackProps() { Penetration = penetration };
            IDefensiveAttributes defenseProps = new StubDefenseProps();
            bool isAutoAttack = false;
            ICalculator battleCalcs = _CreateBattleCalculator();
            Assert.AreEqual(
                expected, 
                battleCalcs.Damage(
                    damage,
                    defenseAbsorption,
                    isAutoAttack, 
                    AttackTarget.MobToMob, 
                    attackProps, 
                    defenseProps)
                );
        }

        ICalculator _CreateBattleCalculator() => new Calculator();
    }
}
