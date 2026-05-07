using Godot;

namespace Rouge;

public partial class EnemyOverworld : CharacterBody2D
{
    private Area2D _area;
    private bool _inBattle;
    private static PackedScene _combatSceneCache;

    [Export] public float PatrolSpeed = 40f;
    [Export] public float PatrolRange = 64f;

    private Vector2 _startPos;
    private int _dir = 1;
    private bool _isBoss;

    public override void _Ready()
    {
        _area = GetNode<Area2D>("DetectionArea");
        _inBattle = false;
        _startPos = Position;
        _area.BodyEntered += OnBodyEntered;

        _isBoss = HasMeta("enemy_is_boss") && (bool)GetMeta("enemy_is_boss");

        // Boss enemies have bigger detection
        if (_isBoss)
        {
            var shape = _area.GetNode<CollisionShape2D>("CollisionShape2D");
            if (shape != null && shape.Shape is RectangleShape2D rect)
                rect.Size = new Vector2(120, 120);
        }

        if (_combatSceneCache == null)
            _combatSceneCache = GD.Load<PackedScene>("res://scenes/CombatScene.tscn");
    }

    void OnBodyEntered(Node body)
    {
        if (_inBattle) return;
        if (body is Player player)
        {
            _inBattle = true;
            string name = GetMeta("enemy_def_name", "Goblin").AsString();
            GD.Print($"ENCOUNTER! vs {name}{(HasMeta("enemy_is_boss") && (bool)GetMeta("enemy_is_boss") ? " (BOSS!)" : "")}");
            StartCombat(player);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 pos = Position;
        pos.X += _dir * PatrolSpeed * (float)delta;
        if (Mathf.Abs(pos.X - _startPos.X) > PatrolRange)
        {
            _dir *= -1;
            pos.X = _startPos.X + _dir * PatrolRange * 0.9f;
        }
        Position = pos;
    }

    void StartCombat(Player player)
    {
        GameManager.Instance.SaveToTemp();
        player.Disable();

        var gm = GameManager.Instance;
        gm.SetMeta("combat_enemy_name", GetMeta("enemy_def_name", "Goblin").AsString());
        gm.SetMeta("combat_enemy_hp", GetMeta("enemy_def_hp", 40).AsInt32());
        gm.SetMeta("combat_enemy_atk", GetMeta("enemy_def_atk", 7).AsInt32());
        gm.SetMeta("combat_enemy_def", GetMeta("enemy_def_def", 3).AsInt32());
        gm.SetMeta("combat_enemy_spd", GetMeta("enemy_def_spd", 5).AsInt32());
        gm.SetMeta("combat_enemy_xp", GetMeta("enemy_def_xp", 20).AsInt32());
        gm.SetMeta("combat_enemy_gold", GetMeta("enemy_def_gold", 10).AsInt32());
        gm.SetMeta("combat_enemy_color", GetMeta("enemy_def_color", "red").AsString());
        gm.SetMeta("combat_enemy_weakness", GetMeta("enemy_def_weakness", "").AsString());
        gm.SetMeta("combat_is_boss", _isBoss);
        gm.SetMeta("combat_enemy_node", this);

        var combatOverlay = _combatSceneCache.Instantiate<Control>();
        combatOverlay.Name = "CombatOverlay";
        GetTree().Root.AddChild(combatOverlay);
    }
}
