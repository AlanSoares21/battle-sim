using BattleSimulator.Server.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleSimulator.Server.Routes;

[ApiController]
[Route("{controller}")]
public class EquipController : ControllerBase
{
    IGameDb _db;
    public EquipController(IGameDb db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize]
    public ActionResult Get()
    {
        return Ok(_db.GetEquips());
    }
}