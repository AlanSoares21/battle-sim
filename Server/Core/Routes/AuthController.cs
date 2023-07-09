using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Models;
using System.Text.RegularExpressions;

namespace BattleSimulator.Server.Routes;

[ApiController]
[Route("{controller}")]
public class AuthController: ControllerBase
{
    ILogger<AuthController> _logger;
    IAuthService _authService;
    public AuthController(
        ILogger<AuthController> logger,
        IAuthService authService) {
        _logger = logger;
        _authService = authService;
    }
    [HttpPost]
    [Route("Login")]
    [AllowAnonymous]
    public async Task<ActionResult> AuthenticateUser(
        [FromBody] NewUser user
    ) {
        _logger.LogInformation("New user: {name}", user.name);
        if (string.IsNullOrEmpty(user.name))
            return NameIsEmpty();
        if (!ValidNameFormat(user.name))
            return NameHasInvalidCharacters(user.name);
        if (_authService.NameIsBeingUsed(user.name))
            return NameIsBeingUsed(user.name);
        return await GenerateJwtToken(user);
    }

    BadRequestObjectResult NameIsEmpty() {
        _logger.LogError("A user tried log in with a empty name.");
        return BadRequest(new ApiError($"Username can't be empty."));
    }

    bool ValidNameFormat(string name) {
        // starts with letter and contains only letters or numbers
        const string startsWithLetter = @"\A[a-zA-Z]";
        const string hasInvalidChars = @"[^a-zA-Z0-9]";

        return Regex.IsMatch(name, startsWithLetter) && 
            !Regex.IsMatch(name, hasInvalidChars);
    }

    BadRequestObjectResult NameHasInvalidCharacters(string name) {
        _logger.LogError("The name {name} has invalid characters.",
            name);
        return BadRequest(new ApiError($"Name {name} has invalid characters."));
    }

    BadRequestObjectResult NameIsBeingUsed(string name) {
        _logger.LogError("Name {name} already being used.", name);
        return BadRequest(new ApiError($"Name {name} already being used."));
    }

    async Task<OkObjectResult> GenerateJwtToken(NewUser user) {
        var accessToken = _authService.CreateAccessToken(user.name);
        var refreshToken = _authService.CreateRefreshToken();
        await _authService.StoreRefreshToken(user.name, refreshToken);
        _logger.LogInformation("New tokens generated to user {user}.", user.name);
        return Ok(new SuccessLoginResponse() { 
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost]
    [Route("Refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken(
        [FromBody] SuccessLoginResponse tokens
    ) {
        _logger.LogInformation("refreshing token: {token}", tokens.AccessToken);
        string username;
        if (!TryGetUsername(tokens.AccessToken, out username))
            return BadRequest(new ApiError("Failied on get username from access token"));
        var userAuthenticated = await TryGetUserAuthenticated(username);
        if (userAuthenticated is null)
            return BadRequest(new ApiError($"Failied on get user authenticated data. Username: {username}"));
        if (userAuthenticated.RefreshToken is null || 
            userAuthenticated.RefreshToken != tokens.RefreshToken)
            return BadRequest(new ApiError($"Your refresh token dont match with the db refresh token. Username: {username}"));
        if (userAuthenticated.RefreshTokenExpiryTime < DateTime.UtcNow)
            return BadRequest(new ApiError($"Your refresh token expired in {userAuthenticated.RefreshTokenExpiryTime}. Username: {username}"));
        
        return Ok(await UpdateTokens(username));
    }

    bool TryGetUsername(string accessToken, out string username)
    {
        try {
            username = _authService.GetUsernameFromAccessToken(accessToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failied on try get username from access token - Message: {m} - token: {token}", 
                ex.Message,
                accessToken
            );
            username = "";
            return false;
        }
    }

    async Task<UserAuthenticated?> TryGetUserAuthenticated(string username)
    {
        try {
            return await _authService.GetUserAuthenticated(username);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failied on try get user authenticated record for {user} - Message: {m}", 
                username,
                ex.Message
            );
            return null;
        }
    }

    async Task<SuccessLoginResponse> UpdateTokens(string username)
    {
        SuccessLoginResponse newTokens = new();
        newTokens.AccessToken = _authService.CreateAccessToken(username);
        newTokens.RefreshToken = _authService.CreateRefreshToken();
        await _authService.StoreRefreshToken(username, newTokens.RefreshToken);
        return newTokens;
    }
}