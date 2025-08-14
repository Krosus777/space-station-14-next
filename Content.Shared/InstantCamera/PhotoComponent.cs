using Robust.Shared.GameStates;

namespace Content.Shared.InstantCamera;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class PhotoComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? TexturePath;
}

