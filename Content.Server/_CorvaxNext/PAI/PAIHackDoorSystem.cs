using Content.Server.Doors.Systems;
using Content.Shared._CorvaxNext.PAI;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.PAI;

namespace Content.Server._CorvaxNext.PAI;

/// <summary>
///     Handles the PAI door hacking ability.
/// </summary>
public sealed class PAIHackDoorSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DoorSystem _door = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PAIComponent, PAIHackDoorActionEvent>(OnHackDoor);
        SubscribeLocalEvent<PAIComponent, PAIHackDoorDoAfterEvent>(OnHackDoorDoAfter);
    }

    private void OnHackDoor(Entity<PAIComponent> ent, ref PAIHackDoorActionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<DoorComponent>(args.Target))
            return;

        var doArgs = new DoAfterArgs(EntityManager, ent.Owner, args.Delay, new PAIHackDoorDoAfterEvent(), ent.Owner, target: args.Target)
        {
            NeedHand = false,
            BreakOnMove = false,
            BreakOnDamage = true,
            DistanceThreshold = 2f
        };

        _doAfter.TryStartDoAfter(doArgs);
        args.Handled = true;
    }

    private void OnHackDoorDoAfter(Entity<PAIComponent> ent, ref PAIHackDoorDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        if (TryComp<DoorBoltComponent>(target, out var bolt))
            _door.SetBoltsDown((target, bolt), false, ent.Owner);

        if (TryComp<DoorComponent>(target, out var door))
            _door.StartOpening(target, door, ent.Owner);

        args.Handled = true;
    }
}
