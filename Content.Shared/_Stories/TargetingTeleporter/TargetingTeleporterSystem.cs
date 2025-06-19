using System.Linq;
using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.DoAfter;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Station;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.TargetingTeleporter;

public abstract partial class SharedTargetingTeleporterSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doafter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    public override void Initialize()
    {
        base.Initialize();
        InitializeUser();
        SubscribeLocalEvent<TargetingTeleporterComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<TargetingTeleporterComponent> entity, ref ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        if (entity.Comp.EyeEntity != null)
        {
            var message = Loc.GetString("machine-already-in-use", ("machine", entity));
            _popup.PopupEntity(message, entity, args.User);
            return;
        }

        if (entity.Comp.GridUid is not { } gridUid)
            return;

        if (SetupEye(entity, new EntityCoordinates(gridUid, Vector2.Zero), args.User))
            AttachEye(entity, args.User);

        if (entity.Comp.EyeEntity != null)
            Dirty<TransformComponent>((entity.Comp.EyeEntity.Value, Transform(entity.Comp.EyeEntity.Value)));

        args.Handled = true;
    }

    private void SpawnExitPortal(Entity<TargetingTeleporterComponent> enterPortal, EntityCoordinates coords)
    {
        if (_net.IsClient)
            return;

        if (TryComp<PortalComponent>(enterPortal, out var portal1))
            portal1.CanTeleportToOtherMaps = true;

        var exitPortal = Spawn(enterPortal.Comp.ExitPortalPrototype, coords);

        if (TryComp<PortalComponent>(exitPortal, out var portal2))
            portal2.CanTeleportToOtherMaps = true;

        _link.TryLink(enterPortal, exitPortal, true);

        if (!enterPortal.Comp.Wayback)
            RemComp<PortalComponent>(exitPortal);

        _audio.PlayPvs(enterPortal.Comp.NewPortalSound, enterPortal);
        _audio.PlayPvs(enterPortal.Comp.NewPortalSound, exitPortal);
    }

}

public sealed partial class TargettingTeleporterSetExitPortalEvent : InstantActionEvent
{
}

public sealed partial class TargettingTeleporterExitEvent : InstantActionEvent
{
}
