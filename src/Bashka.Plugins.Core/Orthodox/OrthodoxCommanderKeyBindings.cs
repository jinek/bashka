using Bashka.Plugins.Core.Configuration;

// ReSharper disable UnusedMember.Global // used via reflection

namespace Bashka.Plugins.Core.Orthodox;

/// <summary>
///     Defaults are in orthodoxcommander.ini
/// </summary>
[SettingsSection("OrthodoxCommander(keys)")]
public class OrthodoxCommanderKeyBindings : KeyBindings
{
    public string? QuickSearchCommand { get; init; }
    public string? SwitchPanelCommand { get; init; }
    public string? ToggleMarkCommand { get; init; }
    public string? ToggleMarkUpCommand { get; init; }
    public string? MarkAllCommand { get; init; }
    public string? UnmarkAllCommand { get; init; }
    public string? InvertMarksCommand { get; init; }

    public string? F1SlotCommand { get; init; }
    public string? CtrlF1SlotCommand { get; init; }
    public string? AltF1SlotCommand { get; init; }
    public string? ShiftF1SlotCommand { get; init; }

    public string? F2SlotCommand { get; init; }
    public string? CtrlF2SlotCommand { get; init; }
    public string? AltF2SlotCommand { get; init; }
    public string? ShiftF2SlotCommand { get; init; }

    public string? F3SlotCommand { get; init; }
    public string? CtrlF3SlotCommand { get; init; }
    public string? AltF3SlotCommand { get; init; }
    public string? ShiftF3SlotCommand { get; init; }

    public string? F4SlotCommand { get; init; }
    public string? CtrlF4SlotCommand { get; init; }
    public string? AltF4SlotCommand { get; init; }
    public string? ShiftF4SlotCommand { get; init; }

    public string? F5SlotCommand { get; init; }
    public string? CtrlF5SlotCommand { get; init; }
    public string? AltF5SlotCommand { get; init; }
    public string? ShiftF5SlotCommand { get; init; }

    public string? F6SlotCommand { get; init; }
    public string? CtrlF6SlotCommand { get; init; }
    public string? AltF6SlotCommand { get; init; }
    public string? ShiftF6SlotCommand { get; init; }

    public string? F7SlotCommand { get; init; }
    public string? CtrlF7SlotCommand { get; init; }
    public string? AltF7SlotCommand { get; init; }
    public string? ShiftF7SlotCommand { get; init; }

    public string? F8SlotCommand { get; init; }
    public string? CtrlF8SlotCommand { get; init; }
    public string? AltF8SlotCommand { get; init; }
    public string? ShiftF8SlotCommand { get; init; }

    public string? F9SlotCommand { get; init; }
    public string? CtrlF9SlotCommand { get; init; }
    public string? AltF9SlotCommand { get; init; }
    public string? ShiftF9SlotCommand { get; init; }

    public string? F10SlotCommand { get; init; }
    public string? CtrlF10SlotCommand { get; init; }
    public string? AltF10SlotCommand { get; init; }
    public string? ShiftF10SlotCommand { get; init; }

    public string? F11SlotCommand { get; init; }
    public string? CtrlF11SlotCommand { get; init; }
    public string? AltF11SlotCommand { get; init; }
    public string? ShiftF11SlotCommand { get; init; }

    public string? F12SlotCommand { get; init; }
    public string? CtrlF12SlotCommand { get; init; }
    public string? AltF12SlotCommand { get; init; }
    public string? ShiftF12SlotCommand { get; init; }
}