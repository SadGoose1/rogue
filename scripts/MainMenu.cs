using Godot;

namespace Rouge;

public partial class MainMenu : Control
{
    private VBoxContainer _menuButtons;
    private Label _titleLabel;
    private Label _subtitleLabel;

    // Bloodborne loading screen particles (falling ash/embers)
    private float _particleTime;
    private const int PARTICLE_COUNT = 20;
    private Vector2[] _particlePos;
    private Vector2[] _particleVel;
    private Color[] _particleColors;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("TitleLabel");
        _subtitleLabel = GetNode<Label>("SubtitleLabel");
        _menuButtons = GetNode<VBoxContainer>("MenuButtons");

        // Style title with Bloodborne palette
        _titleLabel.Text = "ROUGE";
        _subtitleLabel.Text = "Fear the Old Blood";

        // Initialize falling ash particles
        var rng = new RandomNumberGenerator();
        _particlePos = new Vector2[PARTICLE_COUNT];
        _particleVel = new Vector2[PARTICLE_COUNT];
        _particleColors = new Color[PARTICLE_COUNT];

        for (int i = 0; i < PARTICLE_COUNT; i++)
        {
            _particlePos[i] = new Vector2(
                rng.RandfRange(0, GetViewportRect().Size.X),
                rng.RandfRange(0, GetViewportRect().Size.Y)
            );
            _particleVel[i] = new Vector2(
                rng.RandfRange(-6f, -1f),
                rng.RandfRange(-15f, -3f)
            );
            _particleColors[i] = new Color(
                0.5f + rng.Randf() * 0.3f,
                0.03f + rng.Randf() * 0.05f,
                0.03f + rng.Randf() * 0.05f,
                0.08f + rng.Randf() * 0.2f
            );
        }

        // Clear old buttons and rebuild
        foreach (var c in _menuButtons.GetChildren())
            c.QueueFree();

        string[] items = { "Begin Descent", "Exit" };
        string[] actions = { "start", "exit" };

        for (int i = 0; i < items.Length; i++)
        {
            int idx = i;
            var btn = new Button();
            btn.Text = items[i];
            btn.CustomMinimumSize = new Vector2(300, 50);
            StyleMainButton(btn);
            btn.Pressed += () =>
            {
                if (actions[idx] == "start")
                    StartGame();
                else
                    GetTree().Quit();
            };
            _menuButtons.AddChild(btn);
        }

        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        _particleTime += (float)delta;
        var viewport = GetViewportRect();
        float w = viewport.Size.X;
        float h = viewport.Size.Y;

        for (int i = 0; i < PARTICLE_COUNT; i++)
        {
            _particlePos[i] += _particleVel[i] * (float)delta;
            _particlePos[i].X += Mathf.Sin(_particleTime * 0.5f + i) * 3f * (float)delta;
            // Reset when off screen
            if (_particlePos[i].Y < -20)
            {
                _particlePos[i] = new Vector2(
                    (float)GD.RandRange(0, w),
                    h + 20
                );
            }
            if (_particlePos[i].X < -20)
            {
                _particlePos[i] = new Vector2(w + 20, (float)GD.RandRange(0, h));
            }
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int i = 0; i < PARTICLE_COUNT; i++)
        {
            DrawCircle(_particlePos[i], 1.5f + (i % 3), _particleColors[i]);
        }
    }

    void StyleMainButton(Button btn)
    {
        btn.AddThemeColorOverride("font_color", new Color(0.82f, 0.78f, 0.74f));
        btn.AddThemeColorOverride("font_hover_color", new Color(0.95f, 0.9f, 0.85f));
        btn.AddThemeColorOverride("font_pressed_color", new Color(0.9f, 0.1f, 0.1f));
        btn.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new Color(0.1f, 0.04f, 0.05f), BorderColor = new Color(0.35f, 0.08f, 0.08f), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        btn.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = new Color(0.2f, 0.06f, 0.08f), BorderColor = new Color(0.55f, 0.1f, 0.1f), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        btn.AddThemeStyleboxOverride("pressed", new StyleBoxFlat { BgColor = new Color(0.3f, 0.04f, 0.04f), BorderColor = new Color(0.7f, 0.05f, 0.05f), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        btn.AddThemeFontSizeOverride("font_size", 20);
    }

    void StartGame()
    {
        GetTree().ChangeSceneToFile("res://scenes/CharacterSelect.tscn");
    }
}
