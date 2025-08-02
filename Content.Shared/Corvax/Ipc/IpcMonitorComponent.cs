using System.Collections.Generic;
using Robust.Shared.Serialization;

namespace Content.Shared.Corvax.Ipc;

[Serializable, NetSerializable]
public enum IpcMonitorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class IpcMonitorBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly List<string> Monitors;
    public readonly string Current;

    public IpcMonitorBoundUserInterfaceState(List<string> monitors, string current)
    {
        Monitors = monitors;
        Current = current;
    }
}

[Serializable, NetSerializable]
public sealed class IpcMonitorSelectMessage : BoundUserInterfaceMessage
{
    public readonly string Monitor;

    public IpcMonitorSelectMessage(string monitor)
    {
        Monitor = monitor;
    }
}
