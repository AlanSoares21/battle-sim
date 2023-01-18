namespace BattleSimulator.Server.Models;

public class BoardData
{
    public int width { get; set; }
    public int height { get; set; }
    public List<EntityPosition> entitiesPosition { get; set; } = new();
}
