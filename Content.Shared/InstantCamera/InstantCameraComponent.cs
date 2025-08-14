using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.InstantCamera;

/// <summary>
///     Component for a simple instant camera that prints photographs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InstantCameraComponent : Component
{
    /// <summary>
    /// Prototype to spawn for the produced photo.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string PhotoPrototype = "Photo";

    /// <summary>
    /// Sound played when taking a photo.
    /// </summary>
    [DataField]
    public SoundSpecifier SnapSound = new SoundPathSpecifier("/Audio/Items/snap.ogg");
}

