using System;
using System.Numerics;
using GobchatEx.Localization;

namespace GobchatEx.Windows;

/// <summary>
/// The four window-frame color slots a theme overrides. TitleBg and
/// TitleBgActive are the endpoints of the window host's focus fade.
/// TitleBgCollapsed is kept for completeness even though current Dalamud
/// shadows it with its own lerped push (WindowHost.DrawInternal).
/// </summary>
internal readonly record struct FrameColors(
    Vector4 WindowBg, Vector4 TitleBg, Vector4 TitleBgActive, Vector4 TitleBgCollapsed);

/// <summary>
/// One selectable color scheme for the settings window itself (background +
/// title bar, and optionally the toggle tracks), applied live by
/// SettingsWindow.PreDraw so switching previews instantly. Only the frame
/// slots are overridden — all widget colors still come from the user's
/// Dalamud theme. Themes are identified by <see cref="Id"/>, persisted as
/// GeneralConfig.WindowThemeId, so display names can change freely without
/// affecting saved configs. Adding a theme takes two touches: an entry in
/// <see cref="All"/> with a freshly generated Guid, and its display name in
/// Language.resx (plus a de entry translating only the parenthesized color
/// descriptor — the proper-noun part stays untranslated, Loc falls back per
/// key).
/// </summary>
internal sealed record SettingsWindowTheme(
    Guid Id,
    string NameKey,
    FrameColors? Frame = null,
    SettingsUi.TogglePalette? Tracks = null)
{
    /// <summary>Display name, resolved per use so it follows the UI language.</summary>
    public string Name => Loc.Get(NameKey);

    /// <summary>
    /// Every selectable theme, in display order. Dalamud Theme (Guid.Empty,
    /// no overrides) leads: GeneralConfig.WindowThemeId's natural default
    /// value selects it without Config/ referencing this presentation-layer
    /// type.
    /// </summary>
    internal static readonly SettingsWindowTheme[] All =
    [
        new(Guid.Empty, "WindowTheme_Option_Dalamud"),

        // Yojiberry: a dusty pastel pink background kept dark enough that
        // the theme's light text stays readable, and a fuller pink title bar.
        // Toggle tracks are pastel green/red versions of the defaults,
        // matching the pastel look while keeping on/off distinguishable by hue.
        new(new Guid("8365f08a-4126-483d-af1e-681b7123bfbc"), "WindowTheme_Option_Yojiberry",
            new FrameColors(
                WindowBg: new(0.55f, 0.36f, 0.41f, 0.94f),
                TitleBg: new(0.55f, 0.22f, 0.35f, 0.90f),
                TitleBgActive: new(0.78f, 0.33f, 0.50f, 1f),
                TitleBgCollapsed: new(0.55f, 0.22f, 0.35f, 0.75f)),
            new SettingsUi.TogglePalette(
                On: new(0.55f, 0.82f, 0.55f, 1f),
                OnHover: new(0.62f, 0.89f, 0.62f, 1f),
                Off: new(0.91f, 0.49f, 0.46f, 1f),
                OffHover: new(0.96f, 0.58f, 0.55f, 1f))),

        // Ginoberry: "green darkmode" — a near-black green background with a
        // dark golden title bar: deep muted gold unfocused, stepping up to a
        // richer (still dark) gold when the window is focused.
        // No track palette (no pastels).
        new(new Guid("1b03f069-895d-4882-ab4e-eac29406e9c1"), "WindowTheme_Option_Ginoberry",
            new FrameColors(
                WindowBg: new(0.03f, 0.06f, 0.04f, 0.95f),
                TitleBg: new(0.18f, 0.13f, 0.04f, 0.92f),
                TitleBgActive: new(0.42f, 0.31f, 0.09f, 1f),
                TitleBgCollapsed: new(0.18f, 0.13f, 0.04f, 0.75f))),

        // Soft Octopus: the Yojiberry recipe in lavender — dusty purple
        // background, fuller purple title bar. Tracks are the same pastel
        // green/red idea shifted cooler (mint on, rose off) to sit well on
        // the lavender window.
        new(new Guid("101391d6-245e-4885-b342-ab68f126c8e9"), "WindowTheme_Option_SoftOctopus",
            new FrameColors(
                WindowBg: new(0.47f, 0.38f, 0.55f, 0.94f),
                TitleBg: new(0.40f, 0.24f, 0.55f, 0.90f),
                TitleBgActive: new(0.58f, 0.36f, 0.80f, 1f),
                TitleBgCollapsed: new(0.40f, 0.24f, 0.55f, 0.75f)),
            new SettingsUi.TogglePalette(
                On: new(0.53f, 0.82f, 0.64f, 1f),
                OnHover: new(0.60f, 0.89f, 0.71f, 1f),
                Off: new(0.90f, 0.52f, 0.58f, 1f),
                OffHover: new(0.95f, 0.60f, 0.66f, 1f))),

        // Octopus: near-black plum background and a deep dark purple title
        // bar. Deliberately no track palette — this theme has no pastels,
        // and the stock green/red tracks already read dark.
        new(new Guid("f80fcdc2-444a-4a45-acd2-600f9c45a58a"), "WindowTheme_Option_Octopus",
            new FrameColors(
                WindowBg: new(0.12f, 0.08f, 0.18f, 0.95f),
                TitleBg: new(0.10f, 0.05f, 0.16f, 0.92f),
                TitleBgActive: new(0.32f, 0.16f, 0.48f, 1f),
                TitleBgCollapsed: new(0.10f, 0.05f, 0.16f, 0.75f))),

        // Ruby: the Octopus recipe in gemstone red — near-black crimson
        // background (a touch of blue in the red, like the stone), darker
        // crimson title bar, rich ruby when focused.
        // No track palette (no pastels).
        new(new Guid("25cfed31-1ea9-48d8-ba07-905efdeec19c"), "WindowTheme_Option_Ruby",
            new FrameColors(
                WindowBg: new(0.14f, 0.04f, 0.06f, 0.95f),
                TitleBg: new(0.11f, 0.03f, 0.05f, 0.92f),
                TitleBgActive: new(0.35f, 0.06f, 0.12f, 1f),
                TitleBgCollapsed: new(0.11f, 0.03f, 0.05f, 0.75f))),
    ];

    /// <summary>
    /// Resolves a persisted theme id. Unknown ids (a theme removed in an
    /// update, a hand-edited config) fall back to the Dalamud Theme instead
    /// of failing.
    /// </summary>
    internal static SettingsWindowTheme ById(Guid id)
    {
        foreach (var theme in All)
        {
            if (theme.Id == id)
                return theme;
        }

        return All[0];
    }
}
