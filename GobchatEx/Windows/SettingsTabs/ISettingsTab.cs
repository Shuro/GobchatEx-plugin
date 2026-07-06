using Dalamud.Interface;

namespace GobchatEx.Windows.SettingsTabs;

/// <summary>
/// One tab in the settings window. Tabs draw against the live configuration
/// and never call Save — SettingsWindow detects edits on a debounced tick
/// and commits them (persist + apply) itself.
/// </summary>
internal interface ISettingsTab
{
    string Name { get; }
    FontAwesomeIcon Icon { get; }
    void Draw();
}

/// <summary>
/// A settings tab with a top-level enable/disable switch shown on its nav-rail row.
/// The switch reads/writes the tab's own live config field directly.
/// </summary>
internal interface IToggleableTab : ISettingsTab
{
    bool Enabled { get; set; }
}
