using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Models;

public class BoardData
{
    public BoardSize size { get; set; } = new();

    public List<EntityPosition> entitiesPosition { get; set; } = new();
}

public class BoardSize 
{
    public BoardSize(int width, int height) 
    {
        this.width = width;
        this.height = height;
    }

    public BoardSize() {
        this.width = 0;
        this.height = 0;
    }

    public int width { get; set; }
    public int height { get; set; }
}
