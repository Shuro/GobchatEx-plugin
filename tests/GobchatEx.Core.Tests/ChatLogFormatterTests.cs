using System;
using System.Globalization;
using GobchatEx.Core;

namespace GobchatEx.Core.Tests;

/// <summary>
/// ChatLogFormatter renders one chat message into one log line from a token template, ported from
/// the app's CustomChatLogger format engine. WHY this matters: log files are a long-term archive —
/// the default line shape (and its invariant, offset-carrying timestamps) must stay byte-stable
/// across plugin versions and user locales, and a hand-edited template from chatlog.json must
/// never crash the chat pipeline (unknown tokens and stray braces render literally).
/// </summary>
public sealed class ChatLogFormatterTests
{
    private static readonly DateTimeOffset Timestamp =
        new(2026, 7, 11, 21, 5, 7, TimeSpan.FromHours(2));

    private static ChatLogEntry Entry(string sender = "John Gobchat", string message = "hello")
        => new(Timestamp, "Say", sender, message);

    [Fact]
    public void Format_DefaultFormat_RendersTheArchivedLineShape()
    {
        // The default template every user's archive is written with; pinned end-to-end.
        var formatter = new ChatLogFormatter("{channel} [{date} {time-full}] {sender}: {message}");
        formatter.Format(Entry())
            .Should().Be("Say [2026-07-11 21:05:07+02:00] John Gobchat: hello");
    }

    [Theory]
    [InlineData("{time}", "21:05:07")]
    [InlineData("{time-short}", "21:05")]
    [InlineData("{time-full}", "21:05:07+02:00")] // keeps the local UTC offset for cross-timezone archives
    [InlineData("{date}", "2026-07-11")]
    [InlineData("{channel}", "Say")]
    [InlineData("{sender}", "John Gobchat")]
    [InlineData("{message}", "hello")]
    public void Format_ExpandsEachToken(string format, string expected)
    {
        new ChatLogFormatter(format).Format(Entry()).Should().Be(expected);
    }

    [Fact]
    public void Format_BreakToken_RendersNewline()
    {
        new ChatLogFormatter("a{break}b").Format(Entry())
            .Should().Be($"a{Environment.NewLine}b");
    }

    [Theory]
    [InlineData("{TIME}")]
    [InlineData("{Time}")]
    public void Format_TokenNames_AreCaseInsensitive(string format)
    {
        new ChatLogFormatter(format).Format(Entry()).Should().Be("21:05:07");
    }

    [Fact]
    public void Format_AdjacentTokens_RenderWithoutSeparator()
    {
        new ChatLogFormatter("{date}{time}").Format(Entry()).Should().Be("2026-07-1121:05:07");
    }

    [Theory]
    [InlineData("{bogus}")]     // unknown token
    [InlineData("{time-fully}")] // near-miss of a real token
    public void Format_UnknownTokens_PassThroughLiterally(string format)
    {
        new ChatLogFormatter(format).Format(Entry()).Should().Be(format);
    }

    [Fact]
    public void Format_LiteralBraces_RenderVerbatimWithoutThrowing()
    {
        // Regression vs the app's string.Format-based engine, which threw on stray braces in a
        // hand-edited template. "{x}" is an unknown token inside "{{x}}" and passes through.
        new ChatLogFormatter("a { b } {{x}} {sender}").Format(Entry())
            .Should().Be("a { b } {{x}} John Gobchat");
    }

    [Fact]
    public void Format_EmptySender_RendersEmptyInPlace()
    {
        // Senderless channels (Echo, system) produce an empty sender; the line keeps its shape.
        new ChatLogFormatter("{sender}: {message}").Format(Entry(sender: ""))
            .Should().Be(": hello");
    }

    [Fact]
    public void Format_IsInvariantUnderGermanCulture()
    {
        // Timestamps must not pick up locale separators (de-DE uses "." in dates) — archives from a
        // German client must match archives from an English one.
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            new ChatLogFormatter("{date} {time-full}").Format(Entry())
                .Should().Be("2026-07-11 21:05:07+02:00");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
