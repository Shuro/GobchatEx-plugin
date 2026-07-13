using System.Numerics;
using GobchatEx.Core;

namespace GobchatEx.Core.Tests;

/// <summary>
/// The Range tab's preview button draws transient ground rings at the configured fade-out and
/// cut-off distances. WHY this matters: the rings are a measuring tool — a ring that isn't at
/// exactly the configured radius would misrepresent where chat starts fading, and a preview that
/// outlives its advertised ~8 seconds would break the "no permanent effect" promise.
/// </summary>
public sealed class RangeRingMathTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(4000)]
    [InlineData(7000)] // last hold moment: fade covers only the final second
    public void FadeAlpha_HoldsFullyOpaqueUntilFadeStarts(long elapsedMs)
    {
        // The preview must be clearly visible for the advertised duration, not fading from frame one.
        RangeRingMath.FadeAlpha(elapsedMs).Should().Be(1f);
    }

    [Theory]
    [InlineData(7500, 0.5f)]
    [InlineData(7750, 0.25f)]
    public void FadeAlpha_RampsLinearlyThroughTheFinalSecond(long elapsedMs, float expectedAlpha)
    {
        RangeRingMath.FadeAlpha(elapsedMs).Should().BeApproximately(expectedAlpha, 0.001f);
    }

    [Theory]
    [InlineData(8000)]
    [InlineData(60000)]
    public void FadeAlpha_ZeroAtAndAfterExpiry(long elapsedMs)
    {
        // "No permanent effect": at DisplayDurationMs the rings are gone, however long ago Show ran.
        RangeRingMath.FadeAlpha(elapsedMs).Should().Be(0f);
    }

    [Fact]
    public void PointOnRing_EverySampleSitsAtExactlyTheConfiguredRadius()
    {
        // The ring is a measuring tool: any sample off the radius would misrepresent the distance.
        var center = new Vector3(100f, -7.5f, 250f);

        for (var i = 0; i < 128; i++)
        {
            var point = RangeRingMath.PointOnRing(center, radius: 24f, index: i, segments: 128);
            Vector3.Distance(center, point).Should().BeApproximately(24f, 0.001f);
        }
    }

    [Fact]
    public void PointOnRing_StaysFlatAtTheCentersHeight()
    {
        // Flat ground ring at the player's feet — the Y never changes with the angle.
        var center = new Vector3(3f, 12.25f, -9f);

        for (var i = 0; i < 128; i++)
        {
            var point = RangeRingMath.PointOnRing(center, radius: 16f, index: i, segments: 128);
            point.Y.Should().Be(center.Y);
        }
    }

    [Fact]
    public void PointOnRing_IndexEqualToSegmentsWrapsBackToTheFirstPoint()
    {
        // Callers close the loop by sampling index 0..segments inclusive; the wrap must coincide
        // exactly so the ring has no visible seam.
        var center = new Vector3(1f, 2f, 3f);

        var first = RangeRingMath.PointOnRing(center, radius: 24f, index: 0, segments: 128);
        var wrapped = RangeRingMath.PointOnRing(center, radius: 24f, index: 128, segments: 128);

        Vector3.Distance(first, wrapped).Should().BeApproximately(0f, 0.001f);
    }
}
