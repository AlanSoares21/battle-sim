using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Models;
using System.Text.RegularExpressions;

namespace BattleSimulator.Server.Routes;

[ApiController]
[Route("{controller}")]
public class LoginController: ControllerBase
{
    ILogger<LoginController> _logger;
    IAuthService _authService;
    public LoginController(
        ILogger<LoginController> logger,
        IAuthService authService) {
        _logger = logger;
        _authService = authService;
    }
    [HttpPost]
    [AllowAnonymous]
    public ActionResult AuthenticateUser(
        [FromBody] NewUser user
    ) {
        _logger.LogInformation("New user: {name}", user.name);
        if (string.IsNullOrEmpty(user.name))
            return NameIsEmpty();
        if (!ValidNameFormat(user.name))
            return NameHasInvalidCharacters(user.name);
        if (_authService.NameIsBeingUsed(user.name))
            return NameIsBeingUsed(user.name);
        return GenerateJwtToken(user);
    }

    BadRequestObjectResult NameIsEmpty() {
        _logger.LogError("A user tried log in with a empty name.");
        return BadRequest(new ApiError($"Username can't be empty."));
    }

    bool ValidNameFormat(string name) {
        const string startWithLetter = @"\A[a-zA-Z]";
        return Regex.IsMatch(name, startWithLetter);
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

    OkObjectResult GenerateJwtToken(NewUser user) {
        var tokenString = _authService.GenerateJwtToken(user.name);
        _logger.LogInformation("New token generated to user {user}.", user.name);
        return Ok(new SuccessLoginResponse() { Token = tokenString });
    }
}