using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class AttacksRequestedListTest
{
    [TestMethod]
    public void Register_Attack() 
    {
        string source = "source";
        string target = "target";
        var attacksRequested = new AttacksRequestedList();
        Assert.IsTrue(attacksRequested.RegisterAttack(source, target));
    }

    [TestMethod]
    public void Update_Attack_Target() 
    {
        string source = "source";
        string firstTarget = "firstTarget";
        string secondTarget = "secondTarget";
        var attacksRequested = new AttacksRequestedList();
        attacksRequested.RegisterAttack(source, firstTarget);
        Assert.IsTrue(attacksRequested.RegisterAttack(source, secondTarget));
    }

    [TestMethod]
    public void List_Attack_Registered() 
    {
        string source = "source";
        string target = "target";
        var attacksRequested = new AttacksRequestedList();
        attacksRequested.RegisterAttack(source, target);
        var list = attacksRequested.ListAttacks();
        Assert.IsTrue(
            list.All(attack => attack.Key == source && attack.Value == target));
        Assert.IsTrue(list.Count() == 1);
    }

    [TestMethod]
    public void Remove_Attack() 
    {
        string source = "source";
        string target = "target";
        var attacksRequested = new AttacksRequestedList();
        attacksRequested.RegisterAttack(source, target);
        Assert.IsTrue(attacksRequested.RemoveAttack(source));
        var list = attacksRequested.ListAttacks();
        Assert.IsTrue(list.Count() == 0);
    }
}