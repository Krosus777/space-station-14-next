using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Corvax.Ipc;
using Content.Shared.Ninja.Components;
using Content.Shared.Ninja.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell.Components;
using Content.Shared.Damage;
using Content.Server.Emp;
using Content.Shared.Movement.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Sound.Components;
using Content.Shared.UserInterface;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Server.PowerCell;
using Content.Server.Humanoid;
using Robust.Shared.Audio;
using System.Collections.Generic;

namespace Content.Server.Corvax.Ipc;

public sealed class IpcSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedBatteryDrainerSystem _batteryDrainer = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;



    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IpcComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<IpcComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<IpcComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<IpcComponent, ToggleDrainActionEvent>(OnToggleAction);
        SubscribeLocalEvent<IpcComponent, ChangeMonitorActionEvent>(OnChangeMonitorAction);
        SubscribeLocalEvent<IpcComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<IpcComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
        SubscribeLocalEvent<IpcComponent, MobStateChangedEvent>(OnMobStateChanged);

        Subs.BuiEvents<IpcComponent>(IpcMonitorUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnMonitorUiOpened);
            subs.Event<IpcMonitorSelectMessage>(OnMonitorSelected);
        });
    }

    private void OnMapInit(EntityUid uid, IpcComponent component, MapInitEvent args)
    {
        UpdateBatteryAlert((uid, component));
        _action.AddAction(uid, ref component.ActionEntity, component.DrainBatteryAction);
        _action.AddAction(uid, ref component.MonitorActionEntity, component.ChangeMonitorAction);
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);

        if (!string.IsNullOrEmpty(component.DefaultMonitor))
            SetMonitor(uid, component, component.DefaultMonitor);
    }

    private void OnComponentShutdown(EntityUid uid, IpcComponent component, ComponentShutdown args)
    {
        _action.RemoveAction(uid, component.ActionEntity);
        _action.RemoveAction(uid, component.MonitorActionEntity);
    }

    private void OnPowerCellChanged(EntityUid uid, IpcComponent component, PowerCellChangedEvent args)
    {
        if (MetaData(uid).EntityLifeStage >= EntityLifeStage.Terminating)
            return;

        UpdateBatteryAlert((uid, component));

    }

    private void OnToggleAction(EntityUid uid, IpcComponent component, ToggleDrainActionEvent args)
    {
        if (args.Handled)
            return;

        component.DrainActivated = !component.DrainActivated;
        _action.SetToggled(component.ActionEntity, component.DrainActivated);
        args.Handled = true;

        if (component.DrainActivated && _powerCell.TryGetBatteryFromSlot(uid, out var battery, out var _))
        {
            EnsureComp<BatteryDrainerComponent>(uid);
            _batteryDrainer.SetBattery(uid, battery);
        }
        else
            RemComp<BatteryDrainerComponent>(uid);

        var message = component.DrainActivated ? "ipc-component-ready" : "ipc-component-disabled";
        _popup.PopupEntity(Loc.GetString(message), uid, uid);
    }

    private void OnChangeMonitorAction(EntityUid uid, IpcComponent component, ChangeMonitorActionEvent args)
    {
        if (args.Handled)
            return;

        _ui.TryOpenUi(uid, IpcMonitorUiKey.Key, args.Performer);
        args.Handled = true;
    }

    private void OnMonitorUiOpened(Entity<IpcComponent> ent, ref BoundUIOpenedEvent args)
    {
        var monitors = new List<string>(ent.Comp.Monitors.Keys);
        var current = ent.Comp.CurrentMonitor ?? string.Empty;
        _ui.SetUiState(ent.Owner, IpcMonitorUiKey.Key, new IpcMonitorBoundUserInterfaceState(monitors, current));
    }

    private void OnMonitorSelected(Entity<IpcComponent> ent, ref IpcMonitorSelectMessage msg)
    {
        SetMonitor(ent.Owner, ent.Comp, msg.Monitor);
    }

    private void SetMonitor(EntityUid uid, IpcComponent component, string monitor)
    {
        if (!component.Monitors.TryGetValue(monitor, out var marking))
            return;

        component.CurrentMonitor = monitor;
        _humanoid.SetMarkingId(uid, MarkingCategories.Head, 0, marking);
    }
    private void UpdateBatteryAlert(Entity<IpcComponent> ent, PowerCellSlotComponent? slot = null)
    {


        if (!_powerCell.TryGetBatteryFromSlot(ent, out var battery, slot) || battery.CurrentCharge / battery.MaxCharge < 0.01f)
        {
            _alerts.ClearAlert(ent, ent.Comp.BatteryAlert);
            _alerts.ShowAlert(ent, ent.Comp.NoBatteryAlert);

            _movementSpeedModifier.RefreshMovementSpeedModifiers(ent.Owner);
            return;
        }

        var chargePercent = (short) MathF.Round(battery.CurrentCharge / battery.MaxCharge * 10f);

        if (chargePercent == 0 && _powerCell.HasDrawCharge(ent, cell: slot))
            chargePercent = 1;


        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent.Owner);

        _alerts.ClearAlert(ent, ent.Comp.NoBatteryAlert);
        _alerts.ShowAlert(ent, ent.Comp.BatteryAlert, chargePercent);
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, IpcComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!_powerCell.TryGetBatteryFromSlot(uid, out var battery) || battery.CurrentCharge / battery.MaxCharge < 0.01f)
        {
            args.ModifySpeed(0.2f);
        }
    }

    private void OnEmpPulse(EntityUid uid, IpcComponent component, ref EmpPulseEvent args)
    {
        args.Affected = true;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Shock", 30);
        _damageable.TryChangeDamage(uid, damage);

    }

    private void OnMobStateChanged(EntityUid uid, IpcComponent component, ref MobStateChangedEvent args)
    {
        if (_mobState.IsCritical(uid))
        {
            var sound = EnsureComp<SpamEmitSoundComponent>(uid);
            sound.Sound = new SoundPathSpecifier("/Audio/Machines/buzz-two.ogg");
            sound.MinInterval = TimeSpan.FromSeconds(15);
            sound.MaxInterval = TimeSpan.FromSeconds(30);
            sound.PopUp = Loc.GetString("sleep-ipc");
        }
        else
        {
            RemComp<SpamEmitSoundComponent>(uid);
        }

        if (args.NewMobState == MobState.Dead)
            _humanoid.RemoveMarking(uid, MarkingCategories.Head, 0);
        else if (component.CurrentMonitor != null && component.Monitors.TryGetValue(component.CurrentMonitor, out var marking))
            _humanoid.SetMarkingId(uid, MarkingCategories.Head, 0, marking);
    }
}
