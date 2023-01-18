using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Routes;

[ApiController]
[Route("{controller}")]
public class LoginController: ControllerBase
{
    private ILogger<LoginController> _logger;
    private IAuthService _authService;
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
        if (_authService.NameIsBeingUsed(user.name))
            return NameIsBeingUsed(user.name);
        return GenerateJwtToken(user);
    }

    private BadRequestObjectResult NameIsEmpty() {
        _logger.LogError("A user tried log in with a empty name.");
        return BadRequest(new ApiError($"Username can't be empty."));
    }

    private BadRequestObjectResult NameIsBeingUsed(string name) {
        _logger.LogError("Name {name} already being used.", name);
        return BadRequest(new ApiError($"Name {name} already being used."));
    }

    private OkObjectResult GenerateJwtToken(NewUser user) {
        var tokenString = _authService.GenerateJwtToken(user.name);
        _logger.LogInformation("New token generated to user {user}.", user.name);
        return Ok(new SuccessLoginResponse() { Token = tokenString });
    }
}