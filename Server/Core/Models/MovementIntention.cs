using BattleSimulator.Engine;

namespace BattleSimulator.Server.Models;
public class MovementIntention {
    public string entityIdentifier { get; set; } = "";
    public Coordinate cell { get; set; }
}