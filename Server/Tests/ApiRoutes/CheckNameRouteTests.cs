using BattleSimulator.Server.Routes;
using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests.ApiRoutes;

[TestClass]
public class CheckNameRouteTests
{
    [TestMethod]
    public async Task When_User_Name_Is_Alredy_In_Use_Returns_BadRequest() 
    {
        var newUser = new NewUser() { name = "username" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(true);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = await loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task When_User_Name_Is_Empty_Returns_BadRequest() 
    {
        var newUser = new NewUser() { name = "" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = await loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    /*
    [TestMethod]
    public async Task Return_Bad_Request_When_Try_Get_New_Access_Token_With_An_Unregistered_Refresh_Token() 
    {
        SuccessLoginResponse oldTokens = new() {
            AccessToken = "old",
            RefreshToken = "your refresh-token"
        }
        var newUser = new NewUser() { name = "" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        LoginController loginController = CreateTestableLoginController(authService);
        
        var response = loginController.RefreshTokens();
        var response = await loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }
    */
    /*
        todo
            testar falha quando tenta gerar new access token mas o antigo acess token fornecido é invalido
            testar falha quando tenta gerar new access token com refresh token expirado
            testar 
    */

    [TestMethod]
    [DataRow("4name")]
    [DataRow("-name")]
    [DataRow("çname")]
    [DataRow(" name")]
    [DataRow("nçame")]
    [DataRow("na me")]
    public async Task When_User_Name_Contains_Invalid_Characters_Returns_BadRequest(
        string username
    ) {
        NewUser newUser = new() { name = username };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = await loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [DataRow("someValidUsername")]
    [DataRow("s2omeValidUsername")]
    [DataRow("SomeValidUsername")]
    public async Task When_User_Name_Is_Valid_Return_Ok(
        string username
    ) {
        var newUser = new NewUser() { name = username };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        LoginController loginController = CreateTestableLoginController(authService);
        var response = await loginController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
    }

    LoginController CreateTestableLoginController(IAuthService authService) => new LoginController(CreateFakeLogger(), authService);

    ILogger<LoginController> CreateFakeLogger() => A.Fake<ILogger<LoginController>>();
}