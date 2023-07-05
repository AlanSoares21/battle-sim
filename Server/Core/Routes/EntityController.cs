using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Database;
using BattleSimulator.Server.Database.Models;

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
        {
            _logger.LogInformation("User {name} dont have a entity stored", 
                User.Identity.Name);
            return NotFound(new ApiError("You dont have an entity"));
        }
        return Ok(result);
    }

    [HttpPut]
    [Authorize]
    public ActionResult Update([FromBody] Entity entity)
    {
        if (User.Identity is null || User.Identity.Name is null)
        {
            _logger.LogError("Unhauthorized user trying to get entity data.");
            return Unauthorized(new ApiError("Must be authenticated to get your entity data"));
        }
        _logger.LogInformation("Updating entity data for user {name} ", User.Identity.Name);
        _db.UpdateEntity(entity);
        _logger.LogInformation("Entity for user {name} has been updated", User.Identity.Name);
        var result = _db.SearchEntity(User.Identity.Name);
        if (result is null)
        {
            _logger.LogError("Entity for user {name} not foun after update", User.Identity.Name);
            return BadRequest(new ApiError("After update, entitiy not found on db"));
        }
        return Ok(result);
    }
}