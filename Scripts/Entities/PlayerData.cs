using System;
using Godot;

namespace NewGameProject;

public class PlayerData
{
    public EntityStats Stats { get; } = new()
    {
        Name    = "Hero",
        MaxHp   = 30,
        Hp      = 30,
        Attack  = 5,
        Defense = 2,
    };

    public Vector2I GridPos        { get; set; }
    public int      Floor          { get; set; } = 1;
    public int      Level          { get; set; } = 1;
    public int      Xp             { get; set; } = 0;
    public int      XpToNextLevel  { get; set; } = 50;
    public int      KillCount      { get; set; } = 0;
    public int      Score          { get; set; } = 0;
    public const int MaxPotions = 3;
    public int      Potions        { get; set; } = 0;

    /// <summary>HP restored by drinking one Health Potion, scaling with depth.</summary>
    public int PotionHeal => 20 + Floor * 3;
    public string   KilledBy       { get; set; } = "the dungeon";

    public void AddXp(int amount, MessageLog log)
    {
        Xp    += amount;
        Score += amount;
        while (Xp >= XpToNextLevel)
        {
            Xp -= XpToNextLevel;
            LevelUp(log);
        }
    }

    private void LevelUp(MessageLog log)
    {
        Level         += 1;
        Stats.MaxHp   += 10;
        Stats.Hp       = Stats.MaxHp;
        Stats.Attack   += 1;
        Stats.Defense  += 1;
        XpToNextLevel   = (int)(XpToNextLevel * 1.5f);
        log.Add($"Level {Level}! Max HP: {Stats.MaxHp} | ATK: {Stats.Attack} | DEF: {Stats.Defense}", "#ffff44");
    }
}
