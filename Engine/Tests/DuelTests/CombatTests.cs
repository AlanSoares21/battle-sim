using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine.Tests.DuelTests;

[TestClass]
public class CombatTests {
    [TestMethod]
    public void Successfull_Attack_When_Is_On_The_Side_Of_The_Target() 
    {
        IEntity attacker = Utils.FakeEntity("attacker");
        IEntity target = Utils.FakeEntity("target");
        IBattle battle = Utils.CreateDuelWithEntities();
        battle.AddEntity(attacker); 
        Coordinate targetPosition = new(4, 4);
        battle.AddEntity(target, targetPosition);

        for (int x = 3; x <= targetPosition.X + 1; x++)
        for (int y = 3; y <= targetPosition.Y + 1; y++)
        {
            Coordinate attackerPosition = new(x, y);
            battle.Board.Move(attacker.Id, attackerPosition);
            Assert.IsTrue(battle.Attack(target.Id, attacker.Id), 
                $"Attack failed for ({x}, {y})");
        }
    }
}