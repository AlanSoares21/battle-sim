using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Database;

namespace BattleSimulator.Server.Routes;

[ApiController]
[Route("{controller}")]
public class EntityController : ControllerBase
{
    ILogger<EntityController> _logger;
    IGameDb _db;

    public EntityController(
        ILogger<EntityController> logger,
        IGameDb gameDb)
    {
        _logger = logger;
        _db = gameDb;
    }

    [HttpGet]
    [Authorize]
    public ActionResult Get()
    {
        if (User.Identity is null || User.Identity.Name is null)
        {
            _logger.LogError("Unhauthorized user trying to get entity data.");
            return Unauthorized(new ApiError("Must be authenticated to get your entity data"));
        }
        var result = _db.SearchEntity(User.Identity.Name);
        if (result is null)
            return BadRequest(new ApiError("You dont have an entity"));
        return Ok(result);
    }
}