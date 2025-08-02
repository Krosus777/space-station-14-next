using Content.Shared.Actions;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.Shared.Corvax.Ipc;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IpcComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField]
    public EntProtoId DrainBatteryAction = "ActionDrainBattery";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    public EntProtoId ChangeMonitorAction = "ActionChangeIpcMonitor";

    [DataField]
    public EntityUid? MonitorActionEntity;

    /// <summary>
    /// Mapping of monitor ids to humanoid marking ids.
    /// </summary>
    [DataField]
    public Dictionary<string, string> Monitors = new();

    [DataField]
    public string? DefaultMonitor;

    [DataField]
    public string? CurrentMonitor;

    public bool DrainActivated;
}

public sealed partial class ToggleDrainActionEvent : InstantActionEvent
{

}

public sealed partial class ChangeMonitorActionEvent : InstantActionEvent
{

}
