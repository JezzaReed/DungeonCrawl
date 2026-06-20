using System;

namespace NewGameProject;

public class EntityStats
{
    public string Name    { get; set; } = "Unknown";
    public int    MaxHp   { get; set; }
    public int    Hp      { get; set; }
    public int    Attack  { get; set; }
    public int    Defense { get; set; }
    public int    XpReward{ get; set; }

    public bool IsAlive => Hp > 0;

    public int TakeDamage(int attackValue)
    {
        int damage = Math.Max(1, attackValue - Defense);
        Hp = Math.Max(0, Hp - damage);
        return damage;
    }

    public void Heal(int amount) => Hp = Math.Min(MaxHp, Hp + amount);
}
