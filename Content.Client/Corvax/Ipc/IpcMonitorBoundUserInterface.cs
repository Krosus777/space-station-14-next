using System;
using Content.Shared.Corvax.Ipc;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Corvax.Ipc;

public sealed class IpcMonitorBoundUserInterface : BoundUserInterface
{
    private IpcMonitorWindow? _window;

    public IpcMonitorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new IpcMonitorWindow();
        _window.OnSelect += id => SendMessage(new IpcMonitorSelectMessage(id));
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not IpcMonitorBoundUserInterfaceState s)
            return;
        _window.UpdateState(s.Monitors, s.Current);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
