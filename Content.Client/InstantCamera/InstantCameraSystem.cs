using System;
using System.IO;
using Content.Shared.InstantCamera;
using Robust.Client.Graphics;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Upload;
using Robust.Shared.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client.InstantCamera;

public sealed partial class InstantCameraSystem : EntitySystem
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<InstantCameraRequestPhotoEvent>(OnRequestPhoto);
        SubscribeLocalEvent<PhotoComponent, AfterAutoHandleStateEvent>(OnPhotoState);
    }

    private void OnPhotoState(EntityUid uid, PhotoComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (string.IsNullOrEmpty(comp.TexturePath))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetTexture((uid, sprite), 0, new ResPath(comp.TexturePath));
    }

    private void OnRequestPhoto(InstantCameraRequestPhotoEvent ev)
    {
        _clyde.Screenshot(ScreenshotType.Final, (Image<Rgb24> image) =>
        {
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            var data = stream.ToArray();

            var relative = new ResPath($"photos/{Guid.NewGuid()}.png").ToRelativePath();
            var upload = new NetworkResourceUploadMessage
            {
                Data = data,
                RelativePath = relative
            };
            _net.ClientSendMessage(upload);

            RaiseNetworkEvent(new InstantCameraPhotoTakenEvent(ev.Photo, relative.ToString()));
        });
    }
}

