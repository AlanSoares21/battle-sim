
namespace BattleSimulator.Engine
{
    public enum AttackTarget
    {
        // de player em player?
        PlayerToPlayer = 1,
        // de player em monstro?
        PlayerToMob = 2,
        // de monstro em player?
        MobToPlayer = 3,
        // de monstro em monstro?
        MobToMob = 4
    }
}