using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Rouge;

public partial class CombatManager : Node
{
    public static CombatManager Instance { get; private set; }

    [Signal] public delegate void BattleLogUpdatedEventHandler(string message);
    [Signal] public delegate void BattleStateChangedEventHandler(int newState);
    [Signal] public delegate void EnemyDamagedEventHandler(int index, int damage, bool weak);
    [Signal] public delegate void PlayerDamagedEventHandler(int damage);
    [Signal] public delegate void PlayerHealedEventHandler(int amount);
    [Signal] public delegate void BattleEndedEventHandler(bool won);

    public BattleState State { get; private set; } = BattleState.PlayerTurn;
    public List<EnemyDef> Enemies { get; private set; } = new();
    public int TurnNumber { get; private set; } = 1;
    public CombatUI UI;

    private bool _defUp, _atkUp, _raging;
    private int _rageTurns;
    private bool _doubleNextMagic, _evading, _countering, _skipEnemyTurn;
    private bool _battleInitialized = false;
    private EquipmentDef _lootDrop;
    private string _lootSkillName = "";

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartBattle(List<EnemyDef> enemies)
    {
        if (_battleInitialized) return;
        _battleInitialized = true;
        Enemies = enemies;
        foreach (var e in Enemies) e.CurrentHP = e.MaxHP;

        _defUp = _atkUp = _raging = false; _rageTurns = 0;
        _doubleNextMagic = _evading = _countering = _skipEnemyTurn = false;
        State = BattleState.PlayerTurn;
        TurnNumber = 1;

        int tier = GameManager.Instance.GetCurrentCityDef().DifficultyTier;
        _lootDrop = GD.Randf() < 0.4f ? GameManager.Instance.RollLoot(tier) : null;
        _lootSkillName = "";
        BattleLog($"Battle start! {enemies.Count} enemy(ies) appear!");

        var p = GameManager.Instance.PlayerStats;
        if (p.CurrentHP <= 0)
        {
            p.CurrentHP = Mathf.Max(1, GameManager.Instance.GetTotalMaxHP() / 4);
            BattleLog("You barely hang on to consciousness...");
        }

        EmitSignal(SignalName.BattleStateChanged, (int)State);
        if (UI != null) UI.UpdateUI();
    }

    void BattleLog(string msg) => EmitSignal(SignalName.BattleLogUpdated, msg);

    public void OnPlayerAction(CombatAction action, int targetIndex = 0, string skillName = "", string itemName = "")
    {
        if (State != BattleState.PlayerTurn) return;

        var gm = GameManager.Instance;
        var p = gm.PlayerStats;
        int atk = gm.GetTotalATK();
        int def = gm.GetTotalDEF();
        int spd = gm.GetTotalSPD();

        if (_atkUp) atk = (int)(atk * 1.4f);
        if (_raging) atk = (int)(atk * 1.8f);

        switch (action)
        {
            case CombatAction.Attack:
            {
                int target = GetValidTarget(targetIndex);
                int damage = CalcDmg(atk, Enemies[target].DEF, 1.0f);
                bool weak = Enemies[target].Weakness == "Attack";
                if (weak) damage = (int)(damage * 1.8f);
                HitEnemy(target, damage, weak);
                BattleLog($"{p.Name} attacks {Enemies[target].Name}! {damage} dmg{(weak ? " [WEAK!]" : "")}");
                break;
            }
            case CombatAction.Skill:
            {
                UseSkill(skillName, targetIndex, atk, def);
                break;
            }
            case CombatAction.Item:
            {
                UseItem(itemName);
                break;
            }
            case CombatAction.Defend:
            {
                _defUp = true;
                BattleLog($"{p.Name} guards! DEF doubled this turn.");
                break;
            }
            case CombatAction.Flee:
            {
                float fleeChance = 0.5f + (spd * 0.02f);
                if (GD.Randf() < fleeChance) { BattleLog("You fled successfully!"); EndBattle(false); return; }
                else BattleLog("Failed to flee!");
                break;
            }
        }

        if (Enemies.All(e => !e.IsAlive)) { State = BattleState.Won; EndBattle(true); return; }

        if (_skipEnemyTurn)
        {
            _skipEnemyTurn = false;
            TurnNumber++;
            State = BattleState.PlayerTurn;
            BattleLog($"--- Turn {TurnNumber} ---");
            EmitSignal(SignalName.BattleStateChanged, (int)State);
            if (UI != null) UI.UpdateUI();
            return;
        }

        if (_raging && ++_rageTurns >= 2) { _raging = false; BattleLog("Rage subsides..."); }

        State = BattleState.EnemyTurn;
        EmitSignal(SignalName.BattleStateChanged, (int)State);
        BattleLog("--- Enemy Turn ---");
        var timer = GetTree().CreateTimer(0.8f);
        timer.Timeout += ProcessEnemyTurn;
    }

    void UseSkill(string skillName, int targetIndex, int atk, int def)
    {
        var gm = GameManager.Instance;
        var p = gm.PlayerStats;
        var skill = gm.LearnedSkills.FirstOrDefault(s => s.Name == skillName);
        if (skill == null) { BattleLog("Skill not learned!"); return; }
        if (p.CurrentMP < skill.MPCost) { BattleLog("Not enough MP!"); return; }
        p.CurrentMP -= skill.MPCost;

        if (skill.SelfDamageHP > 0)
        {
            int d = Mathf.Min(skill.SelfDamageHP, p.CurrentHP - 1);
            p.CurrentHP -= d;
            EmitSignal(SignalName.PlayerDamaged, d);
            BattleLog($"Self-damage! -{d} HP");
        }

        if (skill.BuffEffect == "evade") { _evading = true; BattleLog($"{p.Name} readies to dodge!"); }
        if (skill.BuffEffect == "counter") { _countering = true; BattleLog($"{p.Name} prepares a counter!"); }
        if (skill.BuffEffect == "double_next_magic") { _doubleNextMagic = true; BattleLog($"{p.Name} charges arcane power!"); }

        if (skill.BuffEffect == "mana_shield")
        {
            int converted = Mathf.Min(p.CurrentMP, 30);
            int heal = converted * 2;
            p.CurrentMP -= converted;
            p.CurrentHP = Mathf.Min(gm.GetTotalMaxHP(), p.CurrentHP + heal);
            EmitSignal(SignalName.PlayerHealed, heal);
            BattleLog($"Mana Shield! Converted {converted} MP into {heal} HP!");
        }

        if (skill.SkipEnemyTurn)
        {
            _skipEnemyTurn = true;
            BattleLog($"{p.Name} freezes time! Enemies skip their turn!");
            return;
        }

        switch (skill.Type)
        {
            case SkillType.Attack:
            case SkillType.Magic:
            {
                if (!HasValidEnemyTarget()) return;
                for (int h = 0; h < skill.Hits; h++)
                {
                    int target = GetValidTarget(targetIndex);
                    int effDef = skill.IgnoresDEF ? 0 : Enemies[target].DEF;
                    float pow = skill.Power;
                    if (skill.IsExecute)
                    {
                        if ((float)Enemies[target].CurrentHP / Enemies[target].MaxHP < 0.25f) { pow *= 3.0f; BattleLog("EXECUTE!"); }
                    }
                    int dmg = CalcDmg(atk, effDef, pow);
                    if (_doubleNextMagic && skill.Type == SkillType.Magic) { dmg *= 2; _doubleNextMagic = false; }
                    bool weak = Enemies[target].Weakness == skill.Name;
                    if (weak) dmg = (int)(dmg * 2.0f);
                    HitEnemy(target, dmg, weak);
                    string extra = skill.IgnoresDEF ? " [ARMOR PIERCING]" : "";
                    if (h == 0) BattleLog($"{p.Name} uses {skill.Name}! {dmg} dmg{(skill.Hits > 1 ? " x" + skill.Hits : "")}{extra}{(weak ? " [WEAK!]" : "")}");
                }
                if (skill.StatusEffect == "stun") BattleLog("Enemy is stunned!");
                if (skill.StatusEffect == "poison") BattleLog("Enemy is poisoned!");
                break;
            }
            case SkillType.Heal:
            {
                int h = skill.HealAmount;
                p.CurrentHP = Mathf.Min(gm.GetTotalMaxHP(), p.CurrentHP + h);
                EmitSignal(SignalName.PlayerHealed, h);
                BattleLog($"{p.Name} uses {skill.Name}! +{h} HP!");
                if (skill.StatusEffect == "def_up") { _defUp = true; BattleLog("DEF rises!"); }
                break;
            }
            case SkillType.Buff:
            {
                if (skill.StatusEffect == "def_up") { _defUp = true; BattleLog($"{p.Name} uses {skill.Name}! DEF up!"); }
                else if (skill.StatusEffect == "rage") { _raging = true; _rageTurns = 0; BattleLog($"{p.Name} goes into a RAGE!"); }
                else BattleLog($"{p.Name} uses {skill.Name}!");
                break;
            }
            case SkillType.Debuff:
            {
                if (skill.StatusEffect == "stun") BattleLog($"{p.Name} uses {skill.Name}! Enemies stunned!");
                else BattleLog($"{p.Name} uses {skill.Name}!");
                break;
            }
        }
    }

    void UseItem(string itemName)
    {
        var gm = GameManager.Instance;
        var p = gm.PlayerStats;
        if (!gm.Inventory.ContainsKey(itemName) || gm.Inventory[itemName] <= 0) { BattleLog("No items left!"); return; }

        var item = GameManager.AllItems[itemName];
        gm.UseItem(itemName);

        switch (item.Type)
        {
            case ItemType.HP:
                p.CurrentHP = Mathf.Min(gm.GetTotalMaxHP(), p.CurrentHP + item.Value);
                EmitSignal(SignalName.PlayerHealed, item.Value);
                BattleLog($"Used {itemName}! +{item.Value} HP!"); break;
            case ItemType.MP:
                p.CurrentMP = Mathf.Min(gm.GetTotalMaxMP(), p.CurrentMP + item.Value);
                BattleLog($"Used {itemName}! +{item.Value} MP!"); break;
            case ItemType.Revive:
                p.CurrentHP = Mathf.Min(gm.GetTotalMaxHP(), p.CurrentHP + item.Value);
                if (p.CurrentHP <= 0) p.CurrentHP = item.Value;
                EmitSignal(SignalName.PlayerHealed, item.Value);
                BattleLog($"Used {itemName}! +{item.Value} HP!"); break;
        }
    }

    void ProcessEnemyTurn()
    {
        var gm = GameManager.Instance;
        var p = gm.PlayerStats;
        int def = gm.GetTotalDEF();
        if (_defUp) def *= 2;

        foreach (var enemy in Enemies)
        {
            if (!enemy.IsAlive) continue;
            int baseDmg = Mathf.Max(1, enemy.ATK - def / 2);
            int dmg = Mathf.Max(1, baseDmg + (int)(GD.Randf() * 6 - 3));
            p.CurrentHP -= dmg;
            EmitSignal(SignalName.PlayerDamaged, dmg);
            BattleLog($"{enemy.Name} attacks! {dmg} damage!");
            if (p.CurrentHP <= 0)
            {
                p.CurrentHP = 0; State = BattleState.Lost;
                EmitSignal(SignalName.BattleStateChanged, (int)State);
                var t = GetTree().CreateTimer(0.5f);
                t.Timeout += () => EndBattle(false);
                return;
            }
        }

        _defUp = false; TurnNumber++; State = BattleState.PlayerTurn;
        BattleLog($"--- Turn {TurnNumber} ---");
        EmitSignal(SignalName.BattleStateChanged, (int)State);
        if (UI != null) UI.UpdateUI();
    }

    void EndBattle(bool won)
    {
        State = won ? BattleState.Won : BattleState.Lost;
        var gm = GameManager.Instance;
        var p = gm.PlayerStats;

        if (won)
        {
            int xp = 0, gold = 0;
            foreach (var e in Enemies) { xp += e.XP; gold += e.Gold; if (GD.Randf() < 0.3f) gm.AddItem("Potion"); if (GD.Randf() < 0.1f) gm.AddItem("Ether"); }
            gm.AddXP(xp); gm.AddGold(gold);
            BattleLog($"Victory! +{xp} XP, +{gold} Gold!");

            if (_lootDrop != null) { gm.AddEquipment(_lootDrop); BattleLog($"Loot: {_lootDrop.Name} ({_lootDrop.Rarity} {_lootDrop.Slot})"); }

            bool boss = gm.HasMeta("combat_is_boss") && (bool)gm.GetMeta("combat_is_boss");
            if (boss) { string s = gm.RollLootSkill(); if (s != null) { gm.LearnLootSkill(s); _lootSkillName = s; BattleLog($"Skill learned: {s}!"); } }
        }
        else BattleLog("Defeated...");

        EmitSignal(SignalName.BattleEnded, won);
    }

    int CalcDmg(int atk, int edef, float power)
    {
        float b = atk * power * 1.5f;
        float r = edef * 0.4f;
        return Mathf.Max(1, Mathf.RoundToInt(b - r + (GD.Randf() * 4 - 2)));
    }

    int GetValidTarget(int preferred)
    {
        for (int i = preferred; i < Enemies.Count; i++) if (Enemies[i].IsAlive) return i;
        for (int i = 0; i < preferred; i++) if (Enemies[i].IsAlive) return i;
        return 0;
    }

    bool HasValidEnemyTarget() { foreach (var e in Enemies) if (e.IsAlive) return true; return false; }

    void HitEnemy(int idx, int dmg, bool weak)
    {
        Enemies[idx].CurrentHP = Mathf.Max(0, Enemies[idx].CurrentHP - dmg);
        EmitSignal(SignalName.EnemyDamaged, idx, dmg, weak);
    }
}
