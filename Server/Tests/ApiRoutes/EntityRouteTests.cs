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
    public void Register_A_Default_Entity_When_User_Dont_Have_Entity_Stored()
    {
        string username = "user";
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEntity(username)).Returns(null);
        IServerConfig config = A.Fake<IServerConfig>();
        Entity defaultEntity = new() { Id = username };
        A.CallTo(() => config.DefaultEntity(username))
            .Returns(defaultEntity);
        EntityController controller = CreateController(db, config);
        
        SetUserAuthenticated(controller, username);
        var response = controller.Get() as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        
        A.CallTo(() => config.DefaultEntity(username))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => db.AddEntity(defaultEntity))
            .MustHaveHappenedOnceExactly();
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(defaultEntity, response.Value);
    }

    [TestMethod]
    public void Return_Entity()
    {
        string username = "user";
        Entity entity = CreateEntity(username);
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEntity(username)).Returns(entity);
        EntityController controller = CreateController(db);
        
        SetUserAuthenticated(controller, username);
        var response = controller.Get() as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(entity, response.Value);
    }

    [TestMethod]
    public void Update_Entity()
    {
        string username = "user";
        Entity entity = CreateEntity(username);
        IGameDb db = A.Fake<IGameDb>();
        A.CallTo(() => db.SearchEntity(username)).Returns(entity);
        EntityController controller = CreateController(db);
        
        SetUserAuthenticated(controller, username);
        var response = controller.Update(entity) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        A.CallTo(() => db.UpdateEntity(An<Entity>.Ignored))
            .MustHaveHappenedOnceExactly();
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
    EntityController CreateController(IGameDb db) => 
        CreateController(
            db,
            A.Fake<IServerConfig>()
        );
    EntityController CreateController(IGameDb db, IServerConfig config) => 
        new EntityController(
            A.Fake<ILogger<EntityController>>(), 
            db,
            config
        );
}

