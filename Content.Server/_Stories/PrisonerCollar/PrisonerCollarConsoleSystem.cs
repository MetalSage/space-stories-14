using Content.Server.Station.Systems;
using Content.Server.SurveillanceCamera;
using Content.Shared._Stories.PrisonerCollar;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server._Stories.PrisonerCollar;

public sealed partial class PrisonerCollarConsoleSystem : EntitySystem
{
    private const float UpdateInterval = 2.0f;

    private readonly Dictionary<ICommonSession, (EntityUid CollarUid, EntityUid TargetUid)>
        _activeCameraViewers = new();

    [Dependency] private PrisonerCollarSystem _collarSystem = default!;
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [Dependency] private InventorySystem _inventorySystem = default!;
    [Dependency] private EntityLookupSystem _lookupSystem = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private StationSystem _stationSystem = default!;
    [Dependency] private SurveillanceCameraSystem _surveillanceCameraSystem = default!;
    [Dependency] private TransformSystem _transformSystem = default!;
    [Dependency] private UserInterfaceSystem _uiSystem = default!;

    private float _updateTimer;
    [Dependency] private SharedViewSubscriberSystem _viewSubscriberSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<PrisonerCollarConsoleComponent>(PrisonerCollarConsoleUiKey.Key,
            subs =>
            {
                subs.Event<BoundUIOpenedEvent>(OnUIOpened);
                subs.Event<BoundUIClosedEvent>(OnUIClosed);
                subs.Event<PrisonerCollarConsoleDetonateMessage>(OnDetonateMessage);
                subs.Event<PrisonerCollarConsoleShockMessage>(OnShockMessage);
                subs.Event<PrisonerCollarConsoleUnlockMessage>(OnUnlockMessage);
                subs.Event<PrisonerCollarConsoleUnlinkMessage>(OnUnlinkMessage);
                subs.Event<PrisonerCollarConsoleScanPairMessage>(OnScanPairMessage);
                subs.Event<PrisonerCollarConsoleSelectCameraMessage>(OnSelectCameraMessage);
            });

        SubscribeLocalEvent<PrisonerCollarConsoleComponent, InteractUsingEvent>(OnInteractUsing);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;
        if (_updateTimer < UpdateInterval)
            return;

        _updateTimer = 0f;

        var query = EntityQueryEnumerator<PrisonerCollarConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out var console, out var ui))
        {
            if (_uiSystem.IsUiOpen(uid, PrisonerCollarConsoleUiKey.Key))
                UpdateUserInterface(uid, console);
        }
    }

    private void OnUIOpened(EntityUid uid, PrisonerCollarConsoleComponent console, BoundUIOpenedEvent args)
    {
        UpdateUserInterface(uid, console);

        if (args.Actor is { Valid: true } actor && TryComp<ActorComponent>(actor, out var actorComp))
            UpdatePlayerCameraSubscription(actorComp.PlayerSession, console, null, uid);
    }

    private void OnUIClosed(EntityUid uid, PrisonerCollarConsoleComponent console, BoundUIClosedEvent args)
    {
        if (args.Actor is { Valid: true } actor && TryComp<ActorComponent>(actor, out var actorComp))
            ClearPlayerCameraSubscription(actorComp.PlayerSession, uid);
    }

    private void OnSelectCameraMessage(EntityUid uid,
        PrisonerCollarConsoleComponent console,
        PrisonerCollarConsoleSelectCameraMessage msg)
    {
        if (msg.Actor is { Valid: true } actor && TryComp<ActorComponent>(actor, out var actorComp))
            UpdatePlayerCameraSubscription(actorComp.PlayerSession, console, msg.Collar, uid);
    }

    private void UpdatePlayerCameraSubscription(ICommonSession session,
        PrisonerCollarConsoleComponent console,
        NetEntity? selectedNetCollar,
        EntityUid consoleUid)
    {
        ClearPlayerCameraSubscription(session, consoleUid);

        if (!selectedNetCollar.HasValue || !console.LinkedCollars.Contains(selectedNetCollar.Value))
        {
            if (console.LinkedCollars.Count == 0)
                return;
            selectedNetCollar = console.LinkedCollars[0];
        }

        var collarUid = GetEntity(selectedNetCollar.Value);
        if (!Exists(collarUid))
            return;

        var parentUid = Transform(collarUid).ParentUid;
        var isWorn = parentUid.IsValid()
                     && HasComp<MobStateComponent>(parentUid)
                     && _inventorySystem.TryGetContainingSlot(collarUid, out var slotContainer)
                     && slotContainer.Name == "neck";

        var targetUid = isWorn ? parentUid : collarUid;

        _viewSubscriberSystem.AddViewSubscriber(targetUid, session);

        if (session.AttachedEntity is { Valid: true } playerEntity)
            _surveillanceCameraSystem.AddActiveViewer(collarUid, playerEntity, consoleUid);

        _activeCameraViewers[session] = (collarUid, targetUid);
    }

    private void ClearPlayerCameraSubscription(ICommonSession session, EntityUid consoleUid = default)
    {
        if (_activeCameraViewers.TryGetValue(session, out var pair))
        {
            if (Exists(pair.TargetUid))
                _viewSubscriberSystem.RemoveViewSubscriber(pair.TargetUid, session);

            if (Exists(pair.CollarUid) && session.AttachedEntity is { Valid: true } playerEntity)
            {
                _surveillanceCameraSystem.RemoveActiveViewer(pair.CollarUid,
                    playerEntity,
                    consoleUid.IsValid() ? consoleUid : null);
            }

            _activeCameraViewers.Remove(session);
        }
    }

    private void OnDetonateMessage(EntityUid uid,
        PrisonerCollarConsoleComponent console,
        PrisonerCollarConsoleDetonateMessage msg)
    {
        var collarUid = GetEntity(msg.Collar);
        if (!console.LinkedCollars.Contains(msg.Collar) || !Exists(collarUid))
            return;

        if (TryComp<PrisonerCollarComponent>(collarUid, out var collarComp))
        {
            _collarSystem.DetonateCollar(collarUid, collarComp);
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-detonate-sent"),
                uid,
                msg.Actor,
                PopupType.MediumCaution);
        }

        UpdateUserInterface(uid, console);
    }

    private void OnShockMessage(EntityUid uid,
        PrisonerCollarConsoleComponent console,
        PrisonerCollarConsoleShockMessage msg)
    {
        var collarUid = GetEntity(msg.Collar);
        if (!console.LinkedCollars.Contains(msg.Collar) || !Exists(collarUid))
            return;

        if (TryComp<PrisonerCollarComponent>(collarUid, out var collarComp))
        {
            if (_inventorySystem.TryGetContainingSlot(collarUid, out var slotContainer) && slotContainer.Name == "neck")
            {
                _collarSystem.ShockWearer(collarUid, collarComp);
                _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-shock-sent"),
                    uid,
                    msg.Actor,
                    PopupType.Medium);
            }
            else
            {
                _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-shock-not-worn"),
                    uid,
                    msg.Actor,
                    PopupType.SmallCaution);
            }
        }

        UpdateUserInterface(uid, console);
    }

    private void OnUnlockMessage(EntityUid uid,
        PrisonerCollarConsoleComponent console,
        PrisonerCollarConsoleUnlockMessage msg)
    {
        var collarUid = GetEntity(msg.Collar);
        if (!console.LinkedCollars.Contains(msg.Collar) || !Exists(collarUid))
            return;

        if (TryComp<PrisonerCollarComponent>(collarUid, out var collarComp))
        {
            _collarSystem.DisarmCollar(collarUid, collarComp, msg.Actor);
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-unlock-sent"),
                uid,
                msg.Actor,
                PopupType.Medium);
        }

        UpdateUserInterface(uid, console);
    }

    private void OnUnlinkMessage(EntityUid uid,
        PrisonerCollarConsoleComponent console,
        PrisonerCollarConsoleUnlinkMessage msg)
    {
        if (console.LinkedCollars.Remove(msg.Collar))
        {
            Dirty(uid, console);
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-unlinked"), uid, msg.Actor);
        }

        UpdateUserInterface(uid, console);
    }

    private void OnScanPairMessage(EntityUid uid,
        PrisonerCollarConsoleComponent console,
        PrisonerCollarConsoleScanPairMessage msg)
    {
        ScanAndPairCollars(uid, console, msg.Actor);
    }

    private void OnInteractUsing(EntityUid uid, PrisonerCollarConsoleComponent console, InteractUsingEvent args)
    {
        if (TryComp<PrisonerCollarComponent>(args.Used, out var collarComp))
        {
            var netCollar = GetNetEntity(args.Used);
            if (!console.LinkedCollars.Contains(netCollar))
            {
                console.LinkedCollars.Add(netCollar);
                collarComp.LinkedConsole = GetNetEntity(uid);
                Dirty(uid, console);
                Dirty(args.Used, collarComp);
                _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-paired"),
                    uid,
                    args.User,
                    PopupType.Medium);
                args.Handled = true;
                UpdateUserInterface(uid, console);
            }
        }
    }

    private void ScanAndPairCollars(EntityUid consoleUid, PrisonerCollarConsoleComponent console, EntityUid user)
    {
        var consolePos = Transform(consoleUid).Coordinates;
        var addedCount = 0;

        foreach (var nearby in _lookupSystem.GetEntitiesInRange(consolePos, 15.0f))
        {
            if (TryComp<PrisonerCollarComponent>(nearby, out var collarComp))
            {
                var netEntity = GetNetEntity(nearby);
                if (!console.LinkedCollars.Contains(netEntity))
                {
                    console.LinkedCollars.Add(netEntity);
                    collarComp.LinkedConsole = GetNetEntity(consoleUid);
                    Dirty(nearby, collarComp);
                    addedCount++;
                }
            }
        }

        Dirty(consoleUid, console);

        if (addedCount > 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-paired-count", ("count", addedCount)),
                consoleUid,
                user,
                PopupType.Medium);
        }
        else
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-none-nearby"), consoleUid, user);

        UpdateUserInterface(consoleUid, console);
    }

    private void UpdateUserInterface(EntityUid uid, PrisonerCollarConsoleComponent console)
    {
        var entries = new List<PrisonerCollarStatusEntry>();
        var toRemove = new List<NetEntity>();

        foreach (var netCollar in console.LinkedCollars)
        {
            var collarUid = GetEntity(netCollar);
            if (!Exists(collarUid) || Deleted(collarUid))
            {
                toRemove.Add(netCollar);
                continue;
            }

            if (!TryComp<PrisonerCollarComponent>(collarUid, out var collarComp))
                continue;

            var parentUid = Transform(collarUid).ParentUid;
            var isWorn = parentUid.IsValid()
                         && HasComp<MobStateComponent>(parentUid)
                         && _inventorySystem.TryGetContainingSlot(collarUid, out var slotContainer)
                         && slotContainer.Name == "neck";

            var wearerName = "N/A";
            var mobState = Loc.GetString("prisoner-collar-mob-unknown");
            var totalDamage = 0;
            var maxDamage = 100;
            NetCoordinates? coords = null;
            var locName = "Станция";

            var targetEntity = isWorn ? parentUid : collarUid;
            var stationUid = _stationSystem.GetOwningStation(targetEntity);
            if (stationUid.HasValue && Exists(stationUid.Value))
            {
                var stationName = MetaData(stationUid.Value).EntityName;
                if (!string.IsNullOrWhiteSpace(stationName))
                    locName = stationName;
            }
            else
            {
                var mapUid = Transform(targetEntity).MapUid;
                if (mapUid.HasValue && Exists(mapUid.Value))
                {
                    var mapName = MetaData(mapUid.Value).EntityName;
                    if (!string.IsNullOrWhiteSpace(mapName))
                        locName = mapName;
                }
            }

            if (isWorn)
            {
                wearerName = MetaData(parentUid).EntityName;

                if (_mobStateSystem.IsAlive(parentUid))
                    mobState = Loc.GetString("prisoner-collar-mob-alive");
                else if (_mobStateSystem.IsCritical(parentUid))
                    mobState = Loc.GetString("prisoner-collar-mob-crit");
                else if (_mobStateSystem.IsDead(parentUid))
                    mobState = Loc.GetString("prisoner-collar-mob-dead");

                if (TryComp<DamageableComponent>(parentUid, out var damageable))
                    totalDamage = _damageableSystem.GetTotalDamage((parentUid, damageable)).Int();

                coords = GetNetCoordinates(Transform(parentUid).Coordinates);
            }
            else
                coords = GetNetCoordinates(Transform(collarUid).Coordinates);

            entries.Add(new PrisonerCollarStatusEntry(
                netCollar,
                isWorn ? GetNetEntity(parentUid) : null,
                wearerName,
                mobState,
                totalDamage,
                maxDamage,
                coords,
                locName,
                collarComp.State,
                isWorn
            ));
        }

        foreach (var deadRef in toRemove)
        {
            console.LinkedCollars.Remove(deadRef);
        }

        _uiSystem.SetUiState(uid,
            PrisonerCollarConsoleUiKey.Key,
            new PrisonerCollarConsoleBoundUserInterfaceState(entries));
    }
}
