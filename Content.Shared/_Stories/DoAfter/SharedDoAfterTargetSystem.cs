using Content.Shared.DoAfter;
using Content.Shared.Actions;
using Robust.Shared.Serialization;
using Content.Shared.FixedPoint;

namespace Content.Shared._Stories.DoAfter;
public sealed partial class SharedDoAfterTargetSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
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

        RaiseLocalEvent(args.User, (object) args.Event, broadcast: true);

        args.Handled = true;
    }
    private void OnEntityTargetActionDoAfterEvent(EntityUid uid, MetaDataComponent component, EntityTargetActionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null || args.Event == null)
            return;

        args.Event.Handled = false;
        args.Event.Performer = args.User;
        args.Event.Target = args.Target.Value;

        RaiseLocalEvent(args.User, (object) args.Event, broadcast: true);

        args.Handled = true;
    }
    private void OnDoAfterTargetEvent(DoAfterTargetEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, args.Delay, new EntityTargetActionDoAfterEvent() { Event = (EntityTargetActionEvent) args.Event }, args.Target, args.Target)
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

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, args.Delay, new InstantActionDoAfterEvent() { Event = (InstantActionEvent) args.Event }, args.Performer, args.Performer)
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
    [DataField("event")]
    [NonSerialized]
    public InstantActionEvent? Event;
}
[Serializable, NetSerializable]
public sealed partial class EntityTargetActionDoAfterEvent : SimpleDoAfterEvent
{
    [DataField("event")]
    [NonSerialized]
    public EntityTargetActionEvent? Event;
}
public sealed partial class DoAfterTargetEvent : EntityTargetActionEvent
{
    /// <summary>
    ///     How long does the do_after require to complete
    /// </summary>
    [DataField("delay", required: true)]
    public float Delay;

    /// <summary>
    /// Whether the progress bar for this DoAfter should be hidden from other players.
    /// </summary>
    [DataField("hidden")]
    public bool Hidden;

    [DataField("event", required: true)]
    public EntityTargetActionEvent Event = default!;

    [DataField("attemptEventFrequency")]
    public AttemptFrequency AttemptFrequency;

    /// <summary>
    /// Should the DoAfter event broadcast? If this is false, then <see cref="EventTarget"/> should be a valid entity.
    /// </summary>
    [DataField("broadcast")]
    public bool Broadcast;

    // Break the chains
    /// <summary>
    ///     Whether or not this do after requires the user to have hands.
    /// </summary>
    [DataField("needHand")]
    public bool NeedHand;

    /// <summary>
    ///     Whether we need to keep our active hand as is (i.e. can't change hand or change item). This also covers
    ///     requiring the hand to be free (if applicable). This does nothing if <see cref="NeedHand"/> is false.
    /// </summary>
    [DataField("breakOnHandChange")]
    public bool BreakOnHandChange = true;

    /// <summary>
    ///     If do_after stops when the user moves
    /// </summary>
    [DataField("breakOnMove")]
    public bool BreakOnMove;

    /// <summary>
    ///     If this is true then any movement, even when weightless, will break the doafter.
    ///     When there is no gravity, BreakOnUserMove is ignored. If it is false to begin with nothing will change.
    /// </summary>
    [DataField("breakOnWeightlessMove")]
    public bool BreakOnWeightlessMove;

    /// <summary>
    ///     Threshold for user and target movement
    /// </summary>
    [DataField("movementThreshold")]
    public float MovementThreshold = 0.1f;

    /// <summary>
    ///     Threshold for distance user from the used OR target entities.
    /// </summary>
    [DataField("distanceThreshold")]
    public float? DistanceThreshold;

    /// <summary>
    ///     Whether damage will cancel the DoAfter. See also <see cref="DamageThreshold"/>.
    /// </summary>
    [DataField("breakOnDamage")]
    public bool BreakOnDamage;

    /// <summary>
    ///     Threshold for user damage. This damage has to be dealt in a single event, not over time.
    /// </summary>
    [DataField("damageThreshold")]
    public FixedPoint2 DamageThreshold = 1;

    /// <summary>
    ///     If true, this DoAfter will be canceled if the user can no longer interact with the target.
    /// </summary>
    [DataField("requireCanInteract")]
    public bool RequireCanInteract = true;

    [DataField("blockDuplicate")]
    public bool BlockDuplicate = true;

    //TODO: User pref to not cancel on second use on specific doafters
    /// <summary>
    ///     If true, this will cancel any duplicate DoAfters when attempting to add a new DoAfter. See also
    ///     <see cref="DuplicateConditions"/>.
    /// </summary>
    [DataField("cancelDuplicate")]
    public bool CancelDuplicate = true;

    /// <summary>
    ///     These flags determine what DoAfter properties are used to determine whether one DoAfter is a duplicate of
    ///     another.
    /// </summary>
    /// <remarks>
    ///     Note that both DoAfters may have their own conditions, and they will be considered duplicated if either set
    ///     of conditions is satisfied.
    /// </remarks>
    [DataField("duplicateCondition")]
    public DuplicateConditions DuplicateCondition = DuplicateConditions.All;
}

public sealed partial class DoAfterUserEvent : InstantActionEvent
{
    /// <summary>
    ///     How long does the do_after require to complete
    /// </summary>
    [DataField("delay", required: true)]
    public float Delay;

    /// <summary>
    /// Whether the progress bar for this DoAfter should be hidden from other players.
    /// </summary>
    [DataField("hidden")]
    public bool Hidden;

    [DataField("event", required: true)]
    public InstantActionEvent Event = default!;

    [DataField("attemptEventFrequency")]
    public AttemptFrequency AttemptFrequency;

    /// <summary>
    /// Should the DoAfter event broadcast? If this is false, then <see cref="EventTarget"/> should be a valid entity.
    /// </summary>
    [DataField("broadcast")]
    public bool Broadcast;

    // Break the chains
    /// <summary>
    ///     Whether or not this do after requires the user to have hands.
    /// </summary>
    [DataField("needHand")]
    public bool NeedHand;

    /// <summary>
    ///     Whether we need to keep our active hand as is (i.e. can't change hand or change item). This also covers
    ///     requiring the hand to be free (if applicable). This does nothing if <see cref="NeedHand"/> is false.
    /// </summary>
    [DataField("breakOnHandChange")]
    public bool BreakOnHandChange = true;

    /// <summary>
    ///     If do_after stops when the user moves
    /// </summary>
    [DataField("breakOnMove")]
    public bool BreakOnMove;

    /// <summary>
    ///     If this is true then any movement, even when weightless, will break the doafter.
    ///     When there is no gravity, BreakOnUserMove is ignored. If it is false to begin with nothing will change.
    /// </summary>
    [DataField("breakOnWeightlessMove")]
    public bool BreakOnWeightlessMove;

    /// <summary>
    ///     Threshold for user and target movement
    /// </summary>
    [DataField("movementThreshold")]
    public float MovementThreshold = 0.1f;

    /// <summary>
    ///     Threshold for distance user from the used OR target entities.
    /// </summary>
    [DataField("distanceThreshold")]
    public float? DistanceThreshold;

    /// <summary>
    ///     Whether damage will cancel the DoAfter. See also <see cref="DamageThreshold"/>.
    /// </summary>
    [DataField("breakOnDamage")]
    public bool BreakOnDamage;

    /// <summary>
    ///     Threshold for user damage. This damage has to be dealt in a single event, not over time.
    /// </summary>
    [DataField("damageThreshold")]
    public FixedPoint2 DamageThreshold = 1;

    /// <summary>
    ///     If true, this DoAfter will be canceled if the user can no longer interact with the target.
    /// </summary>
    [DataField("requireCanInteract")]
    public bool RequireCanInteract = true;

    [DataField("blockDuplicate")]
    public bool BlockDuplicate = true;

    //TODO: User pref to not cancel on second use on specific doafters
    /// <summary>
    ///     If true, this will cancel any duplicate DoAfters when attempting to add a new DoAfter. See also
    ///     <see cref="DuplicateConditions"/>.
    /// </summary>
    [DataField("cancelDuplicate")]
    public bool CancelDuplicate = true;

    /// <summary>
    ///     These flags determine what DoAfter properties are used to determine whether one DoAfter is a duplicate of
    ///     another.
    /// </summary>
    /// <remarks>
    ///     Note that both DoAfters may have their own conditions, and they will be considered duplicated if either set
    ///     of conditions is satisfied.
    /// </remarks>
    [DataField("duplicateCondition")]
    public DuplicateConditions DuplicateCondition = DuplicateConditions.All;
}
