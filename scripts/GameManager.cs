using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Rouge;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public StatsData PlayerStats { get; private set; } = new();
    public CharacterClass SelectedClass { get; private set; } = CharacterClass.Knight;
    public EquipmentDef EquippedWeapon { get; set; }
    public EquipmentDef EquippedArmor { get; set; }
    public EquipmentDef EquippedAccessory { get; set; }
    public List<SkillDef> LearnedSkills { get; private set; } = new();
    public Dictionary<string, int> Inventory { get; private set; } = new();
    public List<EquipmentDef> EquipmentInventory { get; private set; } = new();

    // Loot skills — names of skills that can drop from bosses
    public static readonly Dictionary<CharacterClass, string[]> LootSkills = new()
    {
        [CharacterClass.Knight] = new[] { "Royal Decree" },
        [CharacterClass.Mage] = new[] { "Time Freeze" },
        [CharacterClass.Rogue] = new[] { "Smoke Bomb" },
        [CharacterClass.Paladin] = new[] { "Sacred Oath" },
        [CharacterClass.Ranger] = new[] { "Trap" },
        [CharacterClass.Berserker] = new[] { "Berserker Soul" },
        [CharacterClass.Gunslinger] = new[] { "Silver Bullets" },
    };

    private RandomNumberGenerator _rng = new();

    // === CITY SYSTEM ===
    public string CurrentCityName { get; set; } = "Ravenhold";
    public HashSet<string> UnlockedCities { get; private set; } = new();
    public Dictionary<string, bool> BossesDefeated { get; private set; } = new();
    public bool SecretBossBeatenInCurrentCity { get; set; } = false;
    public bool MainBossBeatenInCurrentCity { get; set; } = false;
    public CityDef GetCurrentCityDef() => AllCities[CurrentCityName];
    public bool IsBossDefeated(string city, bool main) => BossesDefeated.ContainsKey($"{city}_{(main ? "main" : "secret")}");

    // ===== ALL 8 CITIES =====
    public static readonly Dictionary<string, CityDef> AllCities = new()
    {
        ["Ravenhold"] = new CityDef
        {
            Name = "Ravenhold", Description = "A misty forest city, overrun by beasts.",
            ThemeColor = new Color(0.3f, 0.6f, 0.3f), DifficultyTier = 0, Rooms = 3, UnlockedByDefault = true,
            Enemies = new() { ("Slime",25,4,1,3,10,5,"aqua","Fireball"), ("Giant Rat",30,6,2,6,12,6,"brown","Slash"), ("Moss Golem",45,7,5,3,18,10,"lime","")},
            MainBoss = ("The Great Wolf", 80, 12, 6, 7, 50, 30, "gray", "Fireball"),
            SecretBoss = ("Fae King", 150, 18, 10, 14, 120, 80, "purple", "Thunder"),
            UnlocksCities = new() { "Cinderwall" }
        },
        ["Cinderwall"] = new CityDef
        {
            Name = "Cinderwall", Description = "A volcanic fortress where ember and ash reign.",
            ThemeColor = new Color(0.8f, 0.3f, 0.1f), DifficultyTier = 1, Rooms = 4,
            Enemies = new() { ("Ember Imp",35,8,3,5,18,8,"orange","Blizzard"), ("Lava Crab",50,9,7,3,22,12,"red",""), ("Ash Wraith",40,10,4,9,24,14,"gray","Holy Light")},
            MainBoss = ("Magma Titan", 120, 16, 10, 5, 70, 45, "orange", "Blizzard"),
            SecretBoss = ("Phoenix Lord", 200, 24, 12, 16, 160, 100, "gold", "Blizzard"),
            UnlocksCities = new() { "Silverport", "Frostheim" }
        },
        ["Silverport"] = new CityDef
        {
            Name = "Silverport", Description = "A drowned harbor city of ghost pirates.",
            ThemeColor = new Color(0.2f, 0.4f, 0.7f), DifficultyTier = 2, Rooms = 4,
            Enemies = new() { ("Reef Stalker",45,10,5,7,25,14,"aqua","Thunder"), ("Pirate Ghost",55,12,6,8,28,16,"white","Holy Light"), ("Krakenling",60,11,8,5,30,18,"purple","")},
            MainBoss = ("Drowned Captain", 140, 18, 12, 9, 80, 50, "aqua", "Thunder"),
            SecretBoss = ("Leviathan", 240, 28, 16, 12, 180, 120, "blue", "Thunder"),
            UnlocksCities = new() { "Hollowmere" }
        },
        ["Frostheim"] = new CityDef
        {
            Name = "Frostheim", Description = "An icy peak where blizzards never cease.",
            ThemeColor = new Color(0.5f, 0.7f, 0.9f), DifficultyTier = 2, Rooms = 4,
            Enemies = new() { ("Frost Sprite",35,9,4,10,22,10,"aqua","Fireball"), ("Ice Golem",65,13,10,3,32,18,"white",""), ("Snow Panther",50,14,6,12,28,15,"gray","Power Bash")},
            MainBoss = ("Frost Queen", 150, 20, 14, 8, 85, 55, "aqua", "Fireball"),
            SecretBoss = ("Abominable Yeti", 260, 30, 18, 10, 200, 130, "white", ""),
            UnlocksCities = new() { "Hollowmere" }
        },
        ["Hollowmere"] = new CityDef
        {
            Name = "Hollowmere", Description = "A haunted necropolis where the dead refuse to rest.",
            ThemeColor = new Color(0.5f, 0.2f, 0.5f), DifficultyTier = 3, Rooms = 5,
            Enemies = new() { ("Skeleton",55,14,7,5,30,18,"white","Power Bash"), ("Banshee",45,15,5,13,35,20,"purple","Holy Light"), ("Bone Golem",75,16,10,4,38,22,"gray","")},
            MainBoss = ("Lich King", 180, 24, 16, 10, 100, 65, "purple", "Holy Light"),
            SecretBoss = ("Death's Herald", 300, 35, 20, 15, 240, 150, "red", "Holy Light"),
            UnlocksCities = new() { "Sunspire", "Blightfen" }
        },
        ["Sunspire"] = new CityDef
        {
            Name = "Sunspire", Description = "A desert city of sun-worshippers.",
            ThemeColor = new Color(0.9f, 0.7f, 0.2f), DifficultyTier = 4, Rooms = 5,
            Enemies = new() { ("Sand Worm",60,17,8,6,35,22,"brown","Blizzard"), ("Solar Cultist",50,18,6,10,38,24,"orange",""), ("Mummy",70,16,12,4,36,20,"gold","Fireball")},
            MainBoss = ("The Sun Priest", 200, 28, 18, 12, 120, 75, "gold", "Blizzard"),
            SecretBoss = ("Anubis Reborn", 320, 38, 22, 14, 280, 170, "gold", ""),
            UnlocksCities = new() { "Shadowhold" }
        },
        ["Blightfen"] = new CityDef
        {
            Name = "Blightfen", Description = "A toxic swamp where life mutates.",
            ThemeColor = new Color(0.3f, 0.5f, 0.1f), DifficultyTier = 4, Rooms = 5,
            Enemies = new() { ("Toxic Slime",50,16,7,4,32,20,"lime","Fireball"), ("Plague Rat",45,19,5,11,34,18,"brown",""), ("Mutant",80,18,11,6,40,25,"green","Power Bash")},
            MainBoss = ("The Blight Lord", 220, 30, 20, 10, 130, 80, "lime", "Fireball"),
            SecretBoss = ("The Abomination", 350, 40, 24, 8, 300, 180, "green", ""),
            UnlocksCities = new() { "Shadowhold" }
        },
        ["Shadowhold"] = new CityDef
        {
            Name = "Shadowhold", Description = "The final citadel of darkness.",
            ThemeColor = new Color(0.3f, 0.1f, 0.3f), DifficultyTier = 5, Rooms = 6,
            Enemies = new() { ("Shadow Knight",70,22,12,8,45,28,"gray","Holy Light"), ("Void Beast",85,24,10,14,50,30,"purple","Thunder"), ("Nightmare",60,25,8,16,48,26,"red","")},
            MainBoss = ("The Dark Emperor", 300, 36, 24, 15, 200, 120, "purple", ""),
            SecretBoss = ("The Void Itself", 500, 50, 30, 20, 500, 300, "black", ""),
            UnlocksCities = new() { }
        },
    };

    // ===== ALL SKILLS =====
    public static readonly Dictionary<string, SkillDef> AllSkills = new()
    {
        // === UNIVERSAL ===
        ["Slash"] = new("Slash", "A strong sword strike", SkillType.Attack, 0, 1.5f),
        ["Heal"] = new("Heal", "Restore 40 HP", SkillType.Heal, 6, 0, 40),
        ["Power Bash"] = new("Power Bash", "Shatter defenses", SkillType.Attack, 4, 2.0f),

        // === KNIGHT (Lv1,3,7,12,18 + loot) ===
        ["Shield Bash"] = new("Shield Bash", "Stun + damage", SkillType.Attack, 3, 1.2f, 0, 1, "stun"),
        ["Iron Wall"] = new("Iron Wall", "Raise DEF", SkillType.Buff, 4, 0, 0, 1, "def_up"),
        ["Counter Strike"] = new("Counter Strike", "Counterattack", SkillType.Attack, 6, 1.8f, 0, 1, "counter", false, 0, false, false, "counter"),
        ["Holy Wall"] = new("Holy Wall", "Massive DEF boost", SkillType.Buff, 8, 0, 0, 1, "def_up"),
        ["Guillotine"] = new("Guillotine", "Execute weak foes", SkillType.Attack, 10, 4.0f, 0, 1, "", false, 0, true),
        ["Royal Decree"] = new("Royal Decree", "Party ATK up", SkillType.Buff, 10, 0, 0, 1, ""),

        // === MAGE (Lv1,3,7,12,18 + loot) ===
        ["Fireball"] = new("Fireball", "Ball of flame", SkillType.Magic, 8, 2.5f),
        ["Thunder"] = new("Thunder", "Lightning strike", SkillType.Magic, 10, 3.0f),
        ["Blizzard"] = new("Blizzard", "Freezing blast", SkillType.Magic, 12, 3.5f),
        ["Mana Shield"] = new("Mana Shield", "Convert MP to HP", SkillType.Heal, 8, 0, 0, 1, "", false, 0, false, false, "mana_shield"),
        ["Meteor"] = new("Meteor", "Destruction from above", SkillType.Magic, 18, 4.5f),
        ["Arcane Surge"] = new("Arcane Surge", "Double next magic", SkillType.Buff, 6, 0, 0, 1, "", false, 0, false, false, "double_next_magic"),
        ["Time Freeze"] = new("Time Freeze", "Skip enemy turn", SkillType.Magic, 14, 0, 0, 1, "", false, 0, false, true),

        // === ROGUE (Lv1,3,7,12,18 + loot) ===
        ["Backstab"] = new("Backstab", "Precise strike", SkillType.Attack, 5, 3.0f),
        ["Poison Blade"] = new("Poison Blade", "Poisons target", SkillType.Attack, 3, 1.0f, 0, 1, "poison"),
        ["Shadow Step"] = new("Shadow Step", "Crit + evade", SkillType.Attack, 7, 2.5f, 0, 1, "", false, 0, false, false, "evade"),
        ["Venom Strike"] = new("Venom Strike", "Heavy poison", SkillType.Attack, 8, 1.5f, 0, 1, "poison"),
        ["Death Sentence"] = new("Death Sentence", "Execute weak foes", SkillType.Attack, 10, 0, 0, 1, "", false, 0, true),
        ["Smoke Bomb"] = new("Smoke Bomb", "Lower enemy ATK", SkillType.Debuff, 6, 0, 0, 1, ""),

        // === PALADIN (Lv1,3,7,12,18 + loot) ===
        ["Holy Light"] = new("Holy Light", "Sacred damage", SkillType.Magic, 8, 2.0f),
        ["Barrier"] = new("Barrier", "Party DEF up", SkillType.Buff, 6, 0, 0, 1, "def_up"),
        ["Divine Protection"] = new("Divine Protection", "Heal + DEF buff", SkillType.Heal, 8, 0, 30, 1, "def_up"),
        ["Holy Judgment"] = new("Holy Judgment", "AoE + stun", SkillType.Magic, 12, 2.0f, 0, 1, "stun"),
        ["Resurrection"] = new("Resurrection", "Massive heal", SkillType.Heal, 14, 0, 80),
        ["Sacred Oath"] = new("Sacred Oath", "Party ATK+DEF+SPD up", SkillType.Buff, 12, 0),

        // === RANGER (Lv1,3,7,12,18 + loot) ===
        ["Power Shot"] = new("Power Shot", "Piercing arrow", SkillType.Attack, 4, 2.8f, 0, 1, "", true),
        ["Quick Shot"] = new("Quick Shot", "Rapid fire x3", SkillType.Attack, 6, 1.2f, 0, 3),
        ["Rain of Arrows"] = new("Rain of Arrows", "Arrow storm x4", SkillType.Attack, 10, 1.2f, 0, 4),
        ["Eagle Eye"] = new("Eagle Eye", "Sharpen senses", SkillType.Buff, 4, 0, 0, 1, "def_up"),
        ["Sniper Shot"] = new("Sniper Shot", "Ignore DEF", SkillType.Attack, 10, 3.5f, 0, 1, "", true),
        ["Trap"] = new("Trap", "Stun enemy", SkillType.Debuff, 5, 0, 0, 1, "stun"),

        // === BERSERKER (Lv1,3,7,12,18 + loot) ===
        ["War Cry"] = new("War Cry", "Fearsome cry", SkillType.Attack, 5, 1.8f),
        ["Rage"] = new("Rage", "ATK++ DEF--", SkillType.Buff, 0, 0, 0, 1, "rage"),
        ["Bloodletting"] = new("Bloodletting", "HP cost for power", SkillType.Attack, 3, 3.5f, 0, 1, "", false, 30),
        ["Unstoppable"] = new("Unstoppable", "Purge + ATK up", SkillType.Buff, 6, 0, 0, 1, "rage"),
        ["Ragnarok"] = new("Ragnarok", "Ultimate sacrifice x3", SkillType.Attack, 10, 4.5f, 0, 3, "", false, 60),
        ["Berserker Soul"] = new("Berserker Soul", "Massive rage", SkillType.Buff, 8, 0, 0, 1, "rage"),

        // === GUNSLINGER (Lv1,3,7,12,18 + loot) ===
        ["Quick Draw"] = new("Quick Draw", "Fast shot", SkillType.Attack, 0, 1.5f),
        ["Piercing Shot"] = new("Piercing Shot", "Ignores armor", SkillType.Attack, 5, 2.0f, 0, 1, "", true),
        ["Dodge"] = new("Dodge", "Avoid next attack", SkillType.Buff, 3, 0, 0, 1, "", false, 0, false, false, "evade"),
        ["Fan the Hammer"] = new("Fan the Hammer", "Rapid fire x4", SkillType.Attack, 7, 1.0f, 0, 4),
        ["Headshot"] = new("Headshot", "Precise kill shot", SkillType.Attack, 8, 3.0f, 0, 1, "", true),
        ["Trick Shot"] = new("Trick Shot", "Ricochet blast x2", SkillType.Attack, 10, 1.8f, 0, 2, "stun"),
        ["Bullet Storm"] = new("Bullet Storm", "Unload all shots x6", SkillType.Attack, 14, 1.2f, 0, 6),
        ["Silver Bullets"] = new("Silver Bullets", "Pure silver x2", SkillType.Attack, 12, 2.5f, 0, 2, "", true),
    };

    // ===== CHARACTERS =====
    public static readonly Dictionary<CharacterClass, CharacterDef> AllCharacters = new()
    {
        [CharacterClass.Knight] = new() { Class=CharacterClass.Knight, Name="Knight", Description="Stalwart vanguard. High HP and DEF.", BaseHP=140, BaseMP=20, BaseATK=10, BaseDEF=12, BaseSPD=5, StartingSkills=new[]{"Slash","Shield Bash","Iron Wall"}, StartingWeapon=new("Longsword",EquipmentSlot.Weapon,Rarity.Common,5,0,0,0,0,CharacterClass.Knight), StartingArmor=new("Plate Armor",EquipmentSlot.Armor,Rarity.Common,0,6,0,20,0,CharacterClass.Knight), StartingAccessory=new("Iron Ring",EquipmentSlot.Accessory,Rarity.Common,0,2,0,0,0) },
        [CharacterClass.Mage] = new() { Class=CharacterClass.Mage, Name="Mage", Description="Arcane scholar with devastating magic.", BaseHP=70, BaseMP=80, BaseATK=6, BaseDEF=4, BaseSPD=7, StartingSkills=new[]{"Fireball","Thunder","Heal"}, StartingWeapon=new("Mystic Staff",EquipmentSlot.Weapon,Rarity.Common,0,0,0,0,20,CharacterClass.Mage), StartingArmor=new("Arcane Robe",EquipmentSlot.Armor,Rarity.Common,0,2,0,0,15,CharacterClass.Mage), StartingAccessory=new("Mana Crystal",EquipmentSlot.Accessory,Rarity.Common,0,0,0,0,20) },
        [CharacterClass.Rogue] = new() { Class=CharacterClass.Rogue, Name="Rogue", Description="Shadowy assassin with blinding speed.", BaseHP=85, BaseMP=35, BaseATK=14, BaseDEF=5, BaseSPD=12, StartingSkills=new[]{"Slash","Backstab","Poison Blade"}, StartingWeapon=new("Shadow Dagger",EquipmentSlot.Weapon,Rarity.Common,7,0,2,0,0,CharacterClass.Rogue), StartingArmor=new("Leather Vest",EquipmentSlot.Armor,Rarity.Common,0,3,1,0,0,CharacterClass.Rogue), StartingAccessory=new("Swift Boots",EquipmentSlot.Accessory,Rarity.Common,1,0,3,0,0) },
        [CharacterClass.Paladin] = new() { Class=CharacterClass.Paladin, Name="Paladin", Description="Holy warrior who heals and protects.", BaseHP=110, BaseMP=45, BaseATK=9, BaseDEF=9, BaseSPD=6, StartingSkills=new[]{"Slash","Heal","Holy Light"}, StartingWeapon=new("War Mace",EquipmentSlot.Weapon,Rarity.Common,5,1,0,0,0,CharacterClass.Paladin), StartingArmor=new("Chainmail",EquipmentSlot.Armor,Rarity.Common,0,5,0,10,0,CharacterClass.Paladin), StartingAccessory=new("Holy Symbol",EquipmentSlot.Accessory,Rarity.Common,0,0,0,0,10) },
        [CharacterClass.Ranger] = new() { Class=CharacterClass.Ranger, Name="Ranger", Description="Deadly marksman with precise strikes.", BaseHP=90, BaseMP=30, BaseATK=12, BaseDEF=6, BaseSPD=10, StartingSkills=new[]{"Power Shot","Quick Shot","Heal"}, StartingWeapon=new("Longbow",EquipmentSlot.Weapon,Rarity.Common,7,0,1,0,0,CharacterClass.Ranger), StartingArmor=new("Hunter's Garb",EquipmentSlot.Armor,Rarity.Common,1,3,1,0,0,CharacterClass.Ranger), StartingAccessory=new("Eagle Eye",EquipmentSlot.Accessory,Rarity.Common,2,0,2,0,0) },
        [CharacterClass.Berserker] = new() { Class=CharacterClass.Berserker, Name="Berserker", Description="Unstoppable fury. Highest ATK.", BaseHP=130, BaseMP=15, BaseATK=16, BaseDEF=5, BaseSPD=8, StartingSkills=new[]{"Power Bash","Rage","War Cry"}, StartingWeapon=new("Greataxe",EquipmentSlot.Weapon,Rarity.Common,9,0,-1,0,0,CharacterClass.Berserker), StartingArmor=new("Fur Armor",EquipmentSlot.Armor,Rarity.Common,2,2,0,5,0,CharacterClass.Berserker), StartingAccessory=new("War Drum",EquipmentSlot.Accessory,Rarity.Common,3,0,0,0,0) },
        [CharacterClass.Gunslinger] = new() { Class=CharacterClass.Gunslinger, Name="Gunslinger", Description="Quick-draw gunfighter with piercing shots.", BaseHP=95, BaseMP=30, BaseATK=13, BaseDEF=6, BaseSPD=11, StartingSkills=new[]{"Quick Draw","Piercing Shot","Dodge"}, StartingWeapon=new("Flintlock Pistol",EquipmentSlot.Weapon,Rarity.Common,6,0,2,0,0,CharacterClass.Gunslinger), StartingArmor=new("Duster Coat",EquipmentSlot.Armor,Rarity.Common,0,3,1,0,0,CharacterClass.Gunslinger), StartingAccessory=new("Gunpowder Pouch",EquipmentSlot.Accessory,Rarity.Common,2,0,0,0,0) },
    };

    public Color ClassColor(CharacterClass cls) => cls switch
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

    // ===== ALL ITEMS =====
    public static readonly Dictionary<string, ItemData> AllItems = new()
    {
        ["Potion"] = new("Potion", "Restore 40 HP", ItemType.HP, 40),
        ["Hi-Potion"] = new("Hi-Potion", "Restore 100 HP", ItemType.HP, 100),
        ["Ether"] = new("Ether", "Restore 25 MP", ItemType.MP, 25),
        ["Phoenix Down"] = new("Phoenix Down", "Revive with 50% HP", ItemType.Revive, 50),
    };

    // ===== EQUIPMENT =====
    public static List<EquipmentDef> AllEquipment = new()
    {
        new("Rusty Sword",EquipmentSlot.Weapon,Rarity.Common,3,0,0,0,0), new("Iron Blade",EquipmentSlot.Weapon,Rarity.Uncommon,6,0,0,0,0), new("Steel Saber",EquipmentSlot.Weapon,Rarity.Rare,10,1,1,0,0), new("Dark Blade",EquipmentSlot.Weapon,Rarity.Epic,15,0,2,0,0),
        new("Bastard Sword",EquipmentSlot.Weapon,Rarity.Uncommon,7,1,0,10,0,CharacterClass.Knight), new("Greatsword",EquipmentSlot.Weapon,Rarity.Rare,12,2,-1,15,0,CharacterClass.Knight), new("Excalibur",EquipmentSlot.Weapon,Rarity.Legendary,20,3,2,25,5,CharacterClass.Knight),
        new("Archmage Staff",EquipmentSlot.Weapon,Rarity.Uncommon,0,0,0,0,30,CharacterClass.Mage), new("Elder Staff",EquipmentSlot.Weapon,Rarity.Rare,2,0,0,0,45,CharacterClass.Mage), new("Staff of Ages",EquipmentSlot.Weapon,Rarity.Legendary,5,2,3,20,60,CharacterClass.Mage),
        new("Poison Fang",EquipmentSlot.Weapon,Rarity.Uncommon,9,0,2,0,0,CharacterClass.Rogue), new("Viper Blade",EquipmentSlot.Weapon,Rarity.Rare,13,0,4,0,0,CharacterClass.Rogue), new("Soul Reaper",EquipmentSlot.Weapon,Rarity.Legendary,18,0,6,10,5,CharacterClass.Rogue),
        new("Holy Mace",EquipmentSlot.Weapon,Rarity.Uncommon,7,2,0,5,5,CharacterClass.Paladin), new("Crusader's Hammer",EquipmentSlot.Weapon,Rarity.Rare,11,3,0,10,10,CharacterClass.Paladin), new("Morning Star",EquipmentSlot.Weapon,Rarity.Legendary,16,4,1,20,15,CharacterClass.Paladin),
        new("Composite Bow",EquipmentSlot.Weapon,Rarity.Uncommon,9,0,2,0,0,CharacterClass.Ranger), new("Recurve Bow",EquipmentSlot.Weapon,Rarity.Rare,14,0,3,0,5,CharacterClass.Ranger), new("Artemis",EquipmentSlot.Weapon,Rarity.Legendary,20,1,5,10,10,CharacterClass.Ranger),
        new("Berserker Axe",EquipmentSlot.Weapon,Rarity.Uncommon,12,0,-2,5,0,CharacterClass.Berserker), new("Fury Axe",EquipmentSlot.Weapon,Rarity.Rare,18,0,-1,10,0,CharacterClass.Berserker), new("Bloodcleaver",EquipmentSlot.Weapon,Rarity.Legendary,25,1,0,20,0,CharacterClass.Berserker),
        new("Flintlock",EquipmentSlot.Weapon,Rarity.Uncommon,7,0,1,0,0,CharacterClass.Gunslinger), new("Revolver",EquipmentSlot.Weapon,Rarity.Rare,12,0,2,0,0,CharacterClass.Gunslinger), new("Deathbringer",EquipmentSlot.Weapon,Rarity.Legendary,18,1,4,10,5,CharacterClass.Gunslinger),
        new("Leather Armor",EquipmentSlot.Armor,Rarity.Common,0,3,0,0,0), new("Scale Mail",EquipmentSlot.Armor,Rarity.Uncommon,0,5,0,5,0), new("Chain Mail",EquipmentSlot.Armor,Rarity.Rare,0,8,-1,10,0), new("Mithril Plate",EquipmentSlot.Armor,Rarity.Epic,1,12,0,15,5), new("Dragonhide",EquipmentSlot.Armor,Rarity.Legendary,3,16,2,25,10),
        new("Copper Ring",EquipmentSlot.Accessory,Rarity.Common,1,1,0,0,0), new("Silver Ring",EquipmentSlot.Accessory,Rarity.Uncommon,2,2,1,0,0), new("Gold Ring",EquipmentSlot.Accessory,Rarity.Rare,3,3,2,5,5), new("Ruby Pendant",EquipmentSlot.Accessory,Rarity.Epic,5,4,3,10,10), new("Crown of Kings",EquipmentSlot.Accessory,Rarity.Legendary,8,6,5,20,15),
    };

    public override void _Ready()
    {
        Instance = this; _rng.Randomize(); ProcessMode = ProcessModeEnum.Always;
    }

    public void SelectCharacter(CharacterClass cls) => SelectedClass = cls;

    public void StartNewRun()
    {
        var chr = AllCharacters[SelectedClass];
        PlayerStats = new StatsData
        {
            Name = chr.Name,
            MaxHP = chr.BaseHP + chr.StartingArmor.BonusMaxHP + chr.StartingAccessory.BonusMaxHP,
            CurrentHP = chr.BaseHP + chr.StartingArmor.BonusMaxHP + chr.StartingAccessory.BonusMaxHP,
            MaxMP = chr.BaseMP + chr.StartingWeapon.BonusMaxMP + chr.StartingArmor.BonusMaxMP + chr.StartingAccessory.BonusMaxMP,
            CurrentMP = chr.BaseMP + chr.StartingWeapon.BonusMaxMP + chr.StartingArmor.BonusMaxMP + chr.StartingAccessory.BonusMaxMP,
            ATK = chr.BaseATK + chr.StartingWeapon.BonusATK + chr.StartingArmor.BonusATK + chr.StartingAccessory.BonusATK,
            DEF = chr.BaseDEF + chr.StartingArmor.BonusDEF + chr.StartingWeapon.BonusDEF + chr.StartingAccessory.BonusDEF,
            SPD = chr.BaseSPD + chr.StartingWeapon.BonusSPD + chr.StartingArmor.BonusSPD + chr.StartingAccessory.BonusSPD,
            Level = 1, XP = 0, Gold = 20
        };
        EquippedWeapon = chr.StartingWeapon; EquippedArmor = chr.StartingArmor; EquippedAccessory = chr.StartingAccessory;
        LearnedSkills = new List<SkillDef>();
        foreach (var s in chr.StartingSkills)
            if (AllSkills.ContainsKey(s)) LearnedSkills.Add(AllSkills[s]);
        Inventory = new Dictionary<string, int> { ["Potion"] = 3, ["Ether"] = 1 };
        EquipmentInventory = new List<EquipmentDef>();
        CurrentCityName = "Ravenhold"; UnlockedCities = new HashSet<string>(); BossesDefeated = new Dictionary<string, bool>();
        foreach (var kv in AllCities) if (kv.Value.UnlockedByDefault) UnlockedCities.Add(kv.Key);
        SecretBossBeatenInCurrentCity = false; MainBossBeatenInCurrentCity = false;
    }

    // === CITY ===
    public void EnterCity(string cityName) {
        if (!UnlockedCities.Contains(cityName)) return;
        CurrentCityName = cityName; SecretBossBeatenInCurrentCity = IsBossDefeated(cityName, false); MainBossBeatenInCurrentCity = IsBossDefeated(cityName, true);
        PlayerStats.CurrentHP = Mathf.Min(PlayerStats.CurrentHP + 20, GetTotalMaxHP()); PlayerStats.CurrentMP = Mathf.Min(PlayerStats.CurrentMP + 10, GetTotalMaxMP());
    }
    public void OnMainBossDefeated() {
        string key = $"{CurrentCityName}_main"; if (!BossesDefeated.ContainsKey(key)) BossesDefeated[key] = true; MainBossBeatenInCurrentCity = true;
        foreach (var c in AllCities[CurrentCityName].UnlocksCities) UnlockedCities.Add(c);
    }
    public void OnSecretBossDefeated() { string key = $"{CurrentCityName}_secret"; if (!BossesDefeated.ContainsKey(key)) BossesDefeated[key] = true; SecretBossBeatenInCurrentCity = true; }

    // === STATS ===
    public int GetTotalATK() => PlayerStats.ATK + (EquippedWeapon?.BonusATK??0) + (EquippedArmor?.BonusATK??0) + (EquippedAccessory?.BonusATK??0);
    public int GetTotalDEF() => PlayerStats.DEF + (EquippedWeapon?.BonusDEF??0) + (EquippedArmor?.BonusDEF??0) + (EquippedAccessory?.BonusDEF??0);
    public int GetTotalSPD() => PlayerStats.SPD + (EquippedWeapon?.BonusSPD??0) + (EquippedArmor?.BonusSPD??0) + (EquippedAccessory?.BonusSPD??0);
    public int GetTotalMaxHP() => PlayerStats.MaxHP + (EquippedWeapon?.BonusMaxHP??0) + (EquippedArmor?.BonusMaxHP??0) + (EquippedAccessory?.BonusMaxHP??0);
    public int GetTotalMaxMP() => PlayerStats.MaxMP + (EquippedWeapon?.BonusMaxMP??0) + (EquippedArmor?.BonusMaxMP??0) + (EquippedAccessory?.BonusMaxMP??0);

    // === LEVELING (Lv 1 → 25) ===
    public void AddXP(int amount)
    {
        if (PlayerStats.Level >= 25) { PlayerStats.XP = 0; return; }
        PlayerStats.XP += amount;
        int needed = XPToNext(PlayerStats.Level);
        while (PlayerStats.XP >= needed && PlayerStats.Level < 25)
        { PlayerStats.XP -= needed; LevelUp(); needed = XPToNext(PlayerStats.Level); }
        if (PlayerStats.Level >= 25) PlayerStats.XP = 0;
    }
    int XPToNext(int level) => (int)(60 * Mathf.Pow(1.35f, level - 1));

    void LevelUp()
    {
        var p = PlayerStats; var chr = AllCharacters[SelectedClass]; p.Level++;
        float hpS=1,mpS=1,atkS=1,defS=1,spdS=1;
        switch (SelectedClass) {
            case CharacterClass.Knight: hpS=1.2f;defS=1.3f;atkS=0.9f;break;
            case CharacterClass.Mage: mpS=1.4f;hpS=0.6f;atkS=0.7f;break;
            case CharacterClass.Rogue: spdS=1.4f;atkS=1.1f;hpS=0.7f;break;
            case CharacterClass.Paladin: hpS=1.0f;defS=1.1f;mpS=1.1f;break;
            case CharacterClass.Ranger: spdS=1.2f;atkS=1.1f;hpS=0.8f;break;
            case CharacterClass.Berserker: atkS=1.5f;hpS=1.1f;defS=0.5f;break;
            case CharacterClass.Gunslinger: spdS=1.2f;atkS=1.1f;hpS=0.9f;break;
        }
        p.MaxHP+=Mathf.RoundToInt(12*hpS);p.MaxMP+=Mathf.RoundToInt(6*mpS);
        p.ATK+=Mathf.RoundToInt(2*atkS);p.DEF+=Mathf.RoundToInt(2*defS);p.SPD+=Mathf.RoundToInt(1*spdS);
        p.CurrentHP=GetTotalMaxHP();p.CurrentMP=GetTotalMaxMP();

        // Skill unlocks by level
        void Learn(string name) { if (!LearnedSkills.Any(s=>s.Name==name) && AllSkills.ContainsKey(name)) LearnedSkills.Add(AllSkills[name]); }

        // Lv 3
        if (p.Level==3) {
            if (SelectedClass==CharacterClass.Mage) Learn("Blizzard");
            if (SelectedClass==CharacterClass.Berserker) Learn("War Cry");
            if (SelectedClass==CharacterClass.Ranger) Learn("Power Shot");
            if (SelectedClass==CharacterClass.Paladin) Learn("Barrier");
            if (SelectedClass==CharacterClass.Gunslinger) Learn("Fan the Hammer");
        }
        // Lv 5
        if (p.Level==5) {
            if (SelectedClass==CharacterClass.Knight) Learn("Power Bash");
            if (SelectedClass==CharacterClass.Rogue) Learn("Poison Blade");
        }
        // Lv 7
        if (p.Level==7) {
            if (SelectedClass==CharacterClass.Knight) Learn("Counter Strike");
            if (SelectedClass==CharacterClass.Mage) Learn("Mana Shield");
            if (SelectedClass==CharacterClass.Rogue) Learn("Shadow Step");
            if (SelectedClass==CharacterClass.Paladin) Learn("Divine Protection");
            if (SelectedClass==CharacterClass.Ranger) Learn("Rain of Arrows");
            if (SelectedClass==CharacterClass.Berserker) Learn("Bloodletting");
            if (SelectedClass==CharacterClass.Gunslinger) Learn("Headshot");
        }
        // Lv 12
        if (p.Level==12) {
            if (SelectedClass==CharacterClass.Knight) Learn("Holy Wall");
            if (SelectedClass==CharacterClass.Mage) Learn("Meteor");
            if (SelectedClass==CharacterClass.Rogue) Learn("Venom Strike");
            if (SelectedClass==CharacterClass.Paladin) Learn("Holy Judgment");
            if (SelectedClass==CharacterClass.Ranger) Learn("Eagle Eye");
            if (SelectedClass==CharacterClass.Berserker) Learn("Unstoppable");
            if (SelectedClass==CharacterClass.Gunslinger) Learn("Trick Shot");
        }
        // Lv 18
        if (p.Level==18) {
            if (SelectedClass==CharacterClass.Knight) Learn("Guillotine");
            if (SelectedClass==CharacterClass.Mage) Learn("Arcane Surge");
            if (SelectedClass==CharacterClass.Rogue) Learn("Death Sentence");
            if (SelectedClass==CharacterClass.Paladin) Learn("Resurrection");
            if (SelectedClass==CharacterClass.Ranger) Learn("Sniper Shot");
            if (SelectedClass==CharacterClass.Berserker) Learn("Ragnarok");
            if (SelectedClass==CharacterClass.Gunslinger) Learn("Bullet Storm");
        }
    }

    // === ITEMS ===
    public void UseItem(string n) { if (Inventory.ContainsKey(n) && Inventory[n] > 0) Inventory[n]--; }
    public void AddItem(string n, int c=1) { if (!Inventory.ContainsKey(n)) Inventory[n]=0; Inventory[n]+=c; }
    public void AddGold(int a) => PlayerStats.Gold += a;

    // === LOOT EQUIPMENT ===
    public EquipmentDef RollLoot(int tier) {
        var pool = new List<(EquipmentDef item, float w)>();
        foreach (var eq in AllEquipment) {
            if (eq.RestrictedTo.HasValue && eq.RestrictedTo.Value != SelectedClass) continue;
            float tr = eq.Rarity switch { Rarity.Common=>0, Rarity.Uncommon=>1, Rarity.Rare=>3, Rarity.Epic=>5, Rarity.Legendary=>7, _=>0 };
            if (tier < tr) continue;
            pool.Add((eq, eq.Rarity switch { Rarity.Common=>0.4f, Rarity.Uncommon=>0.3f, Rarity.Rare=>0.18f, Rarity.Epic=>0.09f, Rarity.Legendary=>0.03f, _=>0.5f }));
        }
        if (pool.Count==0) return null;
        float tot=pool.Sum(x=>x.w), roll=_rng.RandfRange(0,tot), cum=0;
        foreach (var (item,w) in pool) { cum+=w; if (roll<=cum) return item; }
        return pool[^1].item;
    }
    public void AddEquipment(EquipmentDef eq) {
        bool equip = eq.Slot switch { EquipmentSlot.Weapon=>EquippedWeapon==null||RarityScore(eq)>RarityScore(EquippedWeapon), EquipmentSlot.Armor=>EquippedArmor==null||RarityScore(eq)>RarityScore(EquippedArmor), EquipmentSlot.Accessory=>EquippedAccessory==null||RarityScore(eq)>RarityScore(EquippedAccessory), _=>false };
        if (equip) { EquipmentDef old=null; switch(eq.Slot) { case EquipmentSlot.Weapon: old=EquippedWeapon; EquippedWeapon=eq; break; case EquipmentSlot.Armor: old=EquippedArmor; EquippedArmor=eq; break; case EquipmentSlot.Accessory: old=EquippedAccessory; EquippedAccessory=eq; break; } if (old!=null) EquipmentInventory.Add(old); }
        else EquipmentInventory.Add(eq);
    }
    int RarityScore(EquipmentDef eq) => (int)eq.Rarity;

    // === LOOT SKILLS ===
    // Call after boss defeat. Returns skill name or null.
    public string RollLootSkill()
    {
        var classSkills = LootSkills[SelectedClass];
        if (classSkills == null || classSkills.Length == 0) return null;
        // 40% chance for a loot skill drop
        if (_rng.Randf() > 0.4f) return null;
        string skillName = classSkills[_rng.RandiRange(0, classSkills.Length - 1)];
        // Don't drop if already learned
        if (LearnedSkills.Any(s => s.Name == skillName)) return null;
        return skillName;
    }

    public void LearnLootSkill(string skillName)
    {
        if (!LearnedSkills.Any(s => s.Name == skillName) && AllSkills.ContainsKey(skillName))
            LearnedSkills.Add(AllSkills[skillName]);
    }

    // === COMBAT STATE ===
    int _tempHP, _tempMP, _tempGold;
    public void SaveToTemp() { _tempHP=PlayerStats.CurrentHP; _tempMP=PlayerStats.CurrentMP; _tempGold=PlayerStats.Gold; }
    public void RestoreFromTemp() { PlayerStats.CurrentHP=_tempHP; PlayerStats.CurrentMP=_tempMP; PlayerStats.Gold=_tempGold; }
}
