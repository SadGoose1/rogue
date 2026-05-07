using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Rouge;

public partial class LevelGenerator : Node2D
{
    private PackedScene _playerScene;
    private PackedScene _enemyOverworldScene;

    private Player _player;
    private CameraController _camera;
    private Node _roomRoot;
    private RandomNumberGenerator _rng = new();

    const int SCREEN_W = 1280;
    const int SCREEN_H = 720;

    private List<RoomData> _rooms = new();
    private int _currentRoomIndex;
    private int _secretBossRoomIndex = -1;
    private bool _secretBossFound;

    struct RoomData
    {
        public Rect2 Rect;
        public List<Rect2> Platforms;
        public Vector2 PlayerStart;
        public List<Vector2> EnemySpawns;
        public List<(string name, int hp, int atk, int def, int spd, int xp, int gold, string color, string weakness)> EnemyDefs;
        public Vector2 ExitPos;
        public bool IsBossRoom;
        public bool IsSecretBossRoom;
        public int Enemies;
    }

    public override void _Ready()
    {
        _rng.Randomize();
        _playerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
        _enemyOverworldScene = GD.Load<PackedScene>("res://scenes/EnemyOverworld.tscn");
        SpawnPlayer();
        _camera = GetNode<CameraController>("CameraController");
        GenerateCity();
    }

    void SpawnPlayer()
    {
        _player = _playerScene.Instantiate<Player>();
        AddChild(_player);
    }

    public void GenerateCity()
    {
        if (_roomRoot != null) _roomRoot.QueueFree();
        _roomRoot = new Node2D(); _roomRoot.Name = "Rooms"; AddChild(_roomRoot);

        var city = GameManager.Instance.GetCurrentCityDef();
        int roomCount = city.Rooms;
        _secretBossRoomIndex = _rng.RandiRange(1, roomCount - 2); // hidden in a middle room
        _secretBossFound = GameManager.Instance.SecretBossBeatenInCurrentCity;

        _rooms = GenerateRooms(city, roomCount);
        _currentRoomIndex = 0;
        BuildRoom(_rooms[0]);
        _player.Position = _rooms[0].PlayerStart;
        _camera.GlobalPosition = _player.Position;
    }

    List<RoomData> GenerateRooms(CityDef city, int count)
    {
        var rooms = new List<RoomData>();
        int tier = city.DifficultyTier;

        for (int i = 0; i < count; i++)
        {
            var room = new RoomData();
            int rx = i * (SCREEN_W + 128);
            int groundY = SCREEN_H - 64;
            room.Rect = new Rect2(rx, 0, SCREEN_W, SCREEN_H);
            room.Platforms = new List<Rect2> { new(rx, groundY, SCREEN_W, 64) };

            // Platforms
            for (int p = 0; p < _rng.RandiRange(2, 4); p++)
            {
                int px = rx + (int)_rng.RandiRange(64, SCREEN_W - 128 - 64);
                int py = (int)_rng.RandiRange(SCREEN_H / 3, groundY - 64);
                room.Platforms.Add(new Rect2(px, py, (int)_rng.RandiRange(48, 128), 24));
            }

            room.PlayerStart = new Vector2(rx + 64, groundY - 64);

            // Decide room type
            bool isBoss = i == count - 1; // last room = main boss
            bool isSecretBoss = i == _secretBossRoomIndex && !_secretBossFound;

            room.IsBossRoom = isBoss;
            room.IsSecretBossRoom = isSecretBoss;

            // Enemies for this room
            int enemyCount = isBoss ? 0 : _rng.RandiRange(1, 2 + tier / 2);
            if (isSecretBoss) enemyCount = 0;
            room.Enemies = enemyCount;
            room.EnemySpawns = new List<Vector2>();
            room.EnemyDefs = new List<(string, int, int, int, int, int, int, string, string)>();

            if (isBoss)
            {
                // Main boss
                var b = city.MainBoss;
                Vector2 bossPos = new(rx + SCREEN_W / 2, groundY - 64);
                room.EnemySpawns.Add(bossPos);
                room.EnemyDefs.Add((b.name, (int)(b.hp * 1.0), b.atk, b.def, b.spd, b.xp, b.gold, b.color, b.weakness));
            }
            else if (isSecretBoss)
            {
                // Secret boss (much harder — 2x stats)
                var b = city.SecretBoss;
                Vector2 bossPos = new(rx + SCREEN_W / 2, groundY - 64);
                room.EnemySpawns.Add(bossPos);
                room.EnemyDefs.Add((b.name, (int)(b.hp * 1.0), b.atk, b.def, b.spd, b.xp, b.gold, b.color, b.weakness));
            }
            else
            {
                // Regular enemies from city pool
                for (int e = 0; e < enemyCount; e++)
                {
                    var edef = city.Enemies[_rng.RandiRange(0, city.Enemies.Count - 1)];
                    int ex = rx + (int)_rng.RandiRange(128, SCREEN_W - 128);
                    int ey = groundY - 64;
                    if (e > 0 && room.Platforms.Count > 1)
                    {
                        var plat = room.Platforms[_rng.RandiRange(1, room.Platforms.Count - 1)];
                        ex = (int)_rng.RandiRange((int)plat.Position.X + 16, (int)(plat.Position.X + plat.Size.X - 32));
                        ey = (int)plat.Position.Y - 32;
                    }
                    room.EnemySpawns.Add(new Vector2(ex, ey));
                    room.EnemyDefs.Add(edef);
                }
            }

            // Exit position
            if (isBoss)
                room.ExitPos = new Vector2(rx + SCREEN_W / 2, groundY - 64); // after boss
            else if (isSecretBoss)
                room.ExitPos = new Vector2(rx + SCREEN_W - 48, groundY - 128); // exit at end
            else
                room.ExitPos = new Vector2(rx + SCREEN_W - 48, groundY - 128);

            rooms.Add(room);
        }

        return rooms;
    }

    void BuildRoom(RoomData room)
    {
        foreach (var c in _roomRoot.GetChildren()) c.QueueFree();

        // Platforms
        foreach (var plat in room.Platforms)
        {
            var rect = new ColorRect();
            rect.Position = plat.Position;
            rect.Size = plat.Size;
            var city = GameManager.Instance.GetCurrentCityDef();
            if (room.IsBossRoom) rect.Color = city.ThemeColor * new Color(1.5f, 1.5f, 1.5f);
            else if (room.IsSecretBossRoom) rect.Color = new Color(0.8f, 0.1f, 0.8f);
            else rect.Color = city.ThemeColor * 0.4f + new Color(0.1f, 0.1f, 0.1f);
            _roomRoot.AddChild(rect);

            var staticBody = new StaticBody2D();
            staticBody.Position = plat.Position + plat.Size / 2;
            var colShape = new CollisionShape2D();
            var rectShape = new RectangleShape2D();
            rectShape.Size = plat.Size;
            colShape.Shape = rectShape;
            staticBody.AddChild(colShape);
            _roomRoot.AddChild(staticBody);
        }

        // Enemies
        for (int i = 0; i < room.EnemySpawns.Count; i++)
        {
            var spawn = room.EnemySpawns[i];
            var edef = room.EnemyDefs[i];

            var enemyNode = _enemyOverworldScene.Instantiate<Node2D>();
            enemyNode.Position = spawn;
            enemyNode.SetMeta("enemy_def_name", edef.name);
            enemyNode.SetMeta("enemy_def_hp", edef.hp);
            enemyNode.SetMeta("enemy_def_atk", edef.atk);
            enemyNode.SetMeta("enemy_def_def", edef.def);
            enemyNode.SetMeta("enemy_def_spd", edef.spd);
            enemyNode.SetMeta("enemy_def_xp", edef.xp);
            enemyNode.SetMeta("enemy_def_gold", edef.gold);
            enemyNode.SetMeta("enemy_def_color", edef.color);
            enemyNode.SetMeta("enemy_def_weakness", edef.weakness);
            enemyNode.SetMeta("enemy_is_boss", room.IsBossRoom || room.IsSecretBossRoom);

            // Scale up bosses
            var sprite = enemyNode.GetNode<ColorRect>("Sprite");
            if (room.IsBossRoom || room.IsSecretBossRoom)
            {
                sprite.Size = new Vector2(32, 32);
                sprite.Position = new Vector2(-16, -16);
                var city = GameManager.Instance.GetCurrentCityDef();
                sprite.Color = room.IsSecretBossRoom ? new Color(0.8f, 0.1f, 0.8f) : city.ThemeColor * 1.5f;
            }
            else
            {
                sprite.Color = ColorFromName(edef.color);
            }

            _roomRoot.AddChild(enemyNode);
        }

        // Exit portal (two exits for rooms with secret)
        var cityDef = GameManager.Instance.GetCurrentCityDef();
        Color exitColor = room.IsBossRoom ? new Color(0.8f, 0.6f, 0f) : new Color(0f, 1f, 0.5f);

        // Regular exit for normal/boss rooms
        if (!room.IsSecretBossRoom)
        {
            var exit = MakeExit(room.ExitPos, exitColor, false);
            _roomRoot.AddChild(exit);
        }

        // Secret boss room: show TWO exits — one back (dim) and one forward (purple)
        if (room.IsSecretBossRoom)
        {
            var backExit = MakeExit(new Vector2(room.Rect.Position.X + 48, room.ExitPos.Y), new Color(0.3f, 0.3f, 0.3f), false);
            _roomRoot.AddChild(backExit);

            var bossExit = MakeExit(room.ExitPos, new Color(0.8f, 0.1f, 0.8f), true);
            _roomRoot.AddChild(bossExit);
        }

        // Normal rooms: if this is the secret boss room index, add a hidden exit
        if (!room.IsSecretBossRoom && _currentRoomIndex >= 0 && _currentRoomIndex < _rooms.Count - 1)
        {
            // In the room before the secret boss room, place a hidden exit
            bool isRoomBeforeSecret = _currentRoomIndex == _secretBossRoomIndex - 1 && _secretBossRoomIndex > 0;
            if (isRoomBeforeSecret)
            {
                var hiddenExit = MakeExit(
                    new Vector2(room.Rect.Position.X + SCREEN_W - 200, room.ExitPos.Y + 100),
                    new Color(0.5f, 0.05f, 0.5f, 0.6f), true);
                _roomRoot.AddChild(hiddenExit);
            }
        }

        // Camera bounds
        _camera.SetBounds(room.Rect.Position.X, room.Rect.End.X, room.Rect.Position.Y, room.Rect.End.Y);
    }

    Area2D MakeExit(Vector2 pos, Color color, bool isBossExit)
    {
        var exit = new Area2D();
        exit.Name = isBossExit ? "BossExitPortal" : "ExitPortal";
        exit.Position = pos;
        exit.CollisionMask = 1;
        var exitShape = new CollisionShape2D();
        var exitRect = new RectangleShape2D();
        exitRect.Size = new Vector2(32, 48);
        exitShape.Shape = exitRect;
        exit.AddChild(exitShape);
        var exitSprite = new ColorRect();
        exitSprite.Size = new Vector2(32, 48);
        exitSprite.Color = color;
        exitSprite.Position = new Vector2(-16, -24);
        exit.AddChild(exitSprite);
        exit.BodyEntered += (body) => OnExitBodyEntered(body, isBossExit);
        return exit;
    }

    void OnExitBodyEntered(Node body, bool isSecretExit = false)
    {
        if (body is Player)
        {
            var gm = GameManager.Instance;
            var city = gm.GetCurrentCityDef();

            if (isSecretExit)
            {
                // Secret boss room found — teleport there
                int secretIdx = _secretBossRoomIndex;
                if (secretIdx >= 0 && secretIdx < _rooms.Count)
                {
                    _currentRoomIndex = secretIdx;
                    BuildRoom(_rooms[secretIdx]);
                    _player.Position = _rooms[secretIdx].PlayerStart;
                    return;
                }
            }

            if (_rooms[_currentRoomIndex].IsBossRoom)
            {
                // Main boss defeated — unlock next cities
                gm.OnMainBossDefeated();
                _player.Disable();
                // Transition back to world map
                var timer = GetTree().CreateTimer(1.0f);
                timer.Timeout += () => GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
                return;
            }

            if (_rooms[_currentRoomIndex].IsSecretBossRoom)
            {
                // Secret boss defeated
                gm.OnSecretBossDefeated();
                // Return to world map
                _player.Disable();
                var timer = GetTree().CreateTimer(1.0f);
                timer.Timeout += () => GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
                return;
            }

            // Normal room — advance
            _currentRoomIndex++;
            if (_currentRoomIndex >= _rooms.Count)
            {
                // Shouldn't happen (boss is last), but safety
                _player.Disable();
                GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
                return;
            }

            BuildRoom(_rooms[_currentRoomIndex]);
            _player.Position = _rooms[_currentRoomIndex].PlayerStart;
        }
    }

    Color ColorFromName(string name)
    {
        return name switch
        {
            "red" => new Color(1, 0.2f, 0.2f), "lime" => new Color(0.2f, 1, 0.2f),
            "aqua" => new Color(0.2f, 0.8f, 1), "purple" => new Color(0.6f, 0.2f, 1),
            "white" => new Color(0.9f, 0.9f, 0.9f), "gold" => new Color(1, 0.8f, 0),
            "gray" => new Color(0.5f, 0.5f, 0.5f), "brown" => new Color(0.6f, 0.4f, 0.2f),
            "orange" => new Color(1, 0.6f, 0), "blue" => new Color(0.2f, 0.4f, 1),
            "green" => new Color(0.2f, 0.6f, 0.2f), "black" => new Color(0.1f, 0.1f, 0.1f),
            _ => new Color(1, 0, 0)
        };
    }
}
