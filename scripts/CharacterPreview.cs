using Godot;

namespace Rouge;

/// Draws a detailed Bloodborne-styled character silhouette per class using _Draw().
/// Victorian-gothic aesthetics, trick weapons, dark tones with accent colors.
public partial class CharacterPreview : Control
{
    private CharacterClass _characterClass = CharacterClass.Knight;

    public void SetClass(CharacterClass cls)
    {
        _characterClass = cls;
        QueueRedraw();
    }

    public override void _Draw()
    {
        Vector2 c = Size / 2;
        float w = Size.X;
        float h = Size.Y;
        float s = Mathf.Min(w, h) / 220f;

        switch (_characterClass)
        {
            case CharacterClass.Knight: DrawVilebloodKnight(c, s); break;
            case CharacterClass.Mage: DrawByrgenwerthScholar(c, s); break;
            case CharacterClass.Rogue: DrawCainhurstAssassin(c, s); break;
            case CharacterClass.Paladin: DrawChurchExecutioner(c, s); break;
            case CharacterClass.Ranger: DrawPowderKegHunter(c, s); break;
            case CharacterClass.Berserker: DrawBeastlyHunter(c, s); break;
            case CharacterClass.Gunslinger: DrawYharnamHunter(c, s); break;
        }
    }

    // ─── VILEBLOOD KNIGHT ─────────────────────────────────────────────
    // Gothic full-plate armor with plumed helm, cruciform greatshield, and
    // Ludwig's Holy Blade-style greatsword. Dark steel with blood-red plume.
    void DrawVilebloodKnight(Vector2 c, float s)
    {
        Color steel = new Color(0.35f, 0.35f, 0.5f);
        Color darkSteel = new Color(0.18f, 0.18f, 0.3f);
        Color trim = new Color(0.5f, 0.08f, 0.1f);
        Color gold = new Color(0.55f, 0.45f, 0.1f);
        Color blade = new Color(0.55f, 0.55f, 0.7f);
        Color darkBg = new Color(0.04f, 0.015f, 0.025f);

        // Cape behind (dark cloth)
        DrawRect(new Rect2(c.X - 50*s, c.Y - 60*s, 100*s, 140*s), darkSteel * 0.6f);
        DrawRect(new Rect2(c.X - 55*s, c.Y - 55*s, 110*s, 10*s), darkSteel * 0.7f);

        // Legs — armored greaves
        DrawRect(new Rect2(c.X - 28*s, c.Y + 55*s, 24*s, 45*s), darkSteel);
        DrawRect(new Rect2(c.X + 4*s, c.Y + 55*s, 24*s, 45*s), darkSteel);
        DrawRect(new Rect2(c.X - 30*s, c.Y + 95*s, 28*s, 8*s), steel); // sabatons
        DrawRect(new Rect2(c.X + 2*s, c.Y + 95*s, 28*s, 8*s), steel);

        // Body — plate armor
        DrawRect(new Rect2(c.X - 40*s, c.Y - 20*s, 80*s, 80*s), darkSteel);
        DrawRect(new Rect2(c.X - 35*s, c.Y - 15*s, 70*s, 70*s), steel);
        // Tabard with cross
        DrawRect(new Rect2(c.X - 22*s, c.Y - 10*s, 44*s, 60*s), new Color(0.12f, 0.04f, 0.06f));
        DrawRect(new Rect2(c.X - 8*s, c.Y - 8*s, 16*s, 56*s), trim); // cross vertical
        DrawRect(new Rect2(c.X - 18*s, c.Y + 12*s, 36*s, 6*s), trim); // cross horizontal
        // Belt
        DrawRect(new Rect2(c.X - 38*s, c.Y + 50*s, 76*s, 8*s), darkSteel * 0.8f);
        DrawRect(new Rect2(c.X - 4*s, c.Y + 48*s, 8*s, 12*s), gold); // buckle

        // Shoulders — pauldrons
        DrawRect(new Rect2(c.X - 55*s, c.Y - 35*s, 110*s, 22*s), darkSteel);
        DrawRect(new Rect2(c.X - 60*s, c.Y - 40*s, 120*s, 8*s), steel);

        // Helmet — gothic armet
        DrawRect(new Rect2(c.X - 30*s, c.Y - 85*s, 60*s, 50*s), darkSteel);
        DrawRect(new Rect2(c.X - 22*s, c.Y - 80*s, 44*s, 40*s), steel);
        // Visor slit
        DrawRect(new Rect2(c.X - 4*s, c.Y - 75*s, 8*s, 20*s), new Color(0.06f, 0.03f, 0.03f));
        DrawRect(new Rect2(c.X - 12*s, c.Y - 72*s, 24*s, 2*s), new Color(0.06f, 0.03f, 0.03f));
        // Plume
        DrawRect(new Rect2(c.X - 6*s, c.Y - 120*s, 12*s, 35*s), trim);
        DrawRect(new Rect2(c.X - 4*s, c.Y - 130*s, 8*s, 15*s), new Color(0.6f, 0.2f, 0.25f));
        DrawRect(new Rect2(c.X - 2*s, c.Y - 140*s, 4*s, 10*s), new Color(0.7f, 0.35f, 0.35f));

        // Greatsword — Ludwig's Holy Blade
        DrawRect(new Rect2(c.X + 50*s, c.Y - 20*s, 10*s, 150*s), blade); // blade
        DrawRect(new Rect2(c.X + 48*s, c.Y - 25*s, 14*s, 12*s), gold); // crossguard
        DrawRect(new Rect2(c.X + 50*s, c.Y + 125*s, 10*s, 25*s), new Color(0.25f, 0.12f, 0.06f)); // grip
        DrawRect(new Rect2(c.X + 50*s, c.Y + 148*s, 10*s, 8*s), gold); // pommel
        // Blade fuller
        DrawRect(new Rect2(c.X + 53*s, c.Y - 15*s, 4*s, 130*s), new Color(0.65f, 0.65f, 0.8f));

        // Greatshield — cruciform tower shield
        DrawRect(new Rect2(c.X - 80*s, c.Y - 40*s, 40*s, 100*s), darkSteel);
        DrawRect(new Rect2(c.X - 76*s, c.Y - 36*s, 32*s, 92*s), steel);
        DrawRect(new Rect2(c.X - 76*s, c.Y - 10*s, 32*s, 4*s), gold); // cross bar
        DrawRect(new Rect2(c.X - 60*s, c.Y - 30*s, 4*s, 80*s), gold); // cross vert
        DrawCircle(c + new Vector2(-62*s, 10*s), 8*s, trim); // center gem
    }

    // ─── BYRGENWERTH SCHOLAR ──────────────────────────────────────────
    // Dark ragged arcane robes, pointed wide-brimmed hat, staff with
    // glowing rune orb. Deep indigo blue with gold-trinned hem.
    void DrawByrgenwerthScholar(Vector2 c, float s)
    {
        Color robe = new Color(0.1f, 0.1f, 0.25f);
        Color darkRobe = new Color(0.06f, 0.06f, 0.18f);
        Color trim = new Color(0.6f, 0.5f, 0.2f);
        Color glow = new Color(0.3f, 0.5f, 1.0f, 0.5f);
        Color brightGlow = new Color(0.5f, 0.7f, 1.0f);

        // Cloak — ragged behind
        DrawRect(new Rect2(c.X - 55*s, c.Y - 40*s, 110*s, 130*s), darkRobe);
        DrawRect(new Rect2(c.X - 60*s, c.Y + 85*s, 120*s, 15*s), robe * 0.8f);

        // Legs — under robes
        DrawRect(new Rect2(c.X - 40*s, c.Y + 50*s, 80*s, 60*s), darkRobe);
        DrawRect(new Rect2(c.X - 45*s, c.Y + 105*s, 90*s, 10*s), trim); // hem trim

        // Body — flowing robes
        DrawRect(new Rect2(c.X - 45*s, c.Y - 35*s, 90*s, 95*s), robe);
        // Robe front opening
        DrawRect(new Rect2(c.X - 30*s, c.Y - 30*s, 60*s, 85*s), darkRobe);
        // Arcane runes
        DrawCircle(c + new Vector2(-15*s, 10*s), 4*s, glow);
        DrawCircle(c + new Vector2(10*s, 25*s), 3*s, glow);
        DrawCircle(c + new Vector2(5*s, -5*s), 3*s, glow);
        // Clasp
        DrawCircle(c + new Vector2(0, -30*s), 8*s, trim);
        DrawCircle(c + new Vector2(0, -30*s), 4*s, brightGlow);

        // Hat — pointed wizard with wide brim
        DrawRect(new Rect2(c.X - 55*s, c.Y - 75*s, 110*s, 6*s), darkRobe * 0.8f); // brim
        DrawRect(new Rect2(c.X - 35*s, c.Y - 100*s, 70*s, 28*s), robe); // body
        DrawRect(new Rect2(c.X - 20*s, c.Y - 140*s, 40*s, 42*s), robe); // spire
        DrawRect(new Rect2(c.X - 8*s, c.Y - 160*s, 16*s, 22*s), darkRobe);
        // Hat band
        DrawRect(new Rect2(c.X - 34*s, c.Y - 100*s, 68*s, 4*s), trim);
        // Arcane rune on hat
        DrawCircle(c + new Vector2(0, -148*s), 6*s, glow);
        DrawCircle(c + new Vector2(0, -148*s), 3*s, brightGlow);

        // Arcane staff
        DrawRect(new Rect2(c.X + 48*s, c.Y - 100*s, 8*s, 220*s), new Color(0.35f, 0.2f, 0.08f));
        // Staff wrappings
        for (int i = 0; i < 6; i++)
            DrawRect(new Rect2(c.X + 46*s, c.Y - 80*s + i * 30*s, 12*s, 4*s), new Color(0.25f, 0.15f, 0.1f));
        // Glowing orb at top
        DrawCircle(c + new Vector2(52*s, -110*s), 20*s, glow);
        DrawCircle(c + new Vector2(52*s, -110*s), 12*s, brightGlow);
        DrawCircle(c + new Vector2(52*s, -110*s), 5*s, new Color(0.8f, 0.9f, 1.0f));
        // Arcane wisps
        DrawCircle(c + new Vector2(40*s, -120*s), 6*s, glow);
        DrawCircle(c + new Vector2(65*s, -115*s), 5*s, glow);
        DrawCircle(c + new Vector2(55*s, -130*s), 4*s, glow);
    }

    // ─── CAINHURST ASSASSIN ───────────────────────────────────────────
    // Elegant noble: top hat, opera cape, frilled collar, rapier+dagger.
    // Deep maroon/black with silver accents and a single glowing eye.
    void DrawCainhurstAssassin(Vector2 c, float s)
    {
        Color dark = new Color(0.08f, 0.02f, 0.04f);
        Color coat = new Color(0.18f, 0.04f, 0.06f);
        Color silver = new Color(0.6f, 0.6f, 0.7f);
        Color blood = new Color(0.6f, 0.08f, 0.1f);
        Color skin = new Color(0.55f, 0.45f, 0.4f);

        // Opera cape behind
        DrawRect(new Rect2(c.X - 55*s, c.Y - 40*s, 110*s, 130*s), dark * 0.8f);
        DrawRect(new Rect2(c.X - 60*s, c.Y - 35*s, 120*s, 10*s), dark);

        // Legs — dark trousers
        DrawRect(new Rect2(c.X - 26*s, c.Y + 55*s, 22*s, 45*s), dark);
        DrawRect(new Rect2(c.X + 4*s, c.Y + 55*s, 22*s, 45*s), dark);
        // Knee boots
        DrawRect(new Rect2(c.X - 28*s, c.Y + 90*s, 26*s, 15*s), new Color(0.2f, 0.12f, 0.08f));
        DrawRect(new Rect2(c.X + 2*s, c.Y + 90*s, 26*s, 15*s), new Color(0.2f, 0.12f, 0.08f));

        // Body — elegant doublet
        DrawRect(new Rect2(c.X - 35*s, c.Y - 20*s, 70*s, 80*s), coat);
        DrawRect(new Rect2(c.X - 28*s, c.Y - 15*s, 56*s, 65*s), dark * 1.2f); // vest
        // Vest buttons
        DrawCircle(c + new Vector2(0, -5*s), 2*s, silver);
        DrawCircle(c + new Vector2(0, 8*s), 2*s, silver);
        DrawCircle(c + new Vector2(0, 20*s), 2*s, silver);
        // Cravat/frills
        DrawRect(new Rect2(c.X - 12*s, c.Y - 22*s, 24*s, 8*s), new Color(0.7f, 0.7f, 0.75f));
        DrawRect(new Rect2(c.X - 8*s, c.Y - 24*s, 16*s, 4*s), new Color(0.8f, 0.8f, 0.85f));
        // Belt
        DrawRect(new Rect2(c.X - 34*s, c.Y + 48*s, 68*s, 6*s), dark);
        DrawRect(new Rect2(c.X - 4*s, c.Y + 46*s, 8*s, 10*s), silver); // buckle

        // Shoulders — short capelet
        DrawRect(new Rect2(c.X - 42*s, c.Y - 30*s, 84*s, 14*s), coat);

        // Head — top hat
        DrawRect(new Rect2(c.X - 30*s, c.Y - 95*s, 60*s, 6*s), new Color(0.04f, 0.02f, 0.02f)); // brim
        DrawRect(new Rect2(c.X - 22*s, c.Y - 118*s, 44*s, 24*s), dark); // hat body
        DrawRect(new Rect2(c.X - 10*s, c.Y - 125*s, 20*s, 8*s), dark); // crown
        // Hat band
        DrawRect(new Rect2(c.X - 22*s, c.Y - 100*s, 44*s, 4*s), blood);
        // Face — pale noble
        DrawCircle(c + new Vector2(0, -82*s), 26*s, skin);
        // Half-mask
        DrawRect(new Rect2(c.X - 20*s, c.Y - 90*s, 40*s, 12*s), dark * 1.4f);
        // Single eye glow
        DrawCircle(c + new Vector2(-4*s, -90*s), 4*s, blood);
        DrawCircle(c + new Vector2(-4*s, -90*s), 2*s, new Color(1, 0.15f, 0.15f));

        // Rapier (right hand)
        DrawRect(new Rect2(c.X + 40*s, c.Y + 5*s, 6*s, 110*s), silver); // blade
        DrawRect(new Rect2(c.X + 38*s, c.Y + 110*s, 10*s, 8*s), new Color(0.55f, 0.45f, 0.1f)); // guard
        DrawRect(new Rect2(c.X + 39*s, c.Y + 115*s, 8*s, 18*s), new Color(0.25f, 0.12f, 0.06f)); // grip
        DrawCircle(c + new Vector2(43*s, 135*s), 4*s, new Color(0.5f, 0.45f, 0.1f)); // pommel
        // Rapier crossguard
        DrawRect(new Rect2(c.X + 35*s, c.Y + 108*s, 16*s, 4*s), silver);

        // Dagger (off-hand, crossed behind)
        DrawRect(new Rect2(c.X - 30*s, c.Y + 15*s, 4*s, 70*s), silver); // blade
        DrawRect(new Rect2(c.X - 32*s, c.Y + 80*s, 8*s, 6*s), silver); // guard
        DrawRect(new Rect2(c.X - 31*s, c.Y + 84*s, 6*s, 14*s), dark); // grip
    }

    // ─── CHURCH EXECUTIONER ────────────────────────────────────────────
    // Golden-white holy knight: white ecclesiastical robes, gold armor
    // trim, mitre-like helm, holy mace with radiant glow.
    void DrawChurchExecutioner(Vector2 c, float s)
    {
        Color white = new Color(0.4f, 0.4f, 0.55f);
        Color brightWhite = new Color(0.6f, 0.6f, 0.75f);
        Color gold = new Color(0.6f, 0.5f, 0.1f);
        Color brightGold = new Color(0.75f, 0.65f, 0.2f);
        Color glow = new Color(0.85f, 0.8f, 0.3f, 0.25f);

        // Cloak behind
        DrawRect(new Rect2(c.X - 52*s, c.Y - 45*s, 104*s, 130*s), white * 0.7f);
        DrawRect(new Rect2(c.X - 56*s, c.Y - 40*s, 112*s, 8*s), white * 0.8f);

        // Legs — white robes
        DrawRect(new Rect2(c.X - 30*s, c.Y + 50*s, 60*s, 55*s), white);
        DrawRect(new Rect2(c.X - 34*s, c.Y + 100*s, 68*s, 12*s), gold); // hem
        DrawRect(new Rect2(c.X - 32*s, c.Y + 112*s, 64*s, 6*s), brightWhite);

        // Body — ecclesiastical robes
        DrawRect(new Rect2(c.X - 40*s, c.Y - 25*s, 80*s, 80*s), white);
        DrawRect(new Rect2(c.X - 34*s, c.Y - 20*s, 68*s, 70*s), brightWhite);
        // Gold chestplate
        DrawRect(new Rect2(c.X - 28*s, c.Y - 15*s, 56*s, 40*s), gold);
        DrawRect(new Rect2(c.X - 22*s, c.Y - 10*s, 44*s, 30*s), brightGold);
        // Cross emblem on chest
        DrawRect(new Rect2(c.X - 6*s, c.Y - 10*s, 12*s, 28*s), brightWhite);
        DrawRect(new Rect2(c.X - 14*s, c.Y + 2*s, 28*s, 4*s), brightWhite);
        // Belt
        DrawRect(new Rect2(c.X - 38*s, c.Y + 45*s, 76*s, 8*s), gold);
        DrawRect(new Rect2(c.X - 5*s, c.Y + 42*s, 10*s, 14*s), brightGold);

        // Shoulders
        DrawRect(new Rect2(c.X - 50*s, c.Y - 40*s, 100*s, 18*s), white);
        DrawRect(new Rect2(c.X - 55*s, c.Y - 45*s, 110*s, 8*s), gold);

        // Helm — mitre-style
        DrawRect(new Rect2(c.X - 28*s, c.Y - 85*s, 56*s, 44*s), white);
        DrawRect(new Rect2(c.X - 20*s, c.Y - 82*s, 40*s, 36*s), brightWhite);
        // Visor
        DrawRect(new Rect2(c.X - 14*s, c.Y - 78*s, 28*s, 4*s), gold);
        DrawRect(new Rect2(c.X - 4*s, c.Y - 80*s, 8*s, 16*s), new Color(0.08f, 0.04f, 0.06f)); // eye slot
        // Mitre points
        DrawCircle(c + new Vector2(-10*s, -92*s), 6*s, gold);
        DrawCircle(c + new Vector2(10*s, -92*s), 6*s, gold);
        DrawRect(new Rect2(c.X - 4*s, c.Y - 108*s, 8*s, 24*s), brightWhite); // center spire
        DrawCircle(c + new Vector2(0, -112*s), 5*s, brightGold);

        // Holy aura
        DrawCircle(c + new Vector2(0, 30*s), 80*s, glow);

        // Holy mace
        DrawRect(new Rect2(c.X + 55*s, c.Y - 80*s, 8*s, 130*s), new Color(0.4f, 0.25f, 0.1f)); // shaft
        // Mace head
        DrawCircle(c + new Vector2(59*s, -90*s), 16*s, gold);
        DrawCircle(c + new Vector2(59*s, -90*s), 10*s, brightGold);
        DrawCircle(c + new Vector2(59*s, -90*s), 6*s, new Color(0.9f, 0.85f, 0.4f)); // core glow
        // Mace spikes
        DrawRect(new Rect2(c.X + 50*s, c.Y - 100*s, 6*s, 6*s), gold);
        DrawRect(new Rect2(c.X + 62*s, c.Y - 100*s, 6*s, 6*s), gold);
        DrawRect(new Rect2(c.X + 56*s, c.Y - 104*s, 6*s, 6*s), gold);
        // Grip
        DrawRect(new Rect2(c.X + 54*s, c.Y + 45*s, 10*s, 18*s), new Color(0.2f, 0.1f, 0.06f));
        DrawRect(new Rect2(c.X + 53*s, c.Y + 40*s, 12*s, 8*s), gold); // guard
    }

    // ─── POWDER KEG HUNTER ────────────────────────────────────────────
    // Hunter in green/brown duster, wide-brimmed hat, bandolier across
    // chest, carrying a crossbow and quiver. Practical, gritty.
    void DrawPowderKegHunter(Vector2 c, float s)
    {
        Color duster = new Color(0.15f, 0.35f, 0.15f);
        Color darkDuster = new Color(0.1f, 0.22f, 0.1f);
        Color leather = new Color(0.3f, 0.18f, 0.1f);
        Color darkLeather = new Color(0.15f, 0.08f, 0.05f);
        Color metal = new Color(0.45f, 0.45f, 0.55f);
        Color skin = new Color(0.6f, 0.5f, 0.4f);

        // Cloak behind
        DrawRect(new Rect2(c.X - 50*s, c.Y - 35*s, 100*s, 120*s), darkDuster);
        DrawRect(new Rect2(c.X - 55*s, c.Y - 30*s, 110*s, 8*s), darkDuster);

        // Legs
        DrawRect(new Rect2(c.X - 24*s, c.Y + 55*s, 20*s, 40*s), darkDuster);
        DrawRect(new Rect2(c.X + 4*s, c.Y + 55*s, 20*s, 40*s), darkDuster);
        // Boots
        DrawRect(new Rect2(c.X - 26*s, c.Y + 90*s, 24*s, 14*s), darkLeather);
        DrawRect(new Rect2(c.X + 2*s, c.Y + 90*s, 24*s, 14*s), darkLeather);

        // Body — duster coat
        DrawRect(new Rect2(c.X - 38*s, c.Y - 18*s, 76*s, 78*s), duster);
        DrawRect(new Rect2(c.X - 32*s, c.Y - 12*s, 64*s, 66*s), darkDuster);
        // Vest
        DrawRect(new Rect2(c.X - 22*s, c.Y - 8*s, 44*s, 50*s), leather);
        // Belt
        DrawRect(new Rect2(c.X - 36*s, c.Y + 48*s, 72*s, 6*s), darkLeather);
        DrawRect(new Rect2(c.X - 4*s, c.Y + 46*s, 8*s, 10*s), metal);
        // Bandolier across chest
        DrawRect(new Rect2(c.X - 28*s, c.Y - 5*s, 56*s, 4*s), darkLeather);
        DrawRect(new Rect2(c.X - 22*s, c.Y - 1*s, 44*s, 4*s), darkLeather);
        // Ammo pouches
        DrawCircle(c + new Vector2(-10*s, -2*s), 5*s, darkLeather);
        DrawCircle(c + new Vector2(5*s, 2*s), 4*s, darkLeather);
        DrawCircle(c + new Vector2(-5*s, 2*s), 4*s, darkLeather);

        // Shoulders — short cape
        DrawRect(new Rect2(c.X - 42*s, c.Y - 30*s, 84*s, 14*s), duster);

        // Head — wide hat
        DrawRect(new Rect2(c.X - 40*s, c.Y - 95*s, 80*s, 5*s), darkLeather); // brim
        DrawRect(new Rect2(c.X - 28*s, c.Y - 118*s, 56*s, 24*s), darkLeather); // crown
        DrawRect(new Rect2(c.X - 14*s, c.Y - 122*s, 28*s, 6*s), darkLeather);
        // Hat band
        DrawRect(new Rect2(c.X - 28*s, c.Y - 98*s, 56*s, 3*s), new Color(0.4f, 0.25f, 0.1f));
        // Face
        DrawCircle(c + new Vector2(0, -82*s), 24*s, skin);
        DrawRect(new Rect2(c.X - 16*s, c.Y - 90*s, 32*s, 10*s), darkDuster); // bandana
        // Eyes
        DrawCircle(c + new Vector2(-6*s, -85*s), 3*s, new Color(0.9f, 0.9f, 0.9f));
        DrawCircle(c + new Vector2(6*s, -85*s), 3*s, new Color(0.9f, 0.9f, 0.9f));

        // Crossbow
        DrawRect(new Rect2(c.X - 60*s, c.Y - 10*s, 40*s, 8*s), new Color(0.35f, 0.2f, 0.08f)); // stock
        DrawRect(new Rect2(c.X - 70*s, c.Y - 12*s, 20*s, 24*s), new Color(0.4f, 0.25f, 0.1f)); // bow body
        DrawArc(c + new Vector2(-70*s, 0), 30*s, -1.8f, 1.8f, 20, metal, 3*s); // bow arc
        DrawLine(c + new Vector2(-80*s, -20*s), c + new Vector2(-20*s, 5*s), metal, 2*s); // bowstring
        // Bolt
        DrawRect(new Rect2(c.X - 50*s, c.Y - 4*s, 30*s, 3*s), new Color(0.55f, 0.55f, 0.65f));

        // Quiver on back
        DrawRect(new Rect2(c.X + 35*s, c.Y - 20*s, 18*s, 80*s), darkLeather);
        DrawRect(new Rect2(c.X + 37*s, c.Y - 16*s, 14*s, 72*s), new Color(0.2f, 0.12f, 0.08f));
        // Arrows in quiver
        for (int i = 0; i < 4; i++)
            DrawLine(c + new Vector2(42*s, -14*s + i * 18*s), c + new Vector2(50*s, -40*s + i * 18*s), new Color(0.55f, 0.55f, 0.55f), 2*s);
    }

    // ─── BEASTLY HUNTER ───────────────────────────────────────────────
    // Feral barbarian: wild red mane, bare muscular chest with warpaint,
    // fur pelt, massive serrated saw cleaver. Blood spatter throughout.
    void DrawBeastlyHunter(Vector2 c, float s)
    {
        Color skin = new Color(0.55f, 0.25f, 0.18f);
        Color darkSkin = new Color(0.4f, 0.18f, 0.12f);
        Color fur = new Color(0.3f, 0.15f, 0.06f);
        Color darkFur = new Color(0.18f, 0.08f, 0.04f);
        Color blood = new Color(0.55f, 0.04f, 0.04f);
        Color brightBlood = new Color(0.8f, 0.08f, 0.08f);
        Color metal = new Color(0.45f, 0.45f, 0.55f);
        Color blade = new Color(0.5f, 0.5f, 0.65f);
        Color eyeGlow = new Color(0.9f, 0.9f, 0.15f);

        // Legs — bare with fur wraps
        DrawRect(new Rect2(c.X - 32*s, c.Y + 55*s, 28*s, 45*s), darkSkin);
        DrawRect(new Rect2(c.X + 4*s, c.Y + 55*s, 28*s, 45*s), darkSkin);
        // Fur leg wraps
        DrawRect(new Rect2(c.X - 34*s, c.Y + 80*s, 32*s, 12*s), fur);
        DrawRect(new Rect2(c.X + 2*s, c.Y + 80*s, 32*s, 12*s), fur);
        // Bare feet
        DrawRect(new Rect2(c.X - 32*s, c.Y + 95*s, 28*s, 8*s), darkSkin);
        DrawRect(new Rect2(c.X + 4*s, c.Y + 95*s, 28*s, 8*s), darkSkin);

        // Fur loincloth
        DrawRect(new Rect2(c.X - 42*s, c.Y + 42*s, 84*s, 20*s), darkFur);
        DrawRect(new Rect2(c.X - 38*s, c.Y + 45*s, 76*s, 14*s), fur);

        // Body — massive muscular torso
        DrawRect(new Rect2(c.X - 46*s, c.Y - 40*s, 92*s, 88*s), skin);
        DrawRect(new Rect2(c.X - 36*s, c.Y - 30*s, 72*s, 70*s), darkSkin * 1.2f);
        // Chest definition
        DrawRect(new Rect2(c.X - 24*s, c.Y - 20*s, 20*s, 30*s), darkSkin);
        DrawRect(new Rect2(c.X + 4*s, c.Y - 20*s, 20*s, 30*s), darkSkin);
        // War paint stripes
        DrawRect(new Rect2(c.X - 8*s, c.Y - 25*s, 2*s, 60*s), blood);
        DrawRect(new Rect2(c.X + 6*s, c.Y - 22*s, 2*s, 55*s), blood);
        DrawRect(new Rect2(c.X - 18*s, c.Y - 18*s, 2*s, 50*s), blood);
        DrawRect(new Rect2(c.X + 16*s, c.Y - 18*s, 2*s, 50*s), blood);
        // Blood splatter on chest
        DrawCircle(c + new Vector2(-10*s, 10*s), 5*s, brightBlood);
        DrawCircle(c + new Vector2(15*s, 15*s), 3*s, brightBlood);
        DrawCircle(c + new Vector2(0, 25*s), 4*s, blood);

        // Fur pelt shoulders
        DrawRect(new Rect2(c.X - 55*s, c.Y - 50*s, 110*s, 20*s), darkFur);
        DrawRect(new Rect2(c.X - 58*s, c.Y - 55*s, 116*s, 8*s), fur);
        DrawRect(new Rect2(c.X - 52*s, c.Y - 45*s, 104*s, 10*s), fur);

        // Head — wild savage
        DrawCircle(c + new Vector2(0, -62*s), 32*s, darkSkin);
        DrawCircle(c + new Vector2(0, -65*s), 30*s, skin);
        // Wild hair — spiky mane
        for (int i = -4; i <= 4; i++)
            DrawRect(new Rect2(c.X + i*12*s - 5*s, c.Y - 110*s, 10*s, 48*s + Mathf.Abs(i)*4*s), blood * 0.8f);
        for (int i = -3; i <= 3; i += 2)
            DrawRect(new Rect2(c.X + i*14*s - 3*s, c.Y - 120*s, 6*s, 20*s), brightBlood * 0.6f);
        // Feral eyes — glowing yellow
        DrawCircle(c + new Vector2(-10*s, -70*s), 5*s, eyeGlow);
        DrawCircle(c + new Vector2(10*s, -70*s), 5*s, eyeGlow);
        DrawCircle(c + new Vector2(-10*s, -70*s), 2*s, new Color(1, 1, 0.3f));
        DrawCircle(c + new Vector2(10*s, -70*s), 2*s, new Color(1, 1, 0.3f));
        // War paint on face
        DrawRect(new Rect2(c.X - 2*s, c.Y - 78*s, 4*s, 18*s), blood);
        DrawRect(new Rect2(c.X - 14*s, c.Y - 72*s, 8*s, 3*s), blood);
        DrawRect(new Rect2(c.X + 6*s, c.Y - 72*s, 8*s, 3*s), blood);
        // Mouth — snarling teeth
        DrawRect(new Rect2(c.X - 10*s, c.Y - 52*s, 20*s, 6*s), new Color(0.08f, 0.04f, 0.04f));
        DrawRect(new Rect2(c.X - 12*s, c.Y - 54*s, 4*s, 2*s), new Color(0.9f, 0.9f, 0.9f)); // teeth
        DrawRect(new Rect2(c.X + 4*s, c.Y - 54*s, 4*s, 2*s), new Color(0.9f, 0.9f, 0.9f));

        // Massive Saw Cleaver
        DrawRect(new Rect2(c.X + 55*s, c.Y - 120*s, 14*s, 180*s), blade); // blade body
        DrawRect(new Rect2(c.X + 58*s, c.Y - 115*s, 8*s, 170*s), metal); // inner
        // Serrated teeth
        for (int i = 0; i < 8; i++)
            DrawRect(new Rect2(c.X + 67*s, c.Y - 105*s + i * 20*s, 10*s, 8*s), metal);
        for (int i = 0; i < 6; i++)
            DrawRect(new Rect2(c.X + 67*s, c.Y - 100*s + i * 24*s, 8*s, 6*s), blade);
        // Blood on blade
        DrawCircle(c + new Vector2(62*s, -30*s), 8*s, blood);
        DrawCircle(c + new Vector2(65*s, 10*s), 6*s, brightBlood);
        DrawRect(new Rect2(c.X + 56*s, c.Y - 50*s, 12*s, 4*s), blood);
        // Handle
        DrawRect(new Rect2(c.X + 56*s, c.Y + 55*s, 12*s, 25*s), darkFur);
        DrawRect(new Rect2(c.X + 54*s, c.Y + 52*s, 16*s, 8*s), metal); // crossguard
        DrawCircle(c + new Vector2(62*s, 85*s), 6*s, metal); // pommel
    }

    // ─── YHARNAM HUNTER ───────────────────────────────────────────────
    // The iconic Bloodborne look: tricorn hat, long duster coat, red scarf,
    // Evelyn pistol in one hand, torch or trick weapon in the other.
    void DrawYharnamHunter(Vector2 c, float s)
    {
        Color coat = new Color(0.1f, 0.05f, 0.06f);
        Color darkCoat = new Color(0.06f, 0.03f, 0.04f);
        Color leather = new Color(0.2f, 0.12f, 0.08f);
        Color darkLeather = new Color(0.12f, 0.07f, 0.05f);
        Color silver = new Color(0.55f, 0.55f, 0.65f);
        Color scarf = new Color(0.55f, 0.06f, 0.08f);
        Color skin = new Color(0.6f, 0.45f, 0.35f);
        Color brass = new Color(0.5f, 0.4f, 0.1f);

        // Long coat behind
        DrawRect(new Rect2(c.X - 55*s, c.Y - 35*s, 110*s, 140*s), darkCoat);
        DrawRect(new Rect2(c.X - 60*s, c.Y - 30*s, 120*s, 10*s), darkCoat);

        // Legs — dark trousers
        DrawRect(new Rect2(c.X - 26*s, c.Y + 55*s, 22*s, 40*s), darkCoat);
        DrawRect(new Rect2(c.X + 4*s, c.Y + 55*s, 22*s, 40*s), darkCoat);
        // Knee-high hunter boots
        DrawRect(new Rect2(c.X - 28*s, c.Y + 88*s, 26*s, 18*s), darkLeather);
        DrawRect(new Rect2(c.X + 2*s, c.Y + 88*s, 26*s, 18*s), darkLeather);
        DrawRect(new Rect2(c.X - 28*s, c.Y + 102*s, 26*s, 4*s), silver); // boot spurs
        DrawRect(new Rect2(c.X + 2*s, c.Y + 102*s, 26*s, 4*s), silver);

        // Body — long duster coat
        DrawRect(new Rect2(c.X - 40*s, c.Y - 20*s, 80*s, 80*s), coat);
        DrawRect(new Rect2(c.X - 34*s, c.Y - 15*s, 68*s, 70*s), darkCoat);
        // Vest
        DrawRect(new Rect2(c.X - 22*s, c.Y - 12*s, 44*s, 55*s), darkLeather);
        // Shirt front (white)
        DrawRect(new Rect2(c.X - 10*s, c.Y - 8*s, 20*s, 30*s), new Color(0.45f, 0.45f, 0.5f));
        // Red scarf — signature!
        DrawRect(new Rect2(c.X - 16*s, c.Y - 22*s, 32*s, 10*s), scarf);
        DrawRect(new Rect2(c.X - 12*s, c.Y - 18*s, 24*s, 6*s), new Color(0.7f, 0.1f, 0.12f));
        // Scarf tail
        DrawRect(new Rect2(c.X + 8*s, c.Y - 10*s, 6*s, 20*s), scarf);
        // Belt
        DrawRect(new Rect2(c.X - 38*s, c.Y + 50*s, 76*s, 8*s), darkLeather);
        // Belt pouches
        DrawRect(new Rect2(c.X - 30*s, c.Y + 50*s, 10*s, 12*s), leather);
        DrawRect(new Rect2(c.X + 20*s, c.Y + 50*s, 10*s, 12*s), leather);
        DrawRect(new Rect2(c.X - 4*s, c.Y + 48*s, 8*s, 12*s), brass); // buckle

        // Shoulders
        DrawRect(new Rect2(c.X - 44*s, c.Y - 32*s, 88*s, 14*s), coat);
        DrawRect(new Rect2(c.X - 42*s, c.Y - 28*s, 84*s, 8*s), darkLeather);

        // Coat tails
        DrawRect(new Rect2(c.X - 38*s, c.Y + 55*s, 76*s, 30*s), coat);
        DrawRect(new Rect2(c.X - 34*s, c.Y + 80*s, 68*s, 12*s), darkCoat);

        // Head — tricorn hat
        DrawRect(new Rect2(c.X - 36*s, c.Y - 96*s, 72*s, 5*s), darkCoat); // brim
        DrawRect(new Rect2(c.X - 28*s, c.Y - 118*s, 56*s, 22*s), darkCoat); // crown
        DrawRect(new Rect2(c.X - 24*s, c.Y - 122*s, 48*s, 6*s), darkCoat);
        // Tricorn folds
        DrawRect(new Rect2(c.X - 34*s, c.Y - 98*s, 10*s, 3*s), darkCoat * 1.2f);
        DrawRect(new Rect2(c.X + 24*s, c.Y - 98*s, 10*s, 3*s), darkCoat * 1.2f);
        // Hat band
        DrawRect(new Rect2(c.X - 28*s, c.Y - 100*s, 56*s, 4*s), scarf);
        // Hat buckle
        DrawRect(new Rect2(c.X - 4*s, c.Y - 102*s, 8*s, 6*s), brass);
        // Face
        DrawCircle(c + new Vector2(0, -84*s), 24*s, skin);
        // Bandana
        DrawRect(new Rect2(c.X - 16*s, c.Y - 92*s, 32*s, 10*s), darkCoat);
        // Eye glint
        DrawCircle(c + new Vector2(-4*s, -88*s), 3*s, silver);
        DrawCircle(c + new Vector2(4*s, -88*s), 3*s, silver);

        // Evelyn pistol (right hand, pointed up)
        DrawRect(new Rect2(c.X + 30*s, c.Y + 18*s, 20*s, 8*s), silver); // barrel
        DrawRect(new Rect2(c.X + 48*s, c.Y + 16*s, 6*s, 12*s), brass); // muzzle tip
        DrawRect(new Rect2(c.X + 28*s, c.Y + 24*s, 16*s, 12*s), darkLeather); // grip
        DrawRect(new Rect2(c.X + 44*s, c.Y + 20*s, 4*s, 16*s), darkLeather); // hammer
        DrawRect(new Rect2(c.X + 28*s, c.Y + 22*s, 20*s, 4*s), brass); // frame
        // Trigger guard
        DrawRect(new Rect2(c.X + 30*s, c.Y + 28*s, 8*s, 6*s), silver);
        // Muzzle flash
        DrawCircle(c + new Vector2(52*s, 20*s), 8*s, new Color(1, 0.8f, 0.2f, 0.5f));
        DrawCircle(c + new Vector2(54*s, 20*s), 4*s, new Color(1, 0.9f, 0.4f, 0.7f));

        // Hunter's torch (left hand)
        DrawRect(new Rect2(c.X - 65*s, c.Y - 10*s, 6*s, 40*s), darkLeather); // handle
        DrawCircle(c + new Vector2(-62*s, -20*s), 10*s, new Color(0.8f, 0.4f, 0.05f, 0.6f)); // fire
        DrawCircle(c + new Vector2(-62*s, -22*s), 6*s, new Color(1, 0.7f, 0.1f, 0.5f)); // fire inner
        DrawCircle(c + new Vector2(-62*s, -25*s), 3*s, new Color(1, 0.9f, 0.4f, 0.4f)); // fire core
    }
}
