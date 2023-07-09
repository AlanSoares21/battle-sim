using BattleSimulator.Server.Routes;
using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using BattleSimulator.Server.Models;
using System.Text.Json;

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
        AuthController AuthController = CreateTestableAuthController(authService);
        var response = await AuthController.AuthenticateUser(newUser) as ObjectResult;
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
        AuthController AuthController = CreateTestableAuthController(authService);
        var response = await AuthController.AuthenticateUser(newUser) as ObjectResult;
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
        AuthController AuthController = CreateTestableAuthController(authService);
        
        var response = AuthController.RefreshTokens();
        var response = await AuthController.AuthenticateUser(newUser) as ObjectResult;
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
        AuthController AuthController = CreateTestableAuthController(authService);
        var response = await AuthController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [DataRow("someValidUsername")]
    [DataRow("s2omeValidUsername")]
    [DataRow("SomeValidUsername")]
    public async Task When_User_Name_Is_Valid_Return_Ok(string username) 
    {
        var newUser = new NewUser() { name = username };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        AuthController AuthController = CreateTestableAuthController(authService);
        var response = await AuthController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_And_Store_Refresh_Token_When_Authenticate_User() 
    {
        var newUser = new NewUser() { name = "username" };
        IAuthService authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.NameIsBeingUsed(newUser.name)).Returns(false);
        string refreshToken = "some_refresh_token";
        A.CallTo(() => authService.CreateRefreshToken())
            .Returns(refreshToken);
        AuthController authController = CreateTestableAuthController(authService);
        var response = await authController.AuthenticateUser(newUser) as ObjectResult;
        if (response is null)
            Assert.Fail("fail because response is null.");
        A.CallTo(() => authService.CreateRefreshToken())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => authService.StoreRefreshToken(newUser.name, refreshToken))
            .MustHaveHappenedOnceExactly();
    }

    AuthController CreateTestableAuthController(IAuthService authService) => new AuthController(CreateFakeLogger(), authService);

    ILogger<AuthController> CreateFakeLogger() => A.Fake<ILogger<AuthController>>();

    // testar se retorna bad request quando tenta refresh com access invalido
    [TestMethod]
    public async Task When_Try_Get_New_Tokens_With_An_Invalid_Access_Token_Return_Bad_Request()
    {
        var authService = A.Fake<IAuthService>();
        SuccessLoginResponse tokens = new() {
            AccessToken = "someAccess",
            RefreshToken = "unregistered_refresh_token"
        };
        A.CallTo(() => authService.GetUsernameFromAccessToken(tokens.AccessToken))
            .Throws<ArgumentException>();
        AuthController authController = CreateTestableAuthController(authService);
        var response = await authController.RefreshToken(tokens) as ObjectResult;
        Assert.IsNotNull(response, "fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task When_Try_Get_New_Tokens_With_An_Unregistered_Refresh_Token_Return_Bad_Request()
    {
        string username = "someUsername";
        var authService = A.Fake<IAuthService>();
        SuccessLoginResponse tokens = new() {
            AccessToken = "someAccess",
            RefreshToken = "unregistered_refresh_token"
        };
        var userRegister = new UserAuthenticated() {
            Id = username,
            RefreshToken = "registered_refresh_token"
        };
        A.CallTo(() => authService.GetUsernameFromAccessToken(tokens.AccessToken))
            .Returns(username);
        A.CallTo(() => authService.GetUserAuthenticated(username))
            .Returns(userRegister);
        AuthController authController = CreateTestableAuthController(authService);
        var response = await authController.RefreshToken(tokens) as ObjectResult;
        Assert.IsNotNull(response, "fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task When_Try_Get_New_Access_Token_With_An_Expired_Refresh_Token_Return_Bad_Request()
    {
        string username = "someUsername";
        var authService = A.Fake<IAuthService>();
        SuccessLoginResponse tokens = new() {
            AccessToken = "someAccess",
            RefreshToken = "registered_refresh_token"
        };
        var userRegister = new UserAuthenticated() {
            Id = username,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiryTime = DateTime.MinValue
        };
        A.CallTo(() => authService.GetUsernameFromAccessToken(tokens.AccessToken))
            .Returns(username);
        A.CallTo(() => authService.GetUserAuthenticated(username))
            .Returns(userRegister);
        AuthController authController = CreateTestableAuthController(authService);
        var response = await authController.RefreshToken(tokens) as ObjectResult;
        Assert.IsNotNull(response, "fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    // pega nosovs tokens e atualiza o cache
    [TestMethod]
    public async Task When_Refresh_Tokens_Get_New_Tokens_And_Update_The_User_Record()
    {
        string newAccessToken = "new_access_token";
        string newRefreshToken = "new_refresh_token";
        string username = "someUsername";
        var authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.CreateAccessToken(username))
            .Returns(newAccessToken);
        A.CallTo(() => authService.CreateRefreshToken())
            .Returns(newRefreshToken);
        SuccessLoginResponse tokens = new() {
            AccessToken = "someAccess",
            RefreshToken = "registered_refresh_token"
        };
        var userRegister = new UserAuthenticated() {
            Id = username,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiryTime = DateTime.MaxValue
        };
        A.CallTo(() => authService.GetUsernameFromAccessToken(tokens.AccessToken))
            .Returns(username);
        A.CallTo(() => authService.GetUserAuthenticated(username))
            .Returns(userRegister);
        AuthController authController = CreateTestableAuthController(authService);
        await authController.RefreshToken(tokens);
    
        A.CallTo(() => authService.CreateAccessToken(username))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => authService.CreateRefreshToken())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => authService.StoreRefreshToken(username, newRefreshToken))
            .MustHaveHappenedOnceExactly();
    }
    
    [TestMethod]
    public async Task Return_New_Access_Token()
    {
        string newAccessToken = "new_access_token";
        string newRefreshToken = "new_refresh_token";
        string username = "someUsername";
        var authService = A.Fake<IAuthService>();
        A.CallTo(() => authService.CreateAccessToken(username))
            .Returns(newAccessToken);
        A.CallTo(() => authService.CreateRefreshToken())
            .Returns(newRefreshToken);
        SuccessLoginResponse tokens = new() {
            AccessToken = "someAccess",
            RefreshToken = "registered_refresh_token"
        };
        var userRegister = new UserAuthenticated() {
            Id = username,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiryTime = DateTime.MaxValue
        };
        A.CallTo(() => authService.GetUsernameFromAccessToken(tokens.AccessToken))
            .Returns(username);
        A.CallTo(() => authService.GetUserAuthenticated(username))
            .Returns(userRegister);
        AuthController authController = CreateTestableAuthController(authService);
        var response = await authController.RefreshToken(tokens) as ObjectResult;
        Assert.IsNotNull(response, "fail because response is null.");
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        var newTokens = GetTokenFromControllerResponse(response);
        Assert.IsNotNull(newTokens, "auth controller dont returned the new tokens");
        Assert.AreEqual(newTokens.AccessToken, newAccessToken);
        Assert.AreEqual(newTokens.RefreshToken, newRefreshToken);
    }

    SuccessLoginResponse? GetTokenFromControllerResponse(ObjectResult result)
    {
        return (SuccessLoginResponse)result.Value;
    }
}