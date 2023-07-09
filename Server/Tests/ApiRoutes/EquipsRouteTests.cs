using System.Net;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Routes;
using Microsoft.AspNetCore.Mvc;

namespace BattleSimulator.Server.Tests.ApiRoutes;

[TestClass]
public class EquipsRouteTests
{
    [TestMethod]
    public void List_All_Equips()
    {
        IGameDb db = A.Fake<IGameDb>();
        List<Equip> equips = Utils.DefaultEquips.ToList();
        A.CallTo(() => db.GetEquips())
            .Returns(equips);
        EquipController controller = new EquipController(db);
        var result = controller.Get() as ObjectResult;
        Assert.IsNotNull(result, "result is null");
        Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
        A.CallTo(() => db.GetEquips())
            .MustHaveHappenedOnceExactly();
        Assert.AreEqual(equips, result.Value);
    }   
}