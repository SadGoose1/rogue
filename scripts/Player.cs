using Godot;

namespace Rouge;

public partial class Player : CharacterBody2D
{
    private float _gravity;
    private Node2D _spriteRoot;
    private Area2D _hitbox;
    private CharacterClass _class;

    // Bloodborne-style character pieces — populated per class
    private ColorRect _helmet, _head, _body, _torso, _cloak, _legs, _bootsL, _bootsR;
    private ColorRect _weapon, _weapon2, _shield, _accessory;
    private Node2D _weaponRoot;

    public override void _Ready()
    {
        _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        _spriteRoot = GetNode<Node2D>("SpriteRoot");
        _hitbox = GetNode<Area2D>("Hitbox");

        if (GameManager.Instance != null)
        {
            _class = GameManager.Instance.SelectedClass;
            BuildSprite();
        }
    }

    void BuildSprite()
    {
        // Remove old children
        foreach (var c in _spriteRoot.GetChildren())
            c.QueueFree();

        Color c1 = ClassColor();
        Color dark = c1 * 0.4f;
        Color mid = c1 * 0.65f;
        Color metal = new Color(0.55f, 0.55f, 0.65f);
        Color darkMetal = new Color(0.25f, 0.25f, 0.35f);
        Color leather = new Color(0.3f, 0.18f, 0.1f);
        Color darkLeather = new Color(0.15f, 0.08f, 0.05f);
        Color blood = new Color(0.55f, 0.05f, 0.05f);

        switch (_class)
        {
            case CharacterClass.Knight:
                // Vileblood Knight — Gothic full plate + Ludwig's greatsword
                // Cloak (behind)
                AddPiece(out _cloak, new Vector2(20, 28), new Vector2(-10, 2), darkMetal * 0.6f);
                // Helmet — plumed gothic helm
                AddPiece(out _helmet, new Vector2(16, 18), new Vector2(-8, -12), darkMetal);
                AddPiece(out _, new Vector2(14, 6), new Vector2(-7, -14), darkMetal); // visor
                AddPiece(out _, new Vector2(4, 2), new Vector2(-2, -13), new Color(0.15f, 0.05f, 0.05f)); // visor slit
                // Plume
                AddPiece(out _, new Vector2(4, 10), new Vector2(-2, -24), blood);
                AddPiece(out _, new Vector2(2, 8), new Vector2(-1, -22), new Color(0.7f, 0.3f, 0.3f));
                // Body — gothic plate
                AddPiece(out _body, new Vector2(20, 12), new Vector2(-10, 4), darkMetal);
                // Chestplate — cross emblem
                AddPiece(out _torso, new Vector2(14, 10), new Vector2(-7, 5), mid);
                AddPiece(out _, new Vector2(6, 6), new Vector2(-3, 7), new Color(0.6f, 0.5f, 0.1f)); // emblem
                // Legs
                AddPiece(out _legs, new Vector2(8, 8), new Vector2(-8, 16), darkMetal);
                AddPiece(out _, new Vector2(8, 8), new Vector2(0, 16), darkMetal);
                // Boots — sabatons
                AddPiece(out _bootsL, new Vector2(6, 4), new Vector2(-7, 24), metal);
                AddPiece(out _bootsR, new Vector2(6, 4), new Vector2(1, 24), metal);
                // Greatsword (Ludwig's Holy Blade style)
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(6, 30), new Vector2(-3, -4), new Color(0.65f, 0.65f, 0.75f)); // blade
                AddWeaponPiece(new Vector2(4, 8), new Vector2(-2, 24), metal); // guard
                AddWeaponPiece(new Vector2(8, 4), new Vector2(-4, 28), darkLeather); // grip
                AddWeaponPiece(new Vector2(6, 4), new Vector2(-3, 34), new Color(0.5f, 0.5f, 0.1f)); // pommel
                break;

            case CharacterClass.Mage:
                // Byrgenwerth Scholar — dark robes + arcane staff
                // Cloak
                AddPiece(out _cloak, new Vector2(22, 34), new Vector2(-11, 0), new Color(0.08f, 0.08f, 0.2f));
                // Hat — wide brim pointed
                AddPiece(out _helmet, new Vector2(18, 18), new Vector2(-9, -18), new Color(0.08f, 0.06f, 0.12f));
                AddPiece(out _, new Vector2(14, 4), new Vector2(-7, -22), new Color(0.08f, 0.06f, 0.12f));
                AddPiece(out _, new Vector2(6, 10), new Vector2(-3, -28), new Color(0.08f, 0.06f, 0.12f)); // spire
                AddPiece(out _, new Vector2(8, 2), new Vector2(-4, -30), new Color(0.3f, 0.5f, 1.0f, 0.6f)); // arcane glow
                // Head — pale face
                AddPiece(out _head, new Vector2(8, 8), new Vector2(-4, -8), new Color(0.65f, 0.55f, 0.5f));
                // Body — robes
                AddPiece(out _body, new Vector2(18, 20), new Vector2(-9, 4), new Color(0.1f, 0.12f, 0.3f));
                // Robe trim — arcane runes
                AddPiece(out _torso, new Vector2(14, 6), new Vector2(-7, 16), new Color(0.2f, 0.3f, 0.6f));
                AddPiece(out _, new Vector2(12, 2), new Vector2(-6, 22), new Color(0.7f, 0.6f, 0.3f));
                // Legs — lower robe
                AddPiece(out _legs, new Vector2(14, 8), new Vector2(-7, 22), new Color(0.08f, 0.1f, 0.25f));
                // Staff with arcane orb
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(4, 32), new Vector2(-2, 0), new Color(0.35f, 0.2f, 0.08f)); // staff shaft
                AddWeaponPiece(new Vector2(10, 10), new Vector2(-5, -10), new Color(0.15f, 0.2f, 0.5f, 0.5f)); // glow aura
                AddWeaponPiece(new Vector2(6, 6), new Vector2(-3, -14), new Color(0.4f, 0.6f, 1.0f, 0.8f)); // orb core
                break;

            case CharacterClass.Rogue:
                // Cainhurst Assassin — top hat + rapier + opera cape
                // Cape (behind)
                AddPiece(out _cloak, new Vector2(20, 30), new Vector2(-10, -2), new Color(0.12f, 0.02f, 0.04f));
                // Top hat
                AddPiece(out _helmet, new Vector2(16, 4), new Vector2(-8, -18), new Color(0.06f, 0.04f, 0.04f));
                AddPiece(out _, new Vector2(12, 12), new Vector2(-6, -24), new Color(0.06f, 0.04f, 0.04f));
                AddPiece(out _, new Vector2(6, 2), new Vector2(-3, -28), new Color(0.06f, 0.04f, 0.04f)); // crown
                // Head — masked face
                AddPiece(out _head, new Vector2(10, 10), new Vector2(-5, -10), new Color(0.55f, 0.45f, 0.4f));
                AddPiece(out _, new Vector2(8, 4), new Vector2(-4, -12), new Color(0.1f, 0.1f, 0.12f)); // half-mask
                AddPiece(out _, new Vector2(2, 2), new Vector2(-1, -13), new Color(0.8f, 0.15f, 0.15f)); // eye glow
                // Body — elegant doublet
                AddPiece(out _body, new Vector2(18, 16), new Vector2(-9, 2), new Color(0.15f, 0.05f, 0.06f));
                AddPiece(out _torso, new Vector2(12, 10), new Vector2(-6, 4), new Color(0.25f, 0.08f, 0.1f)); // vest
                AddPiece(out _, new Vector2(4, 2), new Vector2(-2, 3), new Color(0.7f, 0.7f, 0.75f)); // cravat
                // Legs
                AddPiece(out _legs, new Vector2(8, 8), new Vector2(-8, 18), new Color(0.08f, 0.04f, 0.04f));
                AddPiece(out _, new Vector2(8, 8), new Vector2(0, 18), new Color(0.08f, 0.04f, 0.04f));
                // Boots
                AddPiece(out _bootsL, new Vector2(6, 4), new Vector2(-7, 26), darkLeather);
                AddPiece(out _bootsR, new Vector2(6, 4), new Vector2(1, 26), darkLeather);
                // Rapier
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(3, 20), new Vector2(-1, 8), new Color(0.65f, 0.65f, 0.8f)); // blade
                AddWeaponPiece(new Vector2(4, 4), new Vector2(-2, 26), new Color(0.6f, 0.55f, 0.2f)); // guard
                AddWeaponPiece(new Vector2(3, 4), new Vector2(-1, 30), darkLeather); // grip
                // Dagger (off-hand, on the left side of the sprite)
                var daggerNode = new Node2D();
                daggerNode.Position = new Vector2(-10, 4);
                _spriteRoot.AddChild(daggerNode);
                var daggerPiece = new ColorRect();
                daggerPiece.Size = new Vector2(2, 10);
                daggerPiece.Position = new Vector2(0, 0);
                daggerPiece.Color = new Color(0.6f, 0.6f, 0.7f);
                daggerNode.AddChild(daggerPiece);
                break;

            case CharacterClass.Paladin:
                // Church Executioner — golden-white holy knight
                // White cloak
                AddPiece(out _cloak, new Vector2(22, 30), new Vector2(-11, 0), new Color(0.35f, 0.35f, 0.45f));
                // Helmet — gothic mitre-style helm
                AddPiece(out _helmet, new Vector2(14, 16), new Vector2(-7, -12), new Color(0.55f, 0.55f, 0.65f));
                AddPiece(out _, new Vector2(10, 6), new Vector2(-5, -16), new Color(0.55f, 0.55f, 0.65f));
                AddPiece(out _, new Vector2(6, 4), new Vector2(-3, -22), new Color(0.7f, 0.6f, 0.15f)); // golden tip
                AddPiece(out _, new Vector2(2, 2), new Vector2(-1, -14), new Color(0.9f, 0.8f, 0.3f)); // eye slot glow
                // Body — white robes + gold chestplate
                AddPiece(out _body, new Vector2(20, 14), new Vector2(-10, 4), new Color(0.4f, 0.4f, 0.55f));
                AddPiece(out _torso, new Vector2(14, 10), new Vector2(-7, 5), new Color(0.6f, 0.55f, 0.15f)); // gold plate
                AddPiece(out _, new Vector2(6, 6), new Vector2(-3, 7), new Color(0.75f, 0.7f, 0.25f)); // crest
                // Legs
                AddPiece(out _legs, new Vector2(8, 8), new Vector2(-8, 18), new Color(0.35f, 0.35f, 0.45f));
                AddPiece(out _, new Vector2(8, 8), new Vector2(0, 18), new Color(0.35f, 0.35f, 0.45f));
                // Boots
                AddPiece(out _bootsL, new Vector2(6, 4), new Vector2(-7, 26), new Color(0.55f, 0.55f, 0.65f));
                AddPiece(out _bootsR, new Vector2(6, 4), new Vector2(1, 26), new Color(0.55f, 0.55f, 0.65f));
                // Holy mace
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(4, 18), new Vector2(-2, 10), new Color(0.4f, 0.25f, 0.1f));
                AddWeaponPiece(new Vector2(10, 8), new Vector2(-5, -2), new Color(0.7f, 0.6f, 0.15f)); // mace head
                AddWeaponPiece(new Vector2(6, 4), new Vector2(-3, -6), new Color(0.85f, 0.8f, 0.4f)); // glow
                break;

            case CharacterClass.Ranger:
                // Powder Keg Hunter — wide hat + bandolier + crossbow
                // Cloak
                AddPiece(out _cloak, new Vector2(22, 30), new Vector2(-11, 0), new Color(0.08f, 0.2f, 0.08f));
                // Wide hat
                AddPiece(out _helmet, new Vector2(22, 4), new Vector2(-11, -20), new Color(0.12f, 0.08f, 0.04f));
                AddPiece(out _, new Vector2(14, 10), new Vector2(-7, -24), new Color(0.12f, 0.08f, 0.04f));
                AddPiece(out _, new Vector2(4, 4), new Vector2(-2, -16), new Color(0.12f, 0.08f, 0.04f));
                // Head
                AddPiece(out _head, new Vector2(10, 10), new Vector2(-5, -10), new Color(0.6f, 0.5f, 0.4f));
                AddPiece(out _, new Vector2(8, 4), new Vector2(-4, -12), new Color(0.08f, 0.2f, 0.08f)); // bandana
                // Body — hunter's coat
                AddPiece(out _body, new Vector2(20, 18), new Vector2(-10, 2), new Color(0.2f, 0.35f, 0.15f));
                AddPiece(out _torso, new Vector2(14, 10), new Vector2(-7, 4), new Color(0.25f, 0.15f, 0.08f)); // leather vest
                // Bandolier
                AddPiece(out _, new Vector2(12, 2), new Vector2(-6, 10), new Color(0.35f, 0.25f, 0.12f));
                AddPiece(out _, new Vector2(2, 2), new Vector2(2, 8), new Color(0.6f, 0.55f, 0.5f)); // buckle
                // Legs
                AddPiece(out _legs, new Vector2(8, 8), new Vector2(-8, 20), new Color(0.15f, 0.25f, 0.1f));
                AddPiece(out _, new Vector2(8, 8), new Vector2(0, 20), new Color(0.15f, 0.25f, 0.1f));
                // Boots
                AddPiece(out _bootsL, new Vector2(6, 4), new Vector2(-7, 28), darkLeather);
                AddPiece(out _bootsR, new Vector2(6, 4), new Vector2(1, 28), darkLeather);
                // Crossbow
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(12, 4), new Vector2(-6, 6), new Color(0.3f, 0.18f, 0.08f)); // stock
                AddWeaponPiece(new Vector2(10, 2), new Vector2(-5, 2), new Color(0.4f, 0.4f, 0.45f)); // bowstring
                AddWeaponPiece(new Vector2(4, 4), new Vector2(-2, 4), darkMetal); // mechanism
                break;

            case CharacterClass.Berserker:
                // Beastly Hunter — tattered, feral, massive serrated cleaver
                // Wild mane/hair
                for (int i = -3; i <= 3; i++)
                    AddPiece(out _, new Vector2(6, 16), new Vector2(-3 + i * 5, -26), blood * 0.8f);
                for (int i = -2; i <= 2; i++)
                    AddPiece(out _, new Vector2(4, 10), new Vector2(-2 + i * 5, -22), new Color(0.7f, 0.2f, 0.2f));
                // Head — savage/angry face
                AddPiece(out _head, new Vector2(12, 12), new Vector2(-6, -14), new Color(0.6f, 0.3f, 0.2f));
                // War paint
                AddPiece(out _, new Vector2(2, 6), new Vector2(-1, -12), blood * 0.8f);
                AddPiece(out _, new Vector2(6, 2), new Vector2(-3, -16), blood * 0.8f);
                // Eyes — feral glow
                AddPiece(out _, new Vector2(2, 2), new Vector2(-4, -16), new Color(0.9f, 0.9f, 0.15f));
                AddPiece(out _, new Vector2(2, 2), new Vector2(2, -16), new Color(0.9f, 0.9f, 0.15f));
                // Body — bare chest
                AddPiece(out _body, new Vector2(22, 16), new Vector2(-11, 0), new Color(0.55f, 0.25f, 0.18f));
                // Fur pelt on shoulders
                AddPiece(out _torso, new Vector2(24, 8), new Vector2(-12, -4), new Color(0.25f, 0.12f, 0.06f));
                AddPiece(out _, new Vector2(4, 4), new Vector2(-2, 6), blood); // fresh wound
                // Fur loincloth
                AddPiece(out _legs, new Vector2(16, 8), new Vector2(-8, 16), new Color(0.25f, 0.12f, 0.06f));
                // Legs (bare)
                AddPiece(out _, new Vector2(6, 8), new Vector2(-7, 22), new Color(0.5f, 0.22f, 0.15f));
                AddPiece(out _, new Vector2(6, 8), new Vector2(1, 22), new Color(0.5f, 0.22f, 0.15f));
                // Feet (bare/wrapped)
                AddPiece(out _bootsL, new Vector2(5, 3), new Vector2(-7, 30), new Color(0.3f, 0.15f, 0.1f));
                AddPiece(out _bootsR, new Vector2(5, 3), new Vector2(1, 30), new Color(0.3f, 0.15f, 0.1f));
                // Massive serrated cleaver (Saw Cleaver style)
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(8, 30), new Vector2(-4, -4), new Color(0.45f, 0.5f, 0.55f)); // blade
                // Serrated teeth
                for (int i = 0; i < 5; i++)
                    AddWeaponPiece(new Vector2(3, 3), new Vector2(4 + i * 2, -2 + i * 5), metal);
                // Handle
                AddWeaponPiece(new Vector2(4, 8), new Vector2(-2, 24), new Color(0.25f, 0.12f, 0.06f));
                // Blood spatter
                AddWeaponPiece(new Vector2(4, 2), new Vector2(0, 2), blood);
                AddWeaponPiece(new Vector2(3, 2), new Vector2(2, 10), blood);
                break;

            case CharacterClass.Gunslinger:
                // Yharnam Hunter — the iconic Bloodborne look with tricorn + long coat + Evelyn
                // Long coat (behind)
                AddPiece(out _cloak, new Vector2(24, 34), new Vector2(-12, -2), new Color(0.08f, 0.05f, 0.06f));
                // Tricorn hat
                AddPiece(out _helmet, new Vector2(20, 4), new Vector2(-10, -22), new Color(0.06f, 0.04f, 0.05f));
                AddPiece(out _, new Vector2(14, 8), new Vector2(-7, -26), new Color(0.06f, 0.04f, 0.05f));
                AddPiece(out _, new Vector2(12, 4), new Vector2(-6, -28), new Color(0.06f, 0.04f, 0.05f));
                AddPiece(out _, new Vector2(6, 2), new Vector2(-3, -30), new Color(0.2f, 0.15f, 0.1f)); // rim lift
                // Head
                AddPiece(out _head, new Vector2(10, 10), new Vector2(-5, -12), new Color(0.6f, 0.45f, 0.35f));
                AddPiece(out _, new Vector2(8, 3), new Vector2(-4, -14), new Color(0.15f, 0.08f, 0.05f)); // bandana
                AddPiece(out _, new Vector2(2, 2), new Vector2(-1, -15), new Color(0.7f, 0.7f, 0.75f)); // eye glint
                // Scarf
                AddPiece(out _, new Vector2(6, 3), new Vector2(-3, -6), new Color(0.6f, 0.08f, 0.08f)); // red scarf
                // Body — long coat front
                AddPiece(out _body, new Vector2(20, 18), new Vector2(-10, 2), new Color(0.12f, 0.07f, 0.08f));
                AddPiece(out _torso, new Vector2(12, 10), new Vector2(-6, 4), new Color(0.06f, 0.04f, 0.05f)); // vest
                AddPiece(out _, new Vector2(8, 2), new Vector2(-4, 6), new Color(0.6f, 0.55f, 0.5f)); // shirt front
                // Bandolier
                AddPiece(out _, new Vector2(10, 2), new Vector2(-5, 12), new Color(0.3f, 0.18f, 0.08f));
                AddPiece(out _, new Vector2(2, 2), new Vector2(3, 10), metal); // buckle
                // Coat tails
                AddPiece(out _, new Vector2(18, 8), new Vector2(-9, 20), new Color(0.12f, 0.07f, 0.08f));
                // Legs
                AddPiece(out _legs, new Vector2(8, 8), new Vector2(-8, 26), new Color(0.08f, 0.05f, 0.06f));
                AddPiece(out _, new Vector2(8, 8), new Vector2(0, 26), new Color(0.08f, 0.05f, 0.06f));
                // Boots — hunter boots
                AddPiece(out _bootsL, new Vector2(6, 5), new Vector2(-7, 32), new Color(0.2f, 0.12f, 0.08f));
                AddPiece(out _bootsR, new Vector2(6, 5), new Vector2(1, 32), new Color(0.2f, 0.12f, 0.08f));
                // Evelyn pistol
                AddWeaponRoot();
                AddWeaponPiece(new Vector2(6, 6), new Vector2(-3, 2), new Color(0.4f, 0.4f, 0.5f)); // barrel
                AddWeaponPiece(new Vector2(4, 4), new Vector2(-2, 8), new Color(0.35f, 0.25f, 0.12f)); // handle
                AddWeaponPiece(new Vector2(6, 2), new Vector2(-3, 6), new Color(0.5f, 0.5f, 0.1f)); // brass
                AddWeaponPiece(new Vector2(2, 2), new Vector2(-1, 0), new Color(1, 0.8f, 0.2f, 0.4f)); // muzzle flash
                break;
        }
    }

    void AddPiece(out ColorRect rect, Vector2 size, Vector2 pos, Color color)
    {
        rect = new ColorRect();
        rect.Size = size;
        rect.Position = pos;
        rect.Color = color;
        _spriteRoot.AddChild(rect);
    }

    void AddWeaponRoot()
    {
        _weaponRoot = new Node2D();
        _weaponRoot.Position = new Vector2(12, -4);
        _spriteRoot.AddChild(_weaponRoot);
    }

    void AddWeaponPiece(Vector2 size, Vector2 pos, Color color)
    {
        var piece = new ColorRect();
        piece.Size = size;
        piece.Position = pos;
        piece.Color = color;
        _weaponRoot.AddChild(piece);
    }

    Color ClassColor()
    {
        return _class switch
        {
            CharacterClass.Knight => new Color(0.5f, 0.5f, 0.8f),
            CharacterClass.Mage => new Color(0.3f, 0.5f, 1.0f),
            CharacterClass.Rogue => new Color(0.4f, 0.8f, 0.4f),
            CharacterClass.Paladin => new Color(0.9f, 0.8f, 0.3f),
            CharacterClass.Ranger => new Color(0.2f, 0.7f, 0.3f),
            CharacterClass.Berserker => new Color(0.9f, 0.2f, 0.2f),
            CharacterClass.Gunslinger => new Color(0.8f, 0.6f, 0.2f),
            _ => new Color(0.75f, 0.65f, 0.85f),
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        float d = (float)delta;
        Vector2 vel = Velocity;

        if (!IsOnFloor())
            vel.Y += _gravity * d;

        float input = Input.GetAxis("move_left", "move_right");
        float speed = 200f;
        if (GameManager.Instance != null)
            speed = 200f + GameManager.Instance.GetTotalSPD() * 4f;

        if (input != 0)
        {
            vel.X = Mathf.MoveToward(vel.X, input * speed, 1500f * d);
            _spriteRoot.Scale = new Vector2(Mathf.Abs(_spriteRoot.Scale.X) * Mathf.Sign(input), _spriteRoot.Scale.Y);
            if (input < 0)
                _spriteRoot.Scale = new Vector2(-Mathf.Abs(_spriteRoot.Scale.X), _spriteRoot.Scale.Y);
            else
                _spriteRoot.Scale = new Vector2(Mathf.Abs(_spriteRoot.Scale.X), _spriteRoot.Scale.Y);
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, 1000f * d);
        }

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            vel.Y = -420f;

        if (Input.IsActionJustReleased("jump") && vel.Y < 0)
            vel.Y *= 0.5f;

        Velocity = vel;
        MoveAndSlide();
        UpdateVisual();
    }

    void UpdateVisual()
    {
        // Subtle movement-based tinting for the body pieces
        Color c = ClassColor();
        if (!IsOnFloor())
        {
            if (_body != null) _body.Modulate = new Color(0.6f, 0.6f, 0.6f);
        }
        else if (Mathf.Abs(Velocity.X) > 10)
        {
            if (_body != null) _body.Modulate = new Color(1.1f, 1.1f, 1.1f);
        }
        else
        {
            if (_body != null) _body.Modulate = Colors.White;
        }
    }

    public void Disable()
    {
        SetPhysicsProcess(false);
        foreach (var c in _spriteRoot.GetChildren())
        {
            if (c is ColorRect cr)
                cr.Color = new Color(0.15f, 0.05f, 0.05f) * 0.5f;
        }
    }

    public void Enable()
    {
        SetPhysicsProcess(true);
        BuildSprite();
    }
}
