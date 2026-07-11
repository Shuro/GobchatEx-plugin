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
using System.Globalization;
using System.Text;

namespace GobchatEx.Core;

/// <summary>
/// File and folder naming for the chat logger (Milestone 5), ported from the app's ChatLoggerBase:
/// character names are reduced to file-system-safe tokens (FFXIV names contain apostrophes), and
/// each session file is named with an invariant, minute-precision timestamp so archives sort
/// chronologically regardless of the user's locale.
/// </summary>
public static class ChatLogNaming
{
    /// <summary>
    /// Reduces a character name to a filename-safe token: letters/digits kept, whitespace and
    /// hyphens become a single '-', everything else (apostrophes, punctuation) dropped. E.g.
    /// "J'ohn Gobchat" -> "John-Gobchat".
    /// </summary>
    public static string SanitizeForFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var sb = new StringBuilder(name.Length);
        foreach (var ch in name.Trim())
        {
            char toAppend;
            if (char.IsLetterOrDigit(ch))
                toAppend = ch;
            else if (ch == '-' || char.IsWhiteSpace(ch))
                toAppend = '-';
            else
                continue; // drop apostrophes, punctuation, invalid path chars

            if (toAppend == '-' && (sb.Length == 0 || sb[sb.Length - 1] == '-'))
                continue; // collapse runs, no leading hyphen
            sb.Append(toAppend);
        }

        while (sb.Length > 0 && sb[sb.Length - 1] == '-')
            sb.Length--; // no trailing hyphen
        return sb.ToString();
    }

    /// <summary>
    /// Like <see cref="SanitizeForFileName"/> but keeps spaces, so the result reads as a folder name:
    /// letters/digits and single spaces are kept, runs of whitespace collapse to one space, and
    /// everything else (apostrophes, punctuation, invalid path chars) is dropped. E.g.
    /// "J'ohn Gobchat" -> "John Gobchat".
    /// </summary>
    public static string SanitizeForFolderName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var sb = new StringBuilder(name.Length);
        foreach (var ch in name.Trim())
        {
            char toAppend;
            if (char.IsLetterOrDigit(ch))
                toAppend = ch;
            else if (char.IsWhiteSpace(ch))
                toAppend = ' ';
            else
                continue; // drop apostrophes, punctuation, invalid path chars

            if (toAppend == ' ' && (sb.Length == 0 || sb[sb.Length - 1] == ' '))
                continue; // collapse runs, no leading space
            sb.Append(toAppend);
        }

        while (sb.Length > 0 && sb[sb.Length - 1] == ' ')
            sb.Length--; // no trailing space
        return sb.ToString();
    }

    /// <summary>
    /// Builds the per-session log filename: <c>chatlog_{yyyy-MM-dd_HH-mm}[_{Character}].log</c>,
    /// the character suffix omitted when the name sanitizes away entirely.
    /// </summary>
    public static string BuildFileName(DateTimeOffset now, string? characterName)
    {
        var timestamp = now.ToString("yyyy-MM-dd_HH-mm", CultureInfo.InvariantCulture);
        var character = SanitizeForFileName(characterName);
        return character.Length == 0
            ? $"chatlog_{timestamp}.log"
            : $"chatlog_{timestamp}_{character}.log";
    }
}
