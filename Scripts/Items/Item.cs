using Godot;

namespace NewGameProject;

public class Item
{
    public ItemType Type      { get; }
    public Vector2I GridPos   { get; set; }
    public bool     Collected { get; set; }

    public Item(ItemType type, Vector2I pos) { Type = type; GridPos = pos; }

    public string DisplayName => Type switch
    {
        ItemType.HealthPotion => "Health Potion",
        ItemType.Sword        => "Sword",
        ItemType.Shield       => "Shield",
        ItemType.Scroll       => "Scroll of Lightning",
        _                     => "Unknown",
    };

    public void Apply(PlayerData player, MessageLog log)
    {
        switch (Type)
        {
            case ItemType.HealthPotion:
                player.Potions++;
                log.Add($"You pocket a Health Potion. ([Q] to drink — you have {player.Potions})", "#ff6666");
                break;
            case ItemType.Sword:
                player.Stats.Attack  += 3;
                log.Add("You wield a Sword. Attack +3!", "#ddddff");
                break;
            case ItemType.Shield:
                player.Stats.Defense += 2;
                log.Add("You raise a Shield. Defense +2!", "#aaaacc");
                break;
            case ItemType.Scroll:
                player.Stats.Attack  += 5;
                log.Add("Lightning crackles as you read the Scroll! Attack +5!", "#88ffff");
                break;
        }
    }
}
