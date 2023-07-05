using BattleSimulator.Server.Database;
using BattleSimulator.Server.Routes;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Controllers;
using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server.Tests.ApiRoutes;

[TestClass]
public class EntityRouteTests
{
    [TestMethod]
    public void Return_Error_When_User_Dont_Have_Entity_Stored()
    {
        string username = "user";
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEntity(username)).Returns(null);
        EntityController controller = 
            new EntityController(
                A.Fake<ILogger<EntityController>>(), 
                db);
        
        SetUserAuthenticated(controller, username);
        var response = controller.Get() as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void Return_Entity()
    {
        string username = "user";
        Entity entity = CreateEntity(username);
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEntity(username)).Returns(entity);
        EntityController controller = 
            new EntityController(
                A.Fake<ILogger<EntityController>>(), 
                db);
        
        SetUserAuthenticated(controller, username);
        var response = controller.Get() as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(entity, response.Value);
    }

    Entity CreateEntity(string username) => new() 
    {
        Id = username,
        Damage = 2,
        DefenseAbsorption = 0.5,
        HealthRadius = 25
    };
    
    void SetUserAuthenticated(ControllerBase controller, string user)
    {
        var claims = new Claim[] { 
            new(ClaimTypes.Name, user) 
        };
        
        HttpContext httpContext = new DefaultHttpContext();
        RouteValueDictionary routeDataDictioanry = new();
        RouteData routeData = new RouteData(routeDataDictioanry);
        ControllerActionDescriptor actionDescriptor = new ControllerActionDescriptor();
        ActionContext actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        actionContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var controllerContext = new ControllerContext(actionContext);
        controller.ControllerContext = controllerContext;
    }
}

