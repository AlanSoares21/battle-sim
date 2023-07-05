using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server.Models;

public class BattleData
{
    public Guid id { get; set; }
    public BoardData board { get; set; } = new();
    public List<Entity> entities { get; set; } = new();
}