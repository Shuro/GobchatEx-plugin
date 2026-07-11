/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
 * Copyright (C) 2026 Shuro
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GobchatEx.Core;

/// <summary>One chat message with its rendering inputs pre-resolved (channel name mapped, sender
/// flattened to text) so the formatter stays Dalamud-free.</summary>
public sealed record ChatLogEntry(DateTimeOffset Timestamp, string Channel, string Sender, string Message);

/// <summary>
/// Renders one chat message into one log line from a token template, ported from the app's
/// CustomChatLogger format engine ({channel}, {date}, {time-full}, {sender}, {message}, ...).
/// Token names are case-insensitive; unrecognized tokens and stray braces render literally, so a
/// hand-edited template from chatlog.json can never throw at format time (the app's string.Format
/// approach did). All date/time tokens use the invariant culture; {time-full} keeps the local UTC
/// offset so archives from different timezones stay unambiguous.
/// </summary>
public sealed partial class ChatLogFormatter
{
    [GeneratedRegex(@"{(?<name>\w+([_-]\w+)*)}")]
    private static partial Regex TokenRegex();

    private static readonly Dictionary<string, Func<ChatLogEntry, string>> TokensByName = new()
    {
        ["TIME"] = e => e.Timestamp.ToString("HH':'mm':'ss", CultureInfo.InvariantCulture),
        ["TIME-SHORT"] = e => e.Timestamp.ToString("HH':'mm", CultureInfo.InvariantCulture),
        ["TIME-FULL"] = e => e.Timestamp.ToString("HH':'mm':'ssK", CultureInfo.InvariantCulture),
        ["DATE"] = e => e.Timestamp.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture),
        ["CHANNEL"] = e => e.Channel,
        ["SENDER"] = e => e.Sender,
        ["MESSAGE"] = e => e.Message,
        ["BREAK"] = _ => Environment.NewLine,
    };

    /// <summary>Parsed template: literal text runs interleaved with token expansions.</summary>
    private readonly List<(string? Literal, Func<ChatLogEntry, string>? Token)> segments = [];

    public ChatLogFormatter(string format)
    {
        var index = 0;
        foreach (Match match in TokenRegex().Matches(format))
        {
            if (match.Index > index)
                segments.Add((format[index..match.Index], null));

            var name = match.Groups["name"].Value.ToUpperInvariant();
            if (TokensByName.TryGetValue(name, out var token))
                segments.Add((null, token));
            else
                segments.Add((match.Value, null)); // unknown token: keep verbatim
            index = match.Index + match.Length;
        }

        if (index < format.Length)
            segments.Add((format[index..], null));
    }

    public string Format(ChatLogEntry entry)
    {
        var sb = new StringBuilder();
        foreach (var (literal, token) in segments)
            sb.Append(literal ?? token!(entry));
        return sb.ToString();
    }
}
