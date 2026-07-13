using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using GobchatEx.Core;

namespace GobchatEx.Windows;

/// <summary>
/// Transient in-game preview of the range filter's distances: the Range tab's preview button
/// draws two flat ground rings around the player character — yellow at the fade-out radius,
/// orange at the cut-off — for <see cref="RangeRingMath.DisplayDurationMs"/>, fading away over
/// the final second. Radii are captured at the button press (the tab edits the live config, so
/// they're the values currently on the sliders); pressing again restarts with fresh values.
/// Draws straight onto the main viewport's background draw list from <c>Plugin.DrawUI</c> — not
/// a <see cref="Dalamud.Interface.Windowing.Window"/> — so the rings sit behind every plugin
/// window (the settings window stays readable on top) and only ever appear over the game world.
/// </summary>
internal sealed class RangeRingsOverlay
{
    private const int Segments = 128;

    // A zero-radius ring degenerates to this screen-space dot at the character's feet.
    private const float DotRadius = 4f;

    private static readonly Vector4 FadeOutColor = ImGuiColors.DalamudYellow;
    private static readonly Vector4 CutOffColor = ImGuiColors.DalamudOrange;

    // Screen-space polyline scratch for the run currently being built; +1 because the ring
    // closes by sampling index 0..Segments inclusive (see RangeRingMath.PointOnRing's wrap).
    private readonly Vector2[] runBuffer = new Vector2[Segments + 1];

    private long? shownAtMs;
    private float fadeOutRadius;
    private float cutOffRadius;

    /// <summary>Starts (or restarts) the preview with the given radii in yalms.</summary>
    public void Show(float fadeOut, float cutOff)
    {
        fadeOutRadius = fadeOut;
        cutOffRadius = cutOff;
        shownAtMs = Environment.TickCount64;
    }

    /// <summary>Per-frame draw; a no-op unless a preview is running. Call from UiBuilder.Draw.</summary>
    public void Draw()
    {
        if (shownAtMs is not { } shownAt)
            return;

        var alpha = RangeRingMath.FadeAlpha(Environment.TickCount64 - shownAt);
        if (alpha <= 0f)
        {
            shownAtMs = null;
            return;
        }

        // Player gone (logout, zoning): draw nothing but keep the timer running — it expires on
        // its own, and the rings reappear around the player if they're back within the 8 seconds.
        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var drawList = ImGui.GetBackgroundDrawList(ImGuiHelpers.MainViewport);
        var thickness = 3f * ImGuiHelpers.GlobalScale;

        DrawRing(drawList, local.Position, fadeOutRadius, WithAlpha(FadeOutColor, alpha), thickness);
        DrawRing(drawList, local.Position, cutOffRadius, WithAlpha(CutOffColor, alpha), thickness);
    }

    /// <summary>
    /// One ring: samples the world-space circle, projects each point with WorldToScreen, and
    /// strokes runs of consecutively projectable points as separate polylines — the parts of the
    /// ring behind the camera simply drop out instead of producing lines across the screen.
    /// Points in front of the camera but outside the viewport are kept (inView is ignored) so
    /// the stroke exits cleanly through the screen edge. A yalm label sits above the topmost
    /// visible point of the ring: with a typical over-the-shoulder camera that's the part of the
    /// ring most reliably on screen, while the near arc often falls below the viewport.
    /// A zero radius still draws — as a dot at the character's feet — so "fades from the very
    /// first yalm" is visibly different from nothing being shown.
    /// </summary>
    private void DrawRing(ImDrawListPtr drawList, Vector3 center, float radius, uint color, float thickness)
    {
        // WorldToScreen returns game-window coordinates; the background draw list wants ImGui
        // global coordinates — offset by the main viewport's origin, exactly like
        // QuickbarWindow.TryGetChatTopLeft does for addon positions (zero unless multi-viewport).
        var viewportOrigin = ImGuiHelpers.MainViewport.Pos;

        var runLength = 0;
        Vector2? labelPos = null;

        if (radius <= 0f)
        {
            if (Plugin.GameGui.WorldToScreen(center, out var centerScreen, out _))
            {
                var point = viewportOrigin + centerScreen;
                drawList.AddCircleFilled(point, DotRadius * ImGuiHelpers.GlobalScale, color);
                labelPos = point;
            }
        }
        else
        {
            for (var i = 0; i <= Segments; i++)
            {
                var world = RangeRingMath.PointOnRing(center, radius, i, Segments);
                if (Plugin.GameGui.WorldToScreen(world, out var screen, out _))
                {
                    var point = viewportOrigin + screen;
                    runBuffer[runLength++] = point;
                    if (labelPos == null || point.Y < labelPos.Value.Y)
                        labelPos = point;
                }
                else
                {
                    FlushRun(drawList, ref runLength, color, thickness);
                }
            }

            FlushRun(drawList, ref runLength, color, thickness);
        }

        if (labelPos != null)
        {
            var text = $"{radius:0}";
            // Centered on the ring, lifted just above the stroke.
            var size = ImGui.CalcTextSize(text);
            var gap = 4f * ImGuiHelpers.GlobalScale;
            drawList.AddText(labelPos.Value - new Vector2(size.X / 2f, size.Y + gap), color, text);
        }
    }

    private void FlushRun(ImDrawListPtr drawList, ref int runLength, uint color, float thickness)
    {
        if (runLength >= 2)
            drawList.AddPolyline(ref runBuffer[0], runLength, color, ImDrawFlags.None, thickness);
        runLength = 0;
    }

    private static uint WithAlpha(Vector4 color, float alpha)
        => ImGui.GetColorU32(color with { W = color.W * alpha });
}
