using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Rouge;

// === ENUMS ===
public enum SkillType { Attack, Magic, Heal, Buff, Debuff }
public enum ItemType { HP, MP, Revive }
public enum CombatAction { Attack, Skill, Item, Defend, Flee }
public enum BattleState { PlayerTurn, EnemyTurn, Won, Lost, Animating }
public enum EquipmentSlot { Weapon, Armor, Accessory }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
public enum CharacterClass { Knight, Mage, Rogue, Paladin, Ranger, Berserker, Gunslinger }

// === DATA CLASSES ===

public partial class StatsData
{
    public string Name { get; set; } = "Hero";
    public int MaxHP { get; set; } = 100;
    public int CurrentHP { get; set; } = 100;
    public int MaxMP { get; set; } = 50;
    public int CurrentMP { get; set; } = 50;
    public int ATK { get; set; } = 10;
    public int DEF { get; set; } = 5;
    public int SPD { get; set; } = 8;
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int Gold { get; set; } = 0;
}

public partial class SkillDef
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillType Type { get; set; }
    public int MPCost { get; set; } = 0;
    public float Power { get; set; } = 1.0f;
    public int HealAmount { get; set; } = 0;
    public int Hits { get; set; } = 1;
    public string StatusEffect { get; set; } = "";
    public bool IgnoresDEF { get; set; } = false;      // Piercing Shot, Headshot, Sniper Shot, Silver Bullets
    public int SelfDamageHP { get; set; } = 0;         // Bloodletting, Ragnarok — hp cost
    public bool IsExecute { get; set; } = false;        // Death Sentence — kills if enemy HP < 25%
    public bool SkipEnemyTurn { get; set; } = false;    // Time Freeze
    public string BuffEffect { get; set; } = "";        // "counter", "evade", "double_next_magic", "mana_shield"

    public SkillDef() { }
    public SkillDef(string name, string desc, SkillType type, int mpCost, float power, int heal = 0,
        int hits = 1, string status = "", bool ignoresDef = false, int selfDmg = 0,
        bool execute = false, bool skipTurn = false, string buff = "")
    {
        Name = name; Description = desc; Type = type; MPCost = mpCost;
        Power = power; HealAmount = heal; Hits = hits; StatusEffect = status;
        IgnoresDEF = ignoresDef; SelfDamageHP = selfDmg; IsExecute = execute;
        SkipEnemyTurn = skipTurn; BuffEffect = buff;
    }
}

public partial class ItemData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemType Type { get; set; }
    public int Value { get; set; }
    public ItemData() { }
    public ItemData(string name, string desc, ItemType type, int value)
    { Name = name; Description = desc; Type = type; Value = value; }
}

public partial class EnemyDef
{
    public string Name { get; set; } = "Goblin";
    public int MaxHP { get; set; } = 40;
    public int ATK { get; set; } = 7;
    public int DEF { get; set; } = 3;
    public int SPD { get; set; } = 5;
    public int XP { get; set; } = 20;
    public int Gold { get; set; } = 10;
    public string Color { get; set; } = "red";
    public string Weakness { get; set; } = "";
    public int CurrentHP { get; set; }
    public bool IsAlive => CurrentHP > 0;

    public EnemyDef() { }
    public EnemyDef(string name, int hp, int atk, int def, int spd, int xp, int gold, string color, string weakness = "")
    {
        Name = name; MaxHP = hp; ATK = atk; DEF = def; SPD = spd;
        XP = xp; Gold = gold; Color = color; Weakness = weakness;
        CurrentHP = hp;
    }
}

// === EQUIPMENT ===
public partial class EquipmentDef
{
    public string Name { get; set; }
    public EquipmentSlot Slot { get; set; }
    public Rarity Rarity { get; set; }
    public int BonusATK { get; set; }
    public int BonusDEF { get; set; }
    public int BonusSPD { get; set; }
    public int BonusMaxHP { get; set; }
    public int BonusMaxMP { get; set; }
    public CharacterClass? RestrictedTo { get; set; } // null = unrestricted
    public string Description { get; set; }

    public EquipmentDef() { }

    public EquipmentDef(string name, EquipmentSlot slot, Rarity rarity,
        int atk, int def, int spd, int hp, int mp,
        CharacterClass? restricted = null, string desc = "")
    {
        Name = name; Slot = slot; Rarity = rarity;
        BonusATK = atk; BonusDEF = def; BonusSPD = spd;
        BonusMaxHP = hp; BonusMaxMP = mp;
        RestrictedTo = restricted;
        Description = string.IsNullOrEmpty(desc) ? $"{Rarity} {slot}" : desc;
    }

    public string RarityColor()
    {
        return Rarity switch
        {
            Rarity.Common => "#aaaaaa",
            Rarity.Uncommon => "#44cc44",
            Rarity.Rare => "#4488ff",
            Rarity.Epic => "#cc44ff",
            Rarity.Legendary => "#ff8800",
            _ => "#ffffff"
        };
    }
}

// === CHARACTER DEFINITION ===
public partial class CharacterDef
{
    public CharacterClass Class { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int BaseHP { get; set; }
    public int BaseMP { get; set; }
    public int BaseATK { get; set; }
    public int BaseDEF { get; set; }
    public int BaseSPD { get; set; }
    public string[] StartingSkills { get; set; }
    public EquipmentDef StartingWeapon { get; set; }
    public EquipmentDef StartingArmor { get; set; }
    public EquipmentDef StartingAccessory { get; set; }

    public Color ClassColor()
    {
        return Class switch
        {
            CharacterClass.Knight => new Color(0.5f, 0.5f, 0.8f),
            CharacterClass.Mage => new Color(0.3f, 0.5f, 1.0f),
            CharacterClass.Rogue => new Color(0.4f, 0.8f, 0.4f),
            CharacterClass.Paladin => new Color(0.9f, 0.8f, 0.3f),
            CharacterClass.Ranger => new Color(0.2f, 0.7f, 0.3f),
            CharacterClass.Berserker => new Color(0.9f, 0.2f, 0.2f),
            CharacterClass.Gunslinger => new Color(0.8f, 0.6f, 0.2f),
            _ => Colors.White
        };
    }
}

// === LOOT TABLE ENTRY ===
public partial class LootEntry
{
    public string EquipmentName { get; set; }
    public float Weight { get; set; }
    public LootEntry() { }
    public LootEntry(string name, float weight) { EquipmentName = name; Weight = weight; }
}

// === CITY DEFINITION ===
public partial class CityDef
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Color ThemeColor { get; set; }
    public int DifficultyTier { get; set; } // 0-7
    public int Rooms { get; set; } = 4;
    public bool UnlockedByDefault { get; set; } = false;

    // Regular enemies for this city (name, hp, atk, def, spd, xp, gold, color, weakness)
    public List<(string name, int hp, int atk, int def, int spd, int xp, int gold, string color, string weakness)> Enemies { get; set; } = new();

    // Main boss
    public (string name, int hp, int atk, int def, int spd, int xp, int gold, string color, string weakness) MainBoss;

    // Secret boss (much harder)
    public (string name, int hp, int atk, int def, int spd, int xp, int gold, string color, string weakness) SecretBoss;

    // Which cities unlock when this city's main boss is defeated
    public List<string> UnlocksCities { get; set; } = new();

    public CityDef() { }
}
