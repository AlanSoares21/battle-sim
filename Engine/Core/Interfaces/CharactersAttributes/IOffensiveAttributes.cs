
namespace BattleSimulator.Engine.Interfaces.CharactersAttributes
{
    public interface IOffensiveAttributes
    {
        int PhysicalDamage { get; set; }
        int MagicalDamage { get; set; }
        double CriticalHit { get; set; }
        double Accuracy { get; set; }
        double AttackSpeed { get; set; }
        double Penetration { get; set; }
        double SkillsCooldown { get; set; }
    }
}