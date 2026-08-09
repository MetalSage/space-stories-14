using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.DoAfter;

public sealed partial class SharedDoAfterTargetSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DoAfterTargetEvent>(OnDoAfterTargetEvent);
        SubscribeLocalEvent<MetaDataComponent, EntityTargetActionDoAfterEvent>(OnEntityTargetActionDoAfterEvent);
        SubscribeLocalEvent<DoAfterUserEvent>(OnDoAfterUserEvent);
        SubscribeLocalEvent<MetaDataComponent, InstantActionDoAfterEvent>(OnInstantActionDoAfterEvent);
    }

    private void OnInstantActionDoAfterEvent(EntityUid uid, MetaDataComponent component, InstantActionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || args.Event == null)
            return;

        args.Event.Handled = false;
        args.Event.Performer = args.User;

        RaiseLocalEvent(args.User, (object)args.Event, true);

        args.Handled = true;
    }

    private void OnEntityTargetActionDoAfterEvent(EntityUid uid,
        MetaDataComponent component,
        EntityTargetActionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || args.Event == null)
            return;

        args.Event.Handled = false;
        args.Event.Performer = args.User;
        args.Event.Target = args.Target.Value;

        RaiseLocalEvent(args.User, (object)args.Event, true);

        args.Handled = true;
    }

    private void OnDoAfterTargetEvent(DoAfterTargetEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager,
            args.Performer,
            args.Delay,
            new EntityTargetActionDoAfterEvent { Event = (EntityTargetActionEvent)args.Event },
            args.Target,
            args.Target)
        {
            NeedHand = args.NeedHand,
            Hidden = args.Hidden,
            AttemptFrequency = args.AttemptFrequency,
            Broadcast = args.Broadcast,
            BreakOnHandChange = args.BreakOnHandChange,
            BreakOnMove = args.BreakOnMove,
            BreakOnWeightlessMove = args.BreakOnWeightlessMove,
            BreakOnDamage = args.BreakOnDamage,
            DamageThreshold = args.DamageThreshold,
            BlockDuplicate = args.BlockDuplicate,
            CancelDuplicate = args.CancelDuplicate,
            DistanceThreshold = args.DistanceThreshold,
            DuplicateCondition = args.DuplicateCondition,
            MovementThreshold = args.MovementThreshold,
            RequireCanInteract = args.RequireCanInteract,
            EventTarget = args.Target,
        };

        if (_doAfterSystem.TryStartDoAfter(doAfterEventArgs))
            args.Handled = true;
    }

    private void OnDoAfterUserEvent(DoAfterUserEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager,
            args.Performer,
            args.Delay,
            new InstantActionDoAfterEvent { Event = (InstantActionEvent)args.Event },
            args.Performer,
            args.Performer)
        {
            NeedHand = args.NeedHand,
            Hidden = args.Hidden,
            AttemptFrequency = args.AttemptFrequency,
            Broadcast = args.Broadcast,
            BreakOnHandChange = args.BreakOnHandChange,
            BreakOnMove = args.BreakOnMove,
            BreakOnWeightlessMove = args.BreakOnWeightlessMove,
            BreakOnDamage = args.BreakOnDamage,
            DamageThreshold = args.DamageThreshold,
            BlockDuplicate = args.BlockDuplicate,
            CancelDuplicate = args.CancelDuplicate,
            DistanceThreshold = args.DistanceThreshold,
            DuplicateCondition = args.DuplicateCondition,
            MovementThreshold = args.MovementThreshold,
            RequireCanInteract = args.RequireCanInteract,
            EventTarget = args.Performer,
        };

        if (_doAfterSystem.TryStartDoAfter(doAfterEventArgs))
            args.Handled = true;
    }
}

[Serializable, NetSerializable] 
public sealed partial class InstantActionDoAfterEvent : SimpleDoAfterEvent
{
    [DataField("event"), NonSerialized]
    public InstantActionEvent? Event;
}

[Serializable, NetSerializable] 
public sealed partial class EntityTargetActionDoAfterEvent : SimpleDoAfterEvent
{
    [DataField("event"), NonSerialized]
    public EntityTargetActionEvent? Event;
}

public sealed partial class DoAfterTargetEvent : EntityTargetActionEvent
{
    [DataField("attemptEventFrequency")]
    public AttemptFrequency AttemptFrequency;

    [DataField("blockDuplicate")]
    public bool BlockDuplicate = true;

    [DataField("breakOnDamage")]
    public bool BreakOnDamage;

    [DataField("breakOnHandChange")]
    public bool BreakOnHandChange = true;

    [DataField("breakOnMove")]
    public bool BreakOnMove;

    [DataField("breakOnWeightlessMove")]
    public bool BreakOnWeightlessMove;

    [DataField("broadcast")]
    public bool Broadcast;

    [DataField("cancelDuplicate")]
    public bool CancelDuplicate = true;

    [DataField("damageThreshold")]
    public FixedPoint2 DamageThreshold = 1;

    [DataField("delay", required: true)]
    public float Delay;

    [DataField("distanceThreshold")]
    public float? DistanceThreshold;

    [DataField("duplicateCondition")]
    public DuplicateConditions DuplicateCondition = DuplicateConditions.All;

    [DataField("event", required: true)]
    public EntityTargetActionEvent Event = default!;

    [DataField("hidden")]
    public bool Hidden;

    [DataField("movementThreshold")]
    public float MovementThreshold = 0.1f;

    [DataField("needHand")]
    public bool NeedHand;

    [DataField("requireCanInteract")]
    public bool RequireCanInteract = true;
}

public sealed partial class DoAfterUserEvent : InstantActionEvent
{
    [DataField("attemptEventFrequency")]
    public AttemptFrequency AttemptFrequency;

    [DataField("blockDuplicate")]
    public bool BlockDuplicate = true;

    [DataField("breakOnDamage")]
    public bool BreakOnDamage;

    [DataField("breakOnHandChange")]
    public bool BreakOnHandChange = true;

    [DataField("breakOnMove")]
    public bool BreakOnMove;

    [DataField("breakOnWeightlessMove")]
    public bool BreakOnWeightlessMove;

    [DataField("broadcast")]
    public bool Broadcast;

    [DataField("cancelDuplicate")]
    public bool CancelDuplicate = true;

    [DataField("damageThreshold")]
    public FixedPoint2 DamageThreshold = 1;

    [DataField("delay", required: true)]
    public float Delay;

    [DataField("distanceThreshold")]
    public float? DistanceThreshold;

    [DataField("duplicateCondition")]
    public DuplicateConditions DuplicateCondition = DuplicateConditions.All;

    [DataField("event", required: true)]
    public InstantActionEvent Event = default!;

    [DataField("hidden")]
    public bool Hidden;

    [DataField("movementThreshold")]
    public float MovementThreshold = 0.1f;

    [DataField("needHand")]
    public bool NeedHand;

    [DataField("requireCanInteract")]
    public bool RequireCanInteract = true;
}
