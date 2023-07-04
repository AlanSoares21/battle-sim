namespace BattleSimulator.Engine.Tests.PlayerTests;

[TestClass]
public class FunctionalTests
{
    [TestMethod]
    public void Apply_Damage()
    {
        Coordinate damageToApply = new(10, -10);
        Player player = new("player");
        player.ApplyDamage(damageToApply);
        Assert.AreEqual(damageToApply, player.State.CurrentHealth);
    }
}