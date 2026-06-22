using Godot;

namespace NewGameProject;

public class Item
{
    public ItemType Type      { get; }
    public Vector2I GridPos   { get; set; }
    public bool     Collected { get; set; }

    public Item(ItemType type, Vector2I pos) { Type = type; GridPos = pos; }

    public string DisplayName => NameOf(Type);

    public static string NameOf(ItemType type) => type switch
    {
        ItemType.HealthPotion => "Health Potion",
        ItemType.Sword        => "Sword",
        ItemType.Shield       => "Shield",
        ItemType.Scroll       => "Scroll of Lightning",
        _                     => "Unknown",
    };

    /// <summary>Applies the item's effect. Returns false if it had no effect
    /// (e.g. the potion belt is already full), so callers can leave it uncollected.</summary>
    public bool Apply(PlayerData player, MessageLog log)
    {
        switch (Type)
        {
            case ItemType.HealthPotion:
                if (player.Potions >= PlayerData.MaxPotions)
                {
                    // Belt full — drink this one on the spot instead of storing it
                    int heal = player.PotionHeal;
                    player.Stats.Heal(heal);
                    log.Add($"Potion belt full — you drink it at once, restoring {heal} HP.", "#ff6666");
                    return true;
                }
                player.Potions++;
                log.Add($"You pocket a Health Potion. ([Q] to drink — you have {player.Potions})", "#ff6666");
                return true;
            case ItemType.Sword:
                player.Stats.Attack  += 3;
                log.Add("You wield a Sword. Attack +3!", "#ddddff");
                return true;
            case ItemType.Shield:
                player.Stats.Defense += 2;
                log.Add("You raise a Shield. Defense +2!", "#aaaacc");
                return true;
            case ItemType.Scroll:
                player.Stats.Attack  += 5;
                log.Add("Lightning crackles as you read the Scroll! Attack +5!", "#88ffff");
                return true;
            default:
                return false;
        }
    }
}
