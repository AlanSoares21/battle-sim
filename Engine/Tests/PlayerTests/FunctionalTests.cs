using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Equipment.Armor;

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

    [TestMethod]
    [DataRow(0,0, 10,10, 1,1)]
    [DataRow(5,6, 0,-10, 5,5)]
    [DataRow(0,4, 10,0, 1,4)]
    public void Dont_Apply_Full_Damage_When_Hit_A_Barrier(
        int lifeX, int lifeY,
        int damageX, int damageY,
        int expectedX, int expectedY
    )
    {
        Coordinate damageToApply = new(damageX, damageY);
        Player player = new("player");
        player.State.CurrentHealth = new(lifeX, lifeY);
        player.AddEquip(GetBarrier());
        player.ApplyDamage(damageToApply);
        Assert.AreEqual(new(expectedX,expectedY), player.State.CurrentHealth);
    }

    IEquip GetBarrier()
    {
        return new CommomBarrierEquip(
            new Rectangle(new Coordinate[] {
                new(2, 2),
                new(2, 4),
                new(5, 4),
                new(5, 2)
            })
        );
    }
}