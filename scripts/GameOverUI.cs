using Godot;
using System.Linq;

namespace Rouge;

public partial class GameOverUI : Control
{
    private Label _titleLabel;
    private Label _statsLabel;
    private Button _retryButton;
    private Button _quitButton;

    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("TitleLabel");
        _statsLabel = GetNode<Label>("StatsLabel");
        _retryButton = GetNode<Button>("RetryButton");
        _quitButton = GetNode<Button>("QuitButton");

        var p = GameManager.Instance.PlayerStats;
        _titleLabel.Text = "YOU DIED";
        _statsLabel.Text = $"Last City: {GameManager.Instance.CurrentCityName}\n" +
            $"Level: {p.Level}\n" +
            $"Cities Cleared: {GameManager.Instance.BossesDefeated.Count(kv => kv.Key.EndsWith("_main") && kv.Value)}\n" +
            $"Gold Collected: {p.Gold}\n\n" +
            "The hunt begins anew...";

        // Style buttons with Bloodborne theme
        StyleBtn(_retryButton);
        StyleBtn(_quitButton);

        _retryButton.Pressed += () =>
        {
            GameManager.Instance.StartNewRun();
            GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
        };

        _quitButton.Pressed += () =>
        {
            GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
        };
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
