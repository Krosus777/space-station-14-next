using Content.Shared.Hands.EntitySystems;
using Content.Shared.InstantCamera;
using Content.Shared.Interaction.Events;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.Server.InstantCamera;

/// <summary>
///     Handles taking photos and spawning printed pictures.
/// </summary>
public sealed partial class InstantCameraSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InstantCameraComponent, UseInHandEvent>(OnUseInHand);
        SubscribeNetworkEvent<InstantCameraPhotoTakenEvent>(OnPhotoTaken);
    }

    private void OnUseInHand(EntityUid uid, InstantCameraComponent comp, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var user = args.User;
        var userName = EntityManager.ToPrettyString(user);
        var photo = EntityManager.SpawnEntity(comp.PhotoPrototype, Transform(uid).Coordinates);
        _paper.SetContent(photo, Loc.GetString("instant-camera-photo-content", ("user", userName)));
        _hands.TryPickupAnyHand(user, photo);

        _audio.PlayPredicted(comp.SnapSound, uid, user);
        _popup.PopupClient(Loc.GetString("instant-camera-printed"), uid, user);

        var ev = new InstantCameraRequestPhotoEvent(GetNetEntity(photo));
        RaiseNetworkEvent(ev, user);
    }

    private void OnPhotoTaken(InstantCameraPhotoTakenEvent ev)
    {
        var photo = GetEntity(ev.Photo);
        if (!TryComp<PhotoComponent>(photo, out var comp))
            return;

        comp.TexturePath = $"/Uploaded/{ev.RelativePath}";
        Dirty(photo, comp);
    }
}

