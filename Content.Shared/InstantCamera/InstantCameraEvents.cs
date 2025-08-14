using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.InstantCamera;

[Serializable, NetSerializable]
public sealed partial class InstantCameraRequestPhotoEvent : EntityEventArgs
{
    public NetEntity Photo;

    public InstantCameraRequestPhotoEvent(NetEntity photo)
    {
        Photo = photo;
    }

    private InstantCameraRequestPhotoEvent()
    {
    }
}

[Serializable, NetSerializable]
public sealed partial class InstantCameraPhotoTakenEvent : EntityEventArgs
{
    public NetEntity Photo;
    public string RelativePath;

    public InstantCameraPhotoTakenEvent(NetEntity photo, string relativePath)
    {
        Photo = photo;
        RelativePath = relativePath;
    }

    private InstantCameraPhotoTakenEvent()
    {
        RelativePath = string.Empty;
    }
}

