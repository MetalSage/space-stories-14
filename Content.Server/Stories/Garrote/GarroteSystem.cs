using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Body.Components;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Stories.Garrote;

namespace Content.Server.Stories.Garrote;

public sealed class GarroteSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly RespiratorSystem _respirator = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GarroteComponent, AfterInteractEvent>(OnGarroteAttempt);
        SubscribeLocalEvent<GarroteComponent, GarroteDoAfterEvent>(OnGarroteDoAfter);
    }

    private void OnGarroteAttempt(EntityUid uid, GarroteComponent comp, ref AfterInteractEvent args)
    {
        if (args.User == args.Target
        || !HasComp<BodyComponent>(args.Target)
        || !HasComp<DamageableComponent>(args.Target)
        || !TryComp<MobStateComponent>(args.Target, out var mobstate))
            return;

        if (TryComp<WieldableComponent>(uid, out var wieldable) && !wieldable.Wielded)
        {
            var message = Loc.GetString("wieldable-component-requires", ("item", uid));
            _popupSystem.PopupEntity(message, uid, args.User);
            return;
        }

        if (!(mobstate.CurrentState == MobState.Alive && HasComp<RespiratorComponent>(args.Target)))
        {
            var message = Loc.GetString("garrote-component-doesnt-breath", ("target", args.Target));
            _popupSystem.PopupEntity(message, args.Target.Value, args.User);
            return;
        }

        if (!TryComp(args.User, out TransformComponent? userTransform))
            return;

        if (!TryComp(args.Target.Value, out TransformComponent? targetTransform))
            return;

        if (!args.CanReach || !IsRightTargetDistance(userTransform, targetTransform, comp.MaxUseDistance))
        {
            var message = Loc.GetString("garrote-component-too-far-away", ("target", args.Target));
            _popupSystem.PopupEntity(message, args.Target.Value, args.User);
            return;
        }

        if (GetEntityDirection(userTransform) != GetEntityDirection(targetTransform) && _actionBlocker.CanInteract(args.Target.Value, null))
        {
            var message = Loc.GetString("garrote-component-must-be-behind", ("target", args.Target));
            _popupSystem.PopupEntity(message, args.Target.Value, args.User);
            return;
        }

        var messagetarget = Loc.GetString("garrote-component-started-target", ("user", args.User));
        _popupSystem.PopupEntity(messagetarget, args.User, args.Target.Value, PopupType.LargeCaution);

        var messageothers = Loc.GetString("garrote-component-started-others", ("user", args.User), ("target", args.Target));
        _popupSystem.PopupEntity(messageothers, args.User, Filter.PvsExcept(args.Target.Value), true, PopupType.MediumCaution);

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new GarroteDoAfterEvent(), uid, target: args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTool,
            DistanceThreshold = 0.1f
        };

        if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
            return;

        ProtoId<EmotePrototype> emote = "Cough";
        _chatSystem.TryEmoteWithChat(args.Target.Value, emote, ChatTransmitRange.HideChat, ignoreActionBlocker: true);

        _stun.TryStun(args.Target.Value, 2*comp.DoAfterTime, true); // multiplying time by 2 to prevent mispredictons
        _statusEffect.TryAddStatusEffect<MutedComponent>(args.Target.Value, "Muted", 2*comp.DoAfterTime, true);
    }

    private void OnGarroteDoAfter(EntityUid uid, GarroteComponent comp, GarroteDoAfterEvent args)
    {
        if (args.Target == null
        || !TryComp<DamageableComponent>(args.Target, out var damageable)
        || !TryComp<RespiratorComponent>(args.Target, out var respirator)
        || !TryComp<MobStateComponent>(args.Target, out var mobstate))
            return;

        if (args.Cancelled || mobstate.CurrentState != MobState.Alive)
            return;

        _damageable.TryChangeDamage(args.Target, comp.Damage, false, origin: args.User);

        var saturationDelta = respirator.MinSaturation - respirator.Saturation;
        _respirator.UpdateSaturation(args.Target.Value, saturationDelta, respirator);

        _stun.TryStun(args.Target.Value, 2*comp.DoAfterTime, true);
        _statusEffect.TryAddStatusEffect<MutedComponent>(args.Target.Value, "Muted", 2*comp.DoAfterTime, true);

        args.Repeat = true;
    }

    /// <summary>
    ///     Checking whether the distance from the user to the target is set correctly.
    /// </summary>
    /// <remarks>
    ///     Does not check for the presence of TransformComponent.
    /// </remarks>
    private bool IsRightTargetDistance(TransformComponent user, TransformComponent target, float maxUseDistance)
    {
        if (Math.Abs(user.LocalPosition.X - target.LocalPosition.X) <= maxUseDistance
            && Math.Abs(user.LocalPosition.Y - target.LocalPosition.Y) <= maxUseDistance)
            return true;
        else
            return false;
    }

    /// <remarks>
    ///     Does not check for the presence of TransformComponent.
    /// </remarks>
    private Direction GetEntityDirection(TransformComponent entityTransform)
    {
        double entityLocalRotation;

        if (entityTransform.LocalRotation.Degrees < 0)
            entityLocalRotation = 360 - Math.Abs(entityTransform.LocalRotation.Degrees);
        else
            entityLocalRotation = entityTransform.LocalRotation.Degrees;

        if(entityLocalRotation > 43.5d && entityLocalRotation < 136.5d)
            return Direction.East;
        else if(entityLocalRotation >= 136.5d && entityLocalRotation <= 223.5d)
            return Direction.North;
        else if(entityLocalRotation > 223.5d && entityLocalRotation < 316.5d)
            return Direction.West;
        else
            return Direction.South;
    }
}
