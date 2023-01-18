using BattleSimulator.Server.Routes;
using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests;

[TestClass]
public class CheckNameRouteTests
{
    [TestMethod]
    public void When_User_Name_Is_Alredy_In_Use_Returns_BadRequest() 
    {
        var newUser = new NewUser() { name = "username" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(true);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void When_User_Name_Is_Empty_Returns_BadRequest() 
    {
        var newUser = new NewUser() { name = "" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void When_User_Name_Is_Valid_Return_Ok() {
        var newUser = new NewUser() { name = "someValidUsername" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
    }

    LoginController CreateTestableLoginController(IAuthService authService) => new LoginController(CreateFakeLogger(), authService);

    ILogger<LoginController> CreateFakeLogger() => A.Fake<ILogger<LoginController>>();
}