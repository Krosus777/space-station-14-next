using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxNext.PAI;

/// <summary>
///     Event raised when a PAI attempts to hack a door via action.
///     Delay controls the hacking time before the door opens.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class PAIHackDoorActionEvent : EntityTargetActionEvent
{
    /// <summary>
    ///     Hacking time in seconds.
    /// </summary>
    [DataField("delay")]
    public float Delay = 3f;
}

/// <summary>
///     Raised after the hacking delay completes.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class PAIHackDoorDoAfterEvent : SimpleDoAfterEvent
{
}
