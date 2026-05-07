using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Rouge;

public partial class WorldMap : Control
{
    private GridContainer _cityGrid;
    private Label _infoLabel;
    private Label _playerSummaryLabel;
    private Button _travelButton;
    private Button _backButton;

    private string _selectedCity = "";

    public override void _Ready()
    {
        _cityGrid = GetNode<GridContainer>("CityGrid");
        _infoLabel = GetNode<Label>("InfoPanel/InfoLabel");
        _playerSummaryLabel = GetNode<Label>("InfoPanel/PlayerSummary");
        _travelButton = GetNode<Button>("InfoPanel/TravelButton");
        _backButton = GetNode<Button>("InfoPanel/BackButton");

        BuildCityCards();

        // Default select first unlocked
        var unlocked = GameManager.Instance.UnlockedCities;
        if (unlocked.Count > 0)
        {
            string first = unlocked.First();
            foreach (var k in GameManager.AllCities.Keys)
            {
                if (unlocked.Contains(k)) { first = k; break; }
            }
            _selectedCity = first;
        }
        UpdateInfo();

        _travelButton.Pressed += () =>
        {
            if (string.IsNullOrEmpty(_selectedCity) || !GameManager.Instance.UnlockedCities.Contains(_selectedCity)) return;
            GameManager.Instance.EnterCity(_selectedCity);
            GetTree().ChangeSceneToFile("res://scenes/LevelScene.tscn");
        };

        _backButton.Pressed += () =>
        {
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        };

        StyleBtn(_travelButton);
        StyleBtn(_backButton);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
    }

    void BuildCityCards()
    {
        foreach (var c in _cityGrid.GetChildren().OfType<Node>()) c.QueueFree();

        var gm = GameManager.Instance;
        int cols = 4;
        int idx = 0;

        foreach (var (name, city) in GameManager.AllCities)
        {
            bool unlocked = gm.UnlockedCities.Contains(name);
            bool mainDone = gm.IsBossDefeated(name, true);
            bool secretDone = gm.IsBossDefeated(name, false);

            var card = new MarginContainer();
            card.CustomMinimumSize = new Vector2(180, 200);
            card.AddThemeConstantOverride("margin_left", 3);
            card.AddThemeConstantOverride("margin_right", 3);
            card.AddThemeConstantOverride("margin_top", 3);
            card.AddThemeConstantOverride("margin_bottom", 3);

            int i = idx;
            string cityName = name;

            // Border
            var border = new ColorRect();
            border.Size = new Vector2(180, 200);
            border.Color = unlocked ? (mainDone ? new Color(0.2f, 0.5f, 0.2f) : city.ThemeColor * 0.6f) : new Color(0.1f, 0.05f, 0.05f);
            border.MouseFilter = Control.MouseFilterEnum.Ignore;

            var bg = new ColorRect();
            bg.Size = new Vector2(168, 188);
            bg.Position = new Vector2(6, 6);
            bg.Color = unlocked ? new Color(0.08f, 0.04f, 0.05f) : new Color(0.04f, 0.02f, 0.03f);
            bg.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Click
            var clickArea = new ColorRect();
            clickArea.Size = new Vector2(180, 200);
            clickArea.Color = Colors.Transparent;
            clickArea.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
            clickArea.GuiInput += (ev) =>
            {
                if (ev is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left && unlocked)
                {
                    _selectedCity = cityName;
                    UpdateInfo();
                    AcceptEvent();
                }
            };

            // Name
            var nameLabel = new Label();
            nameLabel.Position = new Vector2(8, 8);
            nameLabel.Text = unlocked ? cityName : "???";
            nameLabel.AddThemeColorOverride("font_color", unlocked ? city.ThemeColor : new Color(0.3f, 0.3f, 0.3f));
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Status
            var statusLabel = new Label();
            statusLabel.Position = new Vector2(8, 32);
            if (!unlocked) statusLabel.Text = "[LOCKED]";
            else
            {
                string s = $"[Tier {city.DifficultyTier}]";
                if (mainDone) s += "\n[Main boss slain]";
                if (secretDone) s += "\n[Secret slain]";
                statusLabel.Text = s;
            }
            statusLabel.AddThemeColorOverride("font_color", unlocked ? new Color(0.6f, 0.55f, 0.5f) : new Color(0.2f, 0.2f, 0.2f));
            statusLabel.AddThemeFontSizeOverride("font_size", 10);
            statusLabel.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Color swatch
            if (unlocked)
            {
                var swatch = new ColorRect();
                swatch.Position = new Vector2(20, 75);
                swatch.Size = new Vector2(130, 90);
                swatch.Color = city.ThemeColor * new Color(1, 1, 1, 0.15f);
                swatch.MouseFilter = Control.MouseFilterEnum.Ignore;
                card.AddChild(swatch);
            }

            card.AddChild(border);
            card.AddChild(bg);
            card.AddChild(clickArea);
            card.AddChild(nameLabel);
            card.AddChild(statusLabel);
            _cityGrid.AddChild(card);

            // Click field for border highlight
            border.SetMeta("city_index", idx);

            idx++;
        }
    }

    void UpdateInfo()
    {
        if (string.IsNullOrEmpty(_selectedCity) || !GameManager.AllCities.ContainsKey(_selectedCity))
        {
            _infoLabel.Text = "Select a city";
            _travelButton.Disabled = true;
            return;
        }

        var city = GameManager.AllCities[_selectedCity];
        bool unlocked = GameManager.Instance.UnlockedCities.Contains(_selectedCity);
        bool mainDone = GameManager.Instance.IsBossDefeated(_selectedCity, true);
        bool secretDone = GameManager.Instance.IsBossDefeated(_selectedCity, false);

        _infoLabel.Text = $"[color={city.ThemeColor.ToHtml()}]{city.Name}[/color]\n" +
            $"{city.Description}\n" +
            $"Tier: {city.DifficultyTier}  |  Rooms: {city.Rooms}\n" +
            $"Main Boss: [color=#ffaa00]{city.MainBoss.name}[/color] (HP:{city.MainBoss.hp})\n" +
            $"Secret Boss: [color=#cc44ff]{city.SecretBoss.name}[/color] (HP:{city.SecretBoss.hp})\n" +
            $"Main Boss: {(mainDone ? "[color=#44ff44]SLAIN[/color]" : "[color=#ff4444]ALIVE[/color]")}  " +
            $"Secret: {(secretDone ? "[color=#44ff44]SLAIN[/color]" : "[color=#ff4444]UNKNOWN[/color]")}";

        _travelButton.Disabled = !unlocked;
        _travelButton.Text = unlocked ? $"Travel to {city.Name}" : "LOCKED";

        var p = GameManager.Instance.PlayerStats;
        var gm = GameManager.Instance;
        _playerSummaryLabel.Text = $"{p.Name}  Lv.{p.Level}\n" +
            $"HP: {p.CurrentHP}/{gm.GetTotalMaxHP()}  MP: {p.CurrentMP}/{gm.GetTotalMaxMP()}\n" +
            $"ATK: {gm.GetTotalATK()}  DEF: {gm.GetTotalDEF()}  SPD: {gm.GetTotalSPD()}\n" +
            $"Gold: {p.Gold}  Cities Cleared: {gm.BossesDefeated.Count(kv => kv.Key.EndsWith("_main") && kv.Value)}";
    }

    void StyleBtn(Button btn)
    {
        btn.AddThemeColorOverride("font_color", new Color(0.82f, 0.78f, 0.74f));
        btn.AddThemeColorOverride("font_hover_color", new Color(0.95f, 0.9f, 0.85f));
        btn.AddThemeColorOverride("font_pressed_color", new Color(0.9f, 0.1f, 0.1f));
        btn.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = new Color(0.1f, 0.04f, 0.05f), BorderColor = new Color(0.35f, 0.08f, 0.08f), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        btn.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = new Color(0.2f, 0.06f, 0.08f), BorderColor = new Color(0.55f, 0.1f, 0.1f), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        btn.AddThemeStyleboxOverride("pressed", new StyleBoxFlat { BgColor = new Color(0.3f, 0.04f, 0.04f), BorderColor = new Color(0.7f, 0.05f, 0.05f), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        btn.AddThemeFontSizeOverride("font_size", 18);
    }
}
