using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine.Tests.DuelTests;

[TestClass]
public class CombatTests {
    [TestMethod]
    public void Can_Attack_When_Is_On_The_Side_Of_The_Target() 
    {
        IEntity attacker = Utils.FakeEntity("attacker");
        IEntity target = Utils.FakeEntity("target");
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(attacker); 
        Coordinate targetPosition = new(4, 4);
        battle.AddEntity(target, targetPosition);

        for (int x = 3; x <= targetPosition.X + 1; x++)
        for (int y = 3; y <= targetPosition.Y + 1; y++)
        {
            Coordinate attackerPosition = new(x, y);
            battle.Board.Move(attacker.Id, attackerPosition);
            Assert.IsTrue(battle.CanAttack(target.Id, attacker.Id), 
                $"Attack failed for ({x}, {y})");
        }
    }

    [TestMethod]
    public void Can_Not_Attack_When_One_Square_Away_In_X_From_The_Target() 
    {
        IEntity attacker = Utils.FakeEntity("attacker");
        IEntity target = Utils.FakeEntity("target");
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(attacker); 
        Coordinate targetPosition = new(4, 4);
        battle.AddEntity(target, targetPosition);

        for (int x = 2; x <= targetPosition.X + 2; x += targetPosition.X)
        for (int y = 2; y <= targetPosition.Y + 2; y++)
        {
            Coordinate attackerPosition = new(x, y);
            battle.Board.Move(attacker.Id, attackerPosition);
            Assert.IsFalse(battle.CanAttack(target.Id, attacker.Id), 
                $"Success attack from ({x}, {y})");
        }
    }

    /*
        Obs: in the loop of this test, four tests are repeted from the test
        Attack_Fails_When_One_Square_Away_In_X_From_The_Target
        (the vertices tests)
    */
    [TestMethod]
    public void Can_Not_Attack_When_One_Square_Away_In_Y_From_The_Target() 
    {
        IEntity attacker = Utils.FakeEntity("attacker");
        IEntity target = Utils.FakeEntity("target");
        IBattle battle = Utils.CreateDuel();
        battle.AddEntity(attacker); 
        Coordinate targetPosition = new(4, 4);
        battle.AddEntity(target, targetPosition);

        for (int y = 2; y <= targetPosition.Y + 2; y += targetPosition.Y)
        for (int x = 2; x <= targetPosition.X + 2; x++)
        {
            Coordinate attackerPosition = new(x, y);
            battle.Board.Move(attacker.Id, attackerPosition);
            Assert.IsFalse(battle.CanAttack(target.Id, attacker.Id), 
                $"Success attack from ({x}, {y})");
        }
    }

    [TestMethod]
    public void Dealling_Damage() {
        int damage = 10;
        IEntity attacker = Utils.FakeEntity("attacker");
        DefineDamage(attacker, damage);
        attacker.Weapon = new() { damageOnX = Equipment.DamageDirection.Negative };
        IEntity target = Utils.FakeEntity("target");
        int health = 50;
        DefineHealth(target, health);
        IBattle battle = Utils.CreateDuel();
        
        battle.AddEntity(attacker, new(0, 0));
        battle.AddEntity(target, new(1, 1));
        
        battle.Attack(target.Id, attacker.Id);
        Coordinate expectedHealthAfterAttack = new(health - damage, health);
        
        Assert.AreEqual(expectedHealthAfterAttack, target.State.CurrentHealth);
    }

    void DefineDamage(IEntity entity, int value) {
        entity.OffensiveStats.Damage = value;
    }

    void DefineHealth(IEntity entity, int value) {
        entity.State.HealthRadius = value;
        entity.State.CurrentHealth = new(value, value);
    }
}