using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Rouge;

public partial class CombatUI : Control
{
    // UI Nodes
    private VBoxContainer _actionButtons;
    private VBoxContainer _skillButtons;
    private VBoxContainer _itemButtons;
    private GridContainer _enemyTargets;
    private RichTextLabel _battleLog;
    private Label _playerInfo;
    private Label _enemyInfo;
    private Label _turnLabel;
    private Panel _actionPanel;
    private Panel _skillPanel;
    private Panel _itemPanel;
    private Panel _enemyTargetPanel;

    private List<Button> _enemyButtons = new();
    private string _pendingSkillName = "";
    private string _pendingItemName = "";

    // Dark Bloodborne theme colors
    static readonly Color DarkBg = new Color(0.12f, 0.06f, 0.08f);
    static readonly Color DarkHover = new Color(0.25f, 0.08f, 0.1f);
    static readonly Color DarkPress = new Color(0.35f, 0.05f, 0.05f);
    static readonly Color TextColor = new Color(0.82f, 0.78f, 0.74f);
    static readonly Color TextDisabled = new Color(0.4f, 0.35f, 0.32f);

    void StyleButton(Button btn)
    {
        btn.AddThemeColorOverride("font_color", TextColor);
        btn.AddThemeColorOverride("font_disabled_color", TextDisabled);
        btn.AddThemeColorOverride("font_hover_color", new Color(0.95f, 0.9f, 0.85f));
        btn.AddThemeColorOverride("font_pressed_color", new Color(0.9f, 0.1f, 0.1f));
        btn.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = DarkBg, BorderColor = new Color(0.3f, 0.1f, 0.1f), BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1, CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2 });
        btn.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = DarkHover, BorderColor = new Color(0.5f, 0.15f, 0.15f), BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1, CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2 });
        btn.AddThemeStyleboxOverride("pressed", new StyleBoxFlat { BgColor = DarkPress, BorderColor = new Color(0.7f, 0.05f, 0.05f), BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1, CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2 });
        btn.AddThemeStyleboxOverride("disabled", new StyleBoxFlat { BgColor = new Color(0.06f, 0.03f, 0.04f), BorderColor = new Color(0.15f, 0.05f, 0.05f), BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1 });
        btn.AddThemeFontSizeOverride("font_size", 14);
    }

    public override void _Ready()
    {
        // Get references
        _actionButtons = GetNode<VBoxContainer>("ActionPanel/ActionButtons");
        _skillButtons = GetNode<VBoxContainer>("SkillPanel/SkillButtons");
        _itemButtons = GetNode<VBoxContainer>("ItemPanel/ItemButtons");
        _enemyTargets = GetNode<GridContainer>("EnemyTargetPanel/EnemyGrid");
        _battleLog = GetNode<RichTextLabel>("BattleLogPanel/RichTextLabel");
        _playerInfo = GetNode<Label>("PlayerInfo");
        _enemyInfo = GetNode<Label>("EnemyInfo");
        _turnLabel = GetNode<Label>("TurnLabel");
        _actionPanel = GetNode<Panel>("ActionPanel");
        _skillPanel = GetNode<Panel>("SkillPanel");
        _itemPanel = GetNode<Panel>("ItemPanel");
        _enemyTargetPanel = GetNode<Panel>("EnemyTargetPanel");

        // Connect to CombatManager signals
        var cm = CombatManager.Instance;
        cm.BattleLogUpdated += OnBattleLogUpdated;
        cm.BattleStateChanged += OnBattleStateChanged;
        cm.EnemyDamaged += OnEnemyDamaged;
        cm.PlayerDamaged += OnPlayerDamaged;
        cm.PlayerHealed += OnPlayerHealed;
        cm.BattleEnded += OnBattleEnded;

        // Assign UI reference
        cm.UI = this;

        // Build action buttons
        SetupActionButtons();

        // Start the battle using the enemy data from scene metadata
        StartBattleFromScene();

        ShowActionPanel();
    }

    void StartBattleFromScene()
    {
        // Read enemy data from GameManager metadata (set by EnemyOverworld before transition)
        var gm = GameManager.Instance;
        var enemyName = gm.GetMeta("combat_enemy_name", "Goblin").AsString();
        var enemyHP = gm.GetMeta("combat_enemy_hp", 40).AsInt32();
        var enemyATK = gm.GetMeta("combat_enemy_atk", 7).AsInt32();
        var enemyDEF = gm.GetMeta("combat_enemy_def", 3).AsInt32();
        var enemySPD = gm.GetMeta("combat_enemy_spd", 5).AsInt32();
        var enemyXP = gm.GetMeta("combat_enemy_xp", 20).AsInt32();
        var enemyGold = gm.GetMeta("combat_enemy_gold", 10).AsInt32();
        var enemyColor = gm.GetMeta("combat_enemy_color", "red").AsString();
        var enemyWeakness = gm.GetMeta("combat_enemy_weakness", "").AsString();

        var enemy = new EnemyDef(enemyName, enemyHP, enemyATK, enemyDEF, enemySPD, enemyXP, enemyGold, enemyColor, enemyWeakness);
        CombatManager.Instance.StartBattle(new List<EnemyDef> { enemy });
    }

    void SetupActionButtons()
    {
        // Clear existing
        foreach (var c in _actionButtons.GetChildren())
            c.QueueFree();

        var actions = new (string, CombatAction)[]
        {
            ("Attack", CombatAction.Attack),
            ("Skill", CombatAction.Skill),
            ("Items", CombatAction.Item),
            ("Defend", CombatAction.Defend),
            ("Flee", CombatAction.Flee),
        };

        foreach (var (label, action) in actions)
        {
            var btn = new Button();
            btn.Text = label;
            btn.CustomMinimumSize = new Vector2(160, 40);
            StyleButton(btn);
            if (action == CombatAction.Skill || action == CombatAction.Item)
            {
                btn.Pressed += () => ShowSubPanel(action);
            }
            else if (action == CombatAction.Attack)
            {
                btn.Pressed += () => ShowEnemySelection(CombatAction.Attack);
            }
            else
            {
                btn.Pressed += () => CombatManager.Instance.OnPlayerAction(action);
            }
            _actionButtons.AddChild(btn);
        }
    }

    void ShowSubPanel(CombatAction action)
    {
        _actionPanel.Hide();
        _skillPanel.Visible = false;
        _itemPanel.Visible = false;

        if (action == CombatAction.Skill)
            ShowSkills();
        else
            ShowItems();
    }

    void ShowSkills()
    {
        _skillPanel.Show();
        foreach (var c in _skillButtons.GetChildren())
            c.QueueFree();

        var skills = GameManager.Instance.LearnedSkills;
        foreach (var skill in skills)
        {
            var btn = new Button();
            var p = GameManager.Instance.PlayerStats;
            bool canUse = p.CurrentMP >= skill.MPCost;
            btn.Text = $"{skill.Name} (MP: {skill.MPCost}){(canUse ? "" : " [NO MP]")}";
            btn.Disabled = !canUse;
            btn.CustomMinimumSize = new Vector2(200, 40);

            string skillName = skill.Name;
            btn.Pressed += () =>
            {
                _pendingSkillName = skillName;
                ShowEnemySelection(CombatAction.Skill);
            };
            StyleButton(btn);

            // Tooltip-ish: hover could show desc
            _skillButtons.AddChild(btn);
        }

        // Back button
        var back = new Button();
        back.Text = "Back";
        back.Pressed += ShowActionPanel;
        StyleButton(back);
        _skillButtons.AddChild(back);
    }

    void ShowItems()
    {
        _itemPanel.Show();
        foreach (var c in _itemButtons.GetChildren())
            c.QueueFree();

        var inv = GameManager.Instance.Inventory;
        foreach (var kv in inv)
        {
            if (kv.Value <= 0) continue;
            var item = GameManager.AllItems[kv.Key];
            var btn = new Button();
            btn.Text = $"{item.Name} x{kv.Value}";
            btn.CustomMinimumSize = new Vector2(200, 40);
            StyleButton(btn);

            string itemName = kv.Key;

            // Items that target enemies vs self
            if (item.Type == ItemType.HP || item.Type == ItemType.MP || item.Type == ItemType.Revive)
            {
                btn.Pressed += () =>
                {
                    CombatManager.Instance.OnPlayerAction(CombatAction.Item, 0, "", itemName);
                    ShowActionPanel();
                };
            }
            else
            {
                btn.Pressed += () =>
                {
                    _pendingItemName = itemName;
                    ShowEnemySelection(CombatAction.Item);
                };
            }

            _itemButtons.AddChild(btn);
        }

        // Back button
        var back = new Button();
        back.Text = "Back";
        back.Pressed += ShowActionPanel;
        StyleButton(back);
        _itemButtons.AddChild(back);
    }

    void ShowEnemySelection(CombatAction action)
    {
        _enemyTargetPanel.Show();
        _actionPanel.Hide();
        _skillPanel.Hide();
        _itemPanel.Hide();

        foreach (var c in _enemyTargets.GetChildren())
            c.QueueFree();
        _enemyButtons.Clear();

        for (int i = 0; i < CombatManager.Instance.Enemies.Count; i++)
        {
            var enemy = CombatManager.Instance.Enemies[i];
            if (!enemy.IsAlive) continue;

            int idx = i;
            var btn = new Button();
            btn.Text = $"{enemy.Name} (HP: {enemy.CurrentHP}/{enemy.MaxHP})";
            btn.CustomMinimumSize = new Vector2(200, 50);
            StyleButton(btn);

            string skillName = _pendingSkillName;
            string itemName = _pendingItemName;

            btn.Pressed += () =>
            {
                _enemyTargetPanel.Hide();
                CombatManager.Instance.OnPlayerAction(action, idx, skillName, itemName);
                _pendingSkillName = "";
                _pendingItemName = "";
            };

            _enemyTargets.AddChild(btn);
            _enemyButtons.Add(btn);
        }

        // Back button
        var back = new Button();
        back.Text = "Cancel";
        back.Pressed += () =>
        {
            _enemyTargetPanel.Hide();
            _pendingSkillName = "";
            _pendingItemName = "";
            ShowActionPanel();
        };
        StyleButton(back);
        _enemyTargets.AddChild(back);
    }

    void ShowActionPanel()
    {
        _actionPanel.Show();
        _skillPanel.Hide();
        _itemPanel.Hide();
        _enemyTargetPanel.Hide();
    }

    void OnBattleLogUpdated(string msg)
    {
        _battleLog.Text += msg + "\n";
        // Scroll to bottom
        _battleLog.ScrollToLine(int.MaxValue);
    }

    void OnBattleStateChanged(int state)
    {
        BattleState bs = (BattleState)state;
        bool canAct = bs == BattleState.PlayerTurn;
        _actionPanel.Visible = canAct;
        _skillPanel.Visible = false;
        _itemPanel.Visible = false;
        _enemyTargetPanel.Visible = false;

        if (canAct && IsInstanceValid(_actionButtons))
            ShowActionPanel();
    }

    void OnEnemyDamaged(int index, int damage, bool weak)
    {
        UpdateUI();
        if (weak)
        {
            _enemyInfo.Modulate = new Color(1, 0.8f, 0.2f);
            var timer = GetTree().CreateTimer(0.2f);
            timer.Timeout += () => { if (IsInstanceValid(_enemyInfo)) _enemyInfo.Modulate = Colors.White; };
        }
    }

    void OnPlayerDamaged(int damage)
    {
        UpdateUI();
        // Shake effect
        _playerInfo.Modulate = new Color(1, 0.5f, 0.5f);
        var timer = GetTree().CreateTimer(0.15f);
        timer.Timeout += () =>
        {
            if (IsInstanceValid(_playerInfo))
                _playerInfo.Modulate = Colors.White;
        };
    }

    void OnPlayerHealed(int amount)
    {
        UpdateUI();
        _playerInfo.Modulate = new Color(0.5f, 1, 0.5f);
        var timer = GetTree().CreateTimer(0.15f);
        timer.Timeout += () =>
        {
            if (IsInstanceValid(_playerInfo))
                _playerInfo.Modulate = Colors.White;
        };
    }

    void OnBattleEnded(bool won)
    {
        var timer = GetTree().CreateTimer(1.5f);
        timer.Timeout += () =>
        {
            // Remove the overlay
            QueueFree();

            if (won)
            {
                var gm = GameManager.Instance;
                if (gm.HasMeta("combat_enemy_node"))
                {
                    var enemyNode = gm.GetMeta("combat_enemy_node").As<Node2D>();
                    if (IsInstanceValid(enemyNode))
                        enemyNode.QueueFree();
                    gm.RemoveMeta("combat_enemy_node");
                }
                var player = GetTree().Root.FindChild("Player", true, false) as Player;
                if (player != null) player.Enable();
            }
            else
            {
                GetTree().ChangeSceneToFile("res://scenes/GameOverScene.tscn");
            }
        };
    }

    public void UpdateUI()
    {
        var gm = GameManager.Instance;
        var p = gm.PlayerStats;

        string gear = "";
        if (gm.EquippedWeapon != null) gear += $"Weapon: [color={gm.EquippedWeapon.RarityColor()}]{gm.EquippedWeapon.Name}[/color] (+{gm.EquippedWeapon.BonusATK}ATK)\n";
        if (gm.EquippedArmor != null) gear += $"Armor: [color={gm.EquippedArmor.RarityColor()}]{gm.EquippedArmor.Name}[/color] (+{gm.EquippedArmor.BonusDEF}DEF)\n";
        if (gm.EquippedAccessory != null) gear += $"Acc: [color={gm.EquippedAccessory.RarityColor()}]{gm.EquippedAccessory.Name}[/color]\n";

        _playerInfo.Text = $"{p.Name}  LV.{p.Level}\n" +
            $"HP: {p.CurrentHP}/{gm.GetTotalMaxHP()}\n" +
            $"MP: {p.CurrentMP}/{gm.GetTotalMaxMP()}\n" +
            $"ATK:{gm.GetTotalATK()} DEF:{gm.GetTotalDEF()} SPD:{gm.GetTotalSPD()}\n" +
            $"Gold: {p.Gold}\n{gear}" +
            $"[color=#444444]W/↑↓ Nav  Enter/Space=OK  Esc=Back[/color]";

        string enemyText = "Enemies:\n";
        foreach (var e in CombatManager.Instance.Enemies)
        {
            string hpBar = "";
            float pct = (float)e.CurrentHP / e.MaxHP;
            int bars = Mathf.RoundToInt(pct * 10);
            hpBar = new string('█', bars) + new string('░', 10 - bars);
            string weakTag = string.IsNullOrEmpty(e.Weakness) ? "" : $" [weak: {e.Weakness}]";
            enemyText += $"{e.Name}: {e.CurrentHP}/{e.MaxHP}{weakTag}\n{hpBar}\n";
        }
        _enemyInfo.Text = enemyText;

        _turnLabel.Text = $"{gm.GetCurrentCityDef().Name} - Turn {CombatManager.Instance.TurnNumber}";
        UpdateEnemyButtons();
    }

    void UpdateEnemyButtons()
    {
        for (int i = 0; i < _enemyButtons.Count && i < CombatManager.Instance.Enemies.Count; i++)
        {
            var e = CombatManager.Instance.Enemies[i];
            _enemyButtons[i].Text = $"{e.Name} (HP: {e.CurrentHP}/{e.MaxHP})";
            _enemyButtons[i].Disabled = !e.IsAlive;
        }
    }
}
