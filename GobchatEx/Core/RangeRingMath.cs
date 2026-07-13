using System;
using System.Numerics;

namespace GobchatEx.Core;

/// <summary>
/// Geometry and timing for the Range tab's transient in-game preview rings: flat ground circles
/// at the configured fade-out and cut-off radii, shown for ~8 seconds after the button press and
/// gone without a trace. Pure math (the Dalamud layer projects the sampled world points to the
/// screen) so the radius and lifetime promises stay unit-testable.
/// </summary>
public static class RangeRingMath
{
    /// <summary>Total preview lifetime from button press to fully gone.</summary>
    public const long DisplayDurationMs = 8000;

    /// <summary>The tail of the lifetime over which the rings fade to transparent.</summary>
    public const long FadeDurationMs = 1000;

    /// <summary>
    /// Alpha multiplier over the preview's lifetime: fully opaque until the final
    /// <see cref="FadeDurationMs"/>, then a linear ramp to 0 at <see cref="DisplayDurationMs"/>.
    /// Never negative, so callers can multiply it into a color unconditionally.
    /// </summary>
    public static float FadeAlpha(long elapsedMs)
    {
        var remainingMs = DisplayDurationMs - elapsedMs;
        if (remainingMs <= 0)
            return 0f;
        if (remainingMs >= FadeDurationMs)
            return 1f;
        return remainingMs / (float)FadeDurationMs;
    }

    /// <summary>
    /// The <paramref name="index"/>-th of <paramref name="segments"/> evenly spaced points on a
    /// flat circle in the XZ plane (the ground) around <paramref name="center"/>. An index equal
    /// to <paramref name="segments"/> wraps to exactly the first point (modulo, not a 2π angle),
    /// so sampling 0..segments inclusive closes the ring without a floating-point seam.
    /// </summary>
    public static Vector3 PointOnRing(Vector3 center, float radius, int index, int segments)
    {
        var angle = 2 * MathF.PI * (index % segments) / segments;
        return new Vector3(
            center.X + radius * MathF.Cos(angle),
            center.Y,
            center.Z + radius * MathF.Sin(angle));
    }
}
