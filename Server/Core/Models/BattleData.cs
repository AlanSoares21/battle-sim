using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Models;

public class BattleData
{
    public Guid id { get; set; }
    public BoardData board { get; set; } = new();
    public List<IEntity> entities { get; set; } = new();
}