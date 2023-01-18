using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Tests.StubClasses
{
    class StubAttackProps : IOffensiveAttributes
    {
        public int PhysicalDamage { get; set; }
        public int MagicalDamage { get; set; }
        public double CriticalHit { get; set; }
        public double Accuracy { get; set; }
        public double AttackSpeed { get; set; }
        public double Penetration { get; set; }
        public double SkillsCooldown { get; set; }
    }
}