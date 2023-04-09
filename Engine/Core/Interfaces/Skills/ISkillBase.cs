namespace BattleSimulator.Engine.Interfaces.Skills;

public interface ISkillBase {
    string Name { get; }
    void Exec(IEntity target, IEntity source, IBattle battle);
}