using Godot;
using System.Linq;

namespace Rouge;

public partial class CharacterSelect : Control
{
    private GridContainer _cardGrid;
    private Label _infoLabel;
    private Label _statsLabel;
    private Label _gearLabel;
    private Label _descLabel;
    private Button _confirmButton;
    private Button _backButton;

    private CharacterClass _selectedClass;
    private ColorRect[] _cardBorders;
    private bool _confirmed;
    private CharacterPreview _preview;

    public override void _Ready()
    {
        _cardGrid = GetNode<GridContainer>("CardGrid");
        _infoLabel = GetNode<Label>("InfoPanel/InfoLabel");
        _statsLabel = GetNode<Label>("InfoPanel/StatsLabel");
        _gearLabel = GetNode<Label>("InfoPanel/GearLabel");
        _descLabel = GetNode<Label>("InfoPanel/DescLabel");
        _confirmButton = GetNode<Button>("InfoPanel/ConfirmButton");
        _backButton = GetNode<Button>("InfoPanel/BackButton");

        _cardBorders = new ColorRect[7];
        BuildCards();

        // Create character preview panel (centered above info panel)
        var previewPanel = new ColorRect();
        previewPanel.Name = "PreviewPanel";
        previewPanel.AnchorLeft = 0.46f;
        previewPanel.AnchorTop = 0.05f;
        previewPanel.AnchorRight = 0.58f;
        previewPanel.AnchorBottom = 0.38f;
        previewPanel.Color = new Color(0.04f, 0.015f, 0.025f, 0.9f);
        _preview = new CharacterPreview();
        _preview.Size = new Vector2(140, 220);
        _preview.Position = new Vector2(5, 5);
        AddChild(previewPanel);
        previewPanel.AddChild(_preview);
        _preview.SetClass(CharacterClass.Knight);

        _selectedClass = CharacterClass.Knight;
        HighlightCard(0);
        ShowInfo(CharacterClass.Knight);

        _confirmButton.Pressed += () =>
        {
            _confirmed = true;
            GameManager.Instance.SelectCharacter(_selectedClass);
            GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
        };

        _backButton.Pressed += () =>
        {
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        };

        StyleBtn(_confirmButton);
        StyleBtn(_backButton);

        // Keyboard/controller: arrow keys cycle through characters
        // We handle this via input
        _cardGrid.MouseFilter = Control.MouseFilterEnum.Pass;
    }

    public override void _Input(InputEvent @event)
    {
        if (_confirmed) return;

        if (@event.IsActionPressed("ui_right") || @event.IsActionPressed("move_right"))
        {
            int idx = ((int)_selectedClass + 1) % 7;
            _selectedClass = (CharacterClass)idx;
            HighlightCard(idx);
            ShowInfo(_selectedClass);
            _preview.SetClass(_selectedClass);
            AcceptEvent();
        }
        else if (@event.IsActionPressed("ui_left") || @event.IsActionPressed("move_left"))
        {
            int idx = ((int)_selectedClass + 6) % 7;
            _selectedClass = (CharacterClass)idx;
            HighlightCard(idx);
            ShowInfo(_selectedClass);
            _preview.SetClass(_selectedClass);
            AcceptEvent();
        }
        else if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("jump"))
        {
            _confirmed = true;
            GameManager.Instance.SelectCharacter(_selectedClass);
            GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        }
    }

    void BuildCards()
    {
        foreach (var c in _cardGrid.GetChildren().OfType<Node>())
            c.QueueFree();

        var classes = new (CharacterClass cls, string name, Color color)[]
        {
            (CharacterClass.Knight, "Knight", new Color(0.5f, 0.5f, 0.8f)),
            (CharacterClass.Mage, "Mage", new Color(0.3f, 0.5f, 1.0f)),
            (CharacterClass.Rogue, "Rogue", new Color(0.4f, 0.8f, 0.4f)),
            (CharacterClass.Paladin, "Paladin", new Color(0.9f, 0.8f, 0.3f)),
            (CharacterClass.Ranger, "Ranger", new Color(0.2f, 0.7f, 0.3f)),
            (CharacterClass.Berserker, "Berserker", new Color(0.9f, 0.2f, 0.2f)),
            (CharacterClass.Gunslinger, "Gunslinger", new Color(0.8f, 0.6f, 0.2f)),
        };

        for (int i = 0; i < classes.Length; i++)
        {
            var card = new MarginContainer();
            card.CustomMinimumSize = new Vector2(170, 220);
            card.AddThemeConstantOverride("margin_left", 4);
            card.AddThemeConstantOverride("margin_right", 4);
            card.AddThemeConstantOverride("margin_top", 4);
            card.AddThemeConstantOverride("margin_bottom", 4);

            var border = new ColorRect();
            border.Size = new Vector2(170, 220);
            border.Color = new Color(0.15f, 0.06f, 0.08f);
            border.MouseFilter = Control.MouseFilterEnum.Ignore;

            var bg = new ColorRect();
            bg.Size = new Vector2(158, 208);
            bg.Position = new Vector2(6, 6);
            bg.Color = new Color(0.08f, 0.03f, 0.04f);
            bg.MouseFilter = Control.MouseFilterEnum.Ignore;

            int idx = i;
            var cls = classes[i];

            // Clickable area
            var clickArea = new ColorRect();
            clickArea.Size = new Vector2(170, 220);
            clickArea.Color = Colors.Transparent;
            clickArea.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
            clickArea.GuiInput += (ev) =>
            {
                if (ev is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                {
                    _selectedClass = cls.cls;
                    HighlightCard(idx);
                    ShowInfo(cls.cls);
                    _preview.SetClass(cls.cls);
                    AcceptEvent();
                }
            };

            var nameLabel = new Label();
            nameLabel.Position = new Vector2(8, 12);
            nameLabel.Text = cls.name;
            nameLabel.AddThemeColorOverride("font_color", cls.color);
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Emoji/symbol placeholder for portrait
            var portrait = new ColorRect();
            portrait.Position = new Vector2(20, 50);
            portrait.Size = new Vector2(120, 120);
            portrait.Color = cls.color * new Color(1, 1, 1, 0.2f);
            portrait.MouseFilter = Control.MouseFilterEnum.Ignore;

            // Preview stats
            var preview = new Label();
            preview.Position = new Vector2(8, 185);
            preview.MouseFilter = Control.MouseFilterEnum.Ignore;
            var chr = GameManager.AllCharacters[cls.cls];
            preview.Text = $"HP:{chr.BaseHP} MP:{chr.BaseMP} ATK:{chr.BaseATK} DEF:{chr.BaseDEF} SPD:{chr.BaseSPD}";
            preview.AddThemeColorOverride("font_color", new Color(0.7f, 0.65f, 0.6f));
            preview.AddThemeFontSizeOverride("font_size", 9);

            card.AddChild(border);
            card.AddChild(bg);
            card.AddChild(clickArea);
            card.AddChild(nameLabel);
            card.AddChild(portrait);
            card.AddChild(preview);

            _cardGrid.AddChild(card);
            _cardBorders[i] = border;
        }
    }

    void HighlightCard(int index)
    {
        for (int i = 0; i < _cardBorders.Length; i++)
        {
            _cardBorders[i].Color = i == index
                ? new Color(0.6f, 0.07f, 0.08f)
                : new Color(0.15f, 0.06f, 0.08f);
        }
    }

    void ShowInfo(CharacterClass cls)
    {
        var chr = GameManager.AllCharacters[cls];
        _infoLabel.Text = chr.Name;
        _infoLabel.AddThemeColorOverride("font_color", chr.ClassColor());
        _descLabel.Text = chr.Description;
        _statsLabel.Text = $"HP: {chr.BaseHP}  MP: {chr.BaseMP}\nATK: {chr.BaseATK}  DEF: {chr.BaseDEF}  SPD: {chr.BaseSPD}";
        _gearLabel.Text = $"Weapon: {chr.StartingWeapon.Name} ({chr.StartingWeapon.Rarity})  +{chr.StartingWeapon.BonusATK} ATK\n" +
            $"Armor: {chr.StartingArmor.Name} ({chr.StartingArmor.Rarity})  +{chr.StartingArmor.BonusDEF} DEF\n" +
            $"Accessory: {chr.StartingAccessory.Name} ({chr.StartingAccessory.Rarity})\n" +
            $"Skills: {string.Join(", ", chr.StartingSkills)}";
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
