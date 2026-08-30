using Content.Server.Electrocution;
using Content.Server.Explosion.EntitySystems;
using Content.Shared._Stories.PrisonerCollar;
using Content.Shared.Access.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Emp;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Tools;
using Content.Shared.Tools.Systems;
using Content.Shared.Traits.Assorted;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Stories.PrisonerCollar;

public sealed partial class PrisonerCollarSystem : EntitySystem
{
    public static readonly ProtoId<ToolQualityPrototype> SawingQuality = "Sawing";

    private static readonly TimeSpan ShockCooldown = TimeSpan.FromSeconds(5.0);
    private readonly Dictionary<EntityUid, TimeSpan> _lastShockTimes = new();
    private readonly HashSet<EntityUid> _shockingEntities = new();
    [Dependency] private AccessReaderSystem _accessReader = default!;
    [Dependency] private SharedAudioSystem _audioSystem = default!;
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private ElectrocutionSystem _electrocutionSystem = default!;
    [Dependency] private ExplosionSystem _explosionSystem = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private InventorySystem _inventorySystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _robustRandom = default!;
    [Dependency] private SharedToolSystem _toolSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrisonerCollarComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<PrisonerCollarComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<PrisonerCollarComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
        SubscribeLocalEvent<PrisonerCollarComponent, BeingUnequippedAttemptEvent>(OnUnequipAttempt);
        SubscribeLocalEvent<PrisonerCollarComponent, GotEmaggedEvent>(OnGotEmagged);
        SubscribeLocalEvent<PrisonerCollarComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<PrisonerCollarComponent, EmpDisabledRemovedEvent>(OnEmpDisabledRemoved);
        SubscribeLocalEvent<PrisonerCollarComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PrisonerCollarComponent, GetVerbsEvent<EquipmentVerb>>(OnGetEquipmentVerbs);
        SubscribeLocalEvent<PrisonerCollarComponent, GetVerbsEvent<Verb>>(OnGetStandardVerbs);
        SubscribeLocalEvent<PrisonerCollarComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<PrisonerCollarComponent, PrisonerCollarDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PrisonerCollarComponent, PrisonerCollarRemovalDoAfterEvent>(OnRemovalDoAfter);
    }

    private void OnGotEquipped(EntityUid uid, PrisonerCollarComponent collar, GotEquippedEvent args)
    {
        if (args.Slot == "neck")
        {
            collar.State = PrisonerCollarState.Active;
            Dirty(uid, collar);
            EnsureComp<UnremoveableComponent>(uid);
            _audioSystem.PlayPvs("/Audio/Machines/door_lock_on.ogg", uid);
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-locked"),
                uid,
                args.EquipTarget,
                PopupType.Medium);
        }
    }

    private void OnGotUnequipped(EntityUid uid, PrisonerCollarComponent collar, GotUnequippedEvent args)
    {
        RemComp<UnremoveableComponent>(uid);
    }

    private bool TryShockVictim(EntityUid collarUid, EntityUid victim)
    {
        var curTime = _gameTiming.CurTime;
        if (_lastShockTimes.TryGetValue(victim, out var lastTime) && curTime - lastTime < ShockCooldown)
        {
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-shock-cooldown"), collarUid, victim);
            return false;
        }

        _lastShockTimes[victim] = curTime;
        _electrocutionSystem.TryDoElectrocution(victim,
            collarUid,
            15,
            TimeSpan.FromSeconds(2.5),
            true,
            ignoreInsulation: true);
        _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-shock-attempt"),
            collarUid,
            victim,
            PopupType.SmallCaution);
        _audioSystem.PlayPvs("/Audio/Effects/sparks4.ogg", collarUid);
        return true;
    }

    private void OnRemoveAttempt(EntityUid uid,
        PrisonerCollarComponent collar,
        ContainerGettingRemovedAttemptEvent args)
    {
        if (collar.State != PrisonerCollarState.Active)
            return;

        if (!_inventorySystem.TryGetContainingSlot(uid, out var slotContainer) || slotContainer.Name != "neck")
            return;

        args.Cancel();
    }

    private void OnUnequipAttempt(EntityUid uid, PrisonerCollarComponent collar, BeingUnequippedAttemptEvent args)
    {
        if (collar.State != PrisonerCollarState.Active)
            return;

        if (!_inventorySystem.TryGetContainingSlot(uid, out var slotContainer) || slotContainer.Name != "neck")
            return;

        args.Cancel();

        var user = args.User;
        StartRemovalDoAfter(uid, collar, user);
    }

    private void StartRemovalDoAfter(EntityUid collarUid, PrisonerCollarComponent collar, EntityUid user)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            3.0f,
            new PrisonerCollarRemovalDoAfterEvent(),
            collarUid,
            collarUid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-removal-attempt"), collarUid, user);
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnRemovalDoAfter(EntityUid uid, PrisonerCollarComponent collar, PrisonerCollarRemovalDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (collar.State == PrisonerCollarState.Active && !_accessReader.IsAllowed(args.User, uid))
            TryShockVictim(uid, args.User);
        else
            UnlockAndRemoveCollar(uid, collar, args.User);
    }

    private void OnGotEmagged(EntityUid uid, PrisonerCollarComponent collar, ref GotEmaggedEvent args)
    {
        collar.State = PrisonerCollarState.Disarmed;
        Dirty(uid, collar);
        RemComp<UnremoveableComponent>(uid);

        _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-unlocked"), uid, args.UserUid, PopupType.Medium);
        _audioSystem.PlayPvs("/Audio/Machines/door_lock_off.ogg", uid);
        args.Handled = true;
    }

    private void OnEmpPulse(EntityUid uid, PrisonerCollarComponent collar, ref EmpPulseEvent args)
    {
        if (collar.State == PrisonerCollarState.Active)
        {
            collar.State = PrisonerCollarState.EmpDisabled;
            Dirty(uid, collar);
        }

        args.Disabled = true;
        args.Affected = true;
    }

    private void OnEmpDisabledRemoved(EntityUid uid, PrisonerCollarComponent collar, ref EmpDisabledRemovedEvent args)
    {
        if (collar.State == PrisonerCollarState.EmpDisabled)
        {
            collar.State = PrisonerCollarState.Active;
            Dirty(uid, collar);

            if (Transform(uid).ParentUid.IsValid())
                EnsureComp<UnremoveableComponent>(uid);
        }
    }

    private void OnGetStandardVerbs(EntityUid uid, PrisonerCollarComponent collar, GetVerbsEvent<Verb> args)
    {
        args.Verbs.RemoveWhere(v => v.Text == "Настроить" || v.Text == "Разделать" || v.Category?.Text == "Разделать");
    }

    private void OnGetVerbs(EntityUid uid, PrisonerCollarComponent collar, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!_inventorySystem.TryGetContainingSlot(uid, out var slotContainer) || slotContainer.Name != "neck")
            return;

        if (collar.State == PrisonerCollarState.Disarmed)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("prisoner-collar-verb-remove"),
                Act = () =>
                {
                    UnlockAndRemoveCollar(uid, collar, args.User);
                },
            });
            return;
        }

        if (_accessReader.IsAllowed(args.User, uid))
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("prisoner-collar-verb-unlock"),
                IconEntity = GetNetEntity(uid),
                Act = () =>
                {
                    UnlockAndRemoveCollar(uid, collar, args.User);
                },
            });
        }
        else if (collar.State == PrisonerCollarState.Active)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("prisoner-collar-verb-remove-dangerous"),
                Act = () =>
                {
                    StartRemovalDoAfter(uid, collar, args.User);
                },
            });
        }
    }

    private void OnGetEquipmentVerbs(EntityUid uid, PrisonerCollarComponent collar, GetVerbsEvent<EquipmentVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!_inventorySystem.TryGetContainingSlot(uid, out var slotContainer) || slotContainer.Name != "neck")
            return;

        if (_accessReader.IsAllowed(args.User, uid) || collar.State == PrisonerCollarState.Disarmed)
        {
            args.Verbs.Add(new EquipmentVerb
            {
                Text = Loc.GetString("prisoner-collar-verb-strip"),
                Act = () =>
                {
                    UnlockAndRemoveCollar(uid, collar, args.User);
                },
            });
        }
    }

    private void OnInteractUsing(EntityUid uid, PrisonerCollarComponent collar, InteractUsingEvent args)
    {
        if (!_toolSystem.HasQuality(args.Used, SawingQuality))
            return;

        args.Handled = true;

        if (collar.State == PrisonerCollarState.Active)
        {
            TryShockVictim(uid, args.User);
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-saw-active"),
                uid,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        _toolSystem.UseTool(
            args.Used,
            args.User,
            uid,
            12.0f,
            SawingQuality,
            new PrisonerCollarDoAfterEvent());
    }

    private void OnDoAfter(EntityUid uid, PrisonerCollarComponent collar, PrisonerCollarDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var targetWearer = Transform(uid).ParentUid;
        var victim = targetWearer.IsValid() ? targetWearer : args.User;

        if (_robustRandom.Prob(collar.FailChance))
        {
            _damageableSystem.TryChangeDamage(victim, collar.FailDamage, origin: args.User);
            _audioSystem.PlayPvs("/Audio/Weapons/bladeslice.ogg", uid);
            _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-saw-fail"),
                uid,
                args.User,
                PopupType.SmallCaution);
            return;
        }

        DisarmCollar(uid, collar, args.User);
        UnlockAndRemoveCollar(uid, collar, args.User);
        _popupSystem.PopupEntity(Loc.GetString("prisoner-collar-popup-saw-success"), uid, args.User, PopupType.Medium);
    }

    public void DisarmCollar(EntityUid uid, PrisonerCollarComponent collar, EntityUid user)
    {
        collar.State = PrisonerCollarState.Disarmed;
        Dirty(uid, collar);
        RemComp<UnremoveableComponent>(uid);
        _audioSystem.PlayPvs("/Audio/Machines/door_lock_off.ogg", uid);
    }

    public void UnlockAndRemoveCollar(EntityUid uid, PrisonerCollarComponent collar, EntityUid user)
    {
        DisarmCollar(uid, collar, user);

        var parent = Transform(uid).ParentUid;
        if (parent.IsValid() && _inventorySystem.TryGetContainingSlot(uid, out var slotContainer))
            _inventorySystem.TryUnequip(parent, slotContainer.Name, force: true);
    }

    public void ShockWearer(EntityUid uid, PrisonerCollarComponent collar)
    {
        if (collar.State == PrisonerCollarState.EmpDisabled)
            return;

        if (!_inventorySystem.TryGetContainingSlot(uid, out var slotContainer) || slotContainer.Name != "neck")
            return;

        var parent = Transform(uid).ParentUid;
        if (!parent.IsValid())
            return;

        TryShockVictim(uid, parent);
    }

    public void DetonateCollar(EntityUid uid, PrisonerCollarComponent collar)
    {
        if (collar.State == PrisonerCollarState.EmpDisabled)
            return;

        var parent = Transform(uid).ParentUid;
        if (parent.IsValid())
        {
            EnsureComp<UnrevivableComponent>(parent);

            var fatalDamage = new DamageSpecifier();
            fatalDamage.DamageDict.Add("Blunt", 500);
            fatalDamage.DamageDict.Add("Heat", 500);
            _damageableSystem.TryChangeDamage(parent, fatalDamage, true);
        }

        _explosionSystem.QueueExplosion(uid, ExplosionSystem.DefaultExplosionPrototypeId, 120, 10, 80, maxTileBreak: 4);
        QueueDel(uid);
    }
}
