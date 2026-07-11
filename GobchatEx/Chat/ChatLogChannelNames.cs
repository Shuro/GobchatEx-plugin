using System.Collections.Generic;
using Dalamud.Game.Text;

namespace GobchatEx.Chat;

/// <summary>
/// Maps a chat channel to the stable name the {channel} log token renders. Deliberately its own
/// table rather than <see cref="XivChatType"/>.ToString(): log files are a long-term archive, so
/// the names must not shift if Dalamud ever renames enum members, and the short enum names
/// (Ls1, CrossLinkShell1) read poorly in a text file. These are the plugin's clean successors to
/// the app's channel names (TellIncoming, not the app's intentional "TellRecieve" misspelling).
/// </summary>
internal static class ChatLogChannelNames
{
    private static readonly Dictionary<XivChatType, string> Names = new()
    {
        [XivChatType.Say] = "Say",
        [XivChatType.CustomEmote] = "Emote",
        [XivChatType.StandardEmote] = "StandardEmote",
        [XivChatType.Yell] = "Yell",
        [XivChatType.Shout] = "Shout",
        [XivChatType.TellIncoming] = "TellIncoming",
        [XivChatType.TellOutgoing] = "TellOutgoing",
        [XivChatType.Party] = "Party",
        [XivChatType.CrossParty] = "CrossParty",
        [XivChatType.Alliance] = "Alliance",
        [XivChatType.FreeCompany] = "FreeCompany",
        [XivChatType.NoviceNetwork] = "NoviceNetwork",
        [XivChatType.Echo] = "Echo",
        [XivChatType.Ls1] = "Linkshell1",
        [XivChatType.Ls2] = "Linkshell2",
        [XivChatType.Ls3] = "Linkshell3",
        [XivChatType.Ls4] = "Linkshell4",
        [XivChatType.Ls5] = "Linkshell5",
        [XivChatType.Ls6] = "Linkshell6",
        [XivChatType.Ls7] = "Linkshell7",
        [XivChatType.Ls8] = "Linkshell8",
        [XivChatType.CrossLinkShell1] = "CrossworldLinkshell1",
        [XivChatType.CrossLinkShell2] = "CrossworldLinkshell2",
        [XivChatType.CrossLinkShell3] = "CrossworldLinkshell3",
        [XivChatType.CrossLinkShell4] = "CrossworldLinkshell4",
        [XivChatType.CrossLinkShell5] = "CrossworldLinkshell5",
        [XivChatType.CrossLinkShell6] = "CrossworldLinkshell6",
        [XivChatType.CrossLinkShell7] = "CrossworldLinkshell7",
        [XivChatType.CrossLinkShell8] = "CrossworldLinkshell8",
    };

    /// <summary>The enum-name fallback keeps a hand-edited LogChannels entry loggable even when
    /// it is not in the table.</summary>
    internal static string Get(XivChatType type)
        => Names.TryGetValue(type, out var name) ? name : type.ToString();
}
