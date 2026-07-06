using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;

namespace GobchatEx.Windows;

/// <summary>
/// Small shared widgets for the settings tabs: accent-colored section
/// headers (Dalamud's ImGui bindings have no SeparatorText), labelled
/// green/red toggle switches, and Ctrl+Shift-gated destructive buttons.
/// </summary>
internal static class SettingsUi
{
    // Toggle track colors: green = on, red = off (hover variants slightly
    // brighter). Deliberately muted so a rail full of switches doesn't scream.
    private static readonly Vector4 ToggleOnTrack = new(0.10f, 0.60f, 0.25f, 1f);
    private static readonly Vector4 ToggleOnTrackHover = new(0.12f, 0.72f, 0.30f, 1f);
    private static readonly Vector4 ToggleOffTrack = new(0.65f, 0.18f, 0.18f, 1f);
    private static readonly Vector4 ToggleOffTrackHover = new(0.78f, 0.22f, 0.22f, 1f);

    public static void SectionHeader(string label, string? help = null)
    {
        ImGui.TextColored(ImGuiColors.DalamudOrange, label);
        if (help != null)
            ImGuiComponents.HelpMarker(help);
        ImGui.Separator();
    }

    /// <summary>
    /// An orange warning line — exclamation triangle plus wrapped text — for
    /// "feature unavailable" notices that should read as a warning instead of
    /// dimmed body text.
    /// </summary>
    public static void Warning(string text)
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.TextColored(ImGuiColors.DalamudOrange, FontAwesomeIcon.ExclamationTriangle.ToIconString());
        ImGui.SameLine();
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudOrange))
            ImGui.TextWrapped(text);
    }

    /// <summary>Matches <see cref="ToggleSwitch"/>'s width for layout math.</summary>
    public static float ToggleWidth() => ImGui.GetFrameHeight() * 1.55f;

    /// <summary>
    /// A toggle switch with a green (on) / red (off) track. Same geometry as
    /// <see cref="ImGuiComponents.ToggleButton"/>, drawn ourselves because
    /// Dalamud's hardcodes its gray track colors. Colors go through
    /// <see cref="ImGui.GetColorU32(Vector4)"/> so ImRaii.Disabled dims the
    /// switch like any other widget. Returns true when the value changed.
    /// </summary>
    public static bool ToggleSwitch(string id, ref bool value)
    {
        var p = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        var height = ImGui.GetFrameHeight();
        var width = ToggleWidth();
        var radius = height * 0.50f;

        var changed = false;
        ImGui.InvisibleButton(id, new Vector2(width, height));
        if (ImGui.IsItemClicked())
        {
            value = !value;
            changed = true;
        }

        var track = ImGui.IsItemHovered()
            ? (value ? ToggleOnTrackHover : ToggleOffTrackHover)
            : (value ? ToggleOnTrack : ToggleOffTrack);

        drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height),
            ImGui.GetColorU32(track), height * 0.50f);
        drawList.AddCircleFilled(
            new Vector2(p.X + radius + ((value ? 1 : 0) * (width - (radius * 2.0f))), p.Y + radius),
            radius - 1.5f, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)));

        return changed;
    }

    /// <summary>
    /// A <see cref="ToggleSwitch"/> with a text label to its right. Returns
    /// true when the value changed. The label itself is not click-sensitive.
    /// </summary>
    public static bool Toggle(string label, ref bool value)
    {
        var changed = ToggleSwitch($"##{label}", ref value);
        ImGui.SameLine();
        ImGui.TextUnformatted(label);
        return changed;
    }

    /// <summary>
    /// A destructive icon-only action gated behind holding Ctrl+Shift, so a stray click can't
    /// lose data. Disabled — with a tooltip explaining the gesture — until both modifiers are
    /// held. Returns true only on a real click while the gate is open.
    /// </summary>
    public static bool DangerButton(FontAwesomeIcon icon, string tooltip)
        => DangerButtonCore(icon, null, tooltip);

    /// <summary>Icon+label variant of <see cref="DangerButton(FontAwesomeIcon, string)"/>.</summary>
    public static bool DangerButton(FontAwesomeIcon icon, string label, string tooltip)
        => DangerButtonCore(icon, label, tooltip);

    private static bool DangerButtonCore(FontAwesomeIcon icon, string? label, string tooltip)
    {
        var canActivate = ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift;
        bool clicked;
        using (ImRaii.Disabled(!canActivate))
        {
            clicked = label is null
                ? ImGuiComponents.IconButton(icon)
                : ImGuiComponents.IconButtonWithText(icon, label);
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using (ImRaii.Tooltip())
                ImGui.TextUnformatted(tooltip);
        }

        return clicked;
    }
}
