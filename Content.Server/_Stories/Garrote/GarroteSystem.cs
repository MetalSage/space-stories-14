using Content.Server.Body.Components;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Player;
using Content.Shared._Stories.Weapons.Special.Garrote;

namespace Content.Server._Stories.Garrote;

public sealed class GarroteSystem : SharedGarroteSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GarroteComponent, AfterInteractEvent>(OnGarroteAttempt);
    }

    private void OnGarroteAttempt(EntityUid uid, GarroteComponent comp, ref AfterInteractEvent args)
    {
        if (args.Target == null || args.User == args.Target)
            return;

        if(!IsСanStrangled(uid, args.Target, args.User))
            return;

        if(!CanUseGarrote(comp, args))
            return;

        UseGarrote(uid, comp, args);
    }

    private bool IsСanStrangled(EntityUid uid, EntityUid? target, EntityUid user)
    {
        if (!HasComp<BodyComponent>(target) || !HasComp<DamageableComponent>(target))
            return false;

        if (TryComp<WieldableComponent>(uid, out var wieldAble) && !wieldAble.Wielded)
        {
            var message = Loc.GetString("wieldable-component-requires", ("item", uid));
            _popupSystem.PopupEntity(message, uid, user);
            return false;
        }

        if (!TryComp<MobStateComponent>(target, out var mobState)
            && !(mobState?.CurrentState == MobState.Alive && HasComp<RespiratorComponent>(target)))
        {
            var message = Loc.GetString("strangled-doesnt-breath", ("target", target));
            _popupSystem.PopupEntity(message, target.Value, user);
            return false;
        }

        return true;
    }

    private bool CanUseGarrote(GarroteComponent comp, AfterInteractEvent args)
    {
        if (args.Target == null)
            return false;

        if (!TryComp(args.User, out TransformComponent? userTransform)
            || !TryComp(args.Target.Value, out TransformComponent? targetTransform))
            return false;

        if (!args.CanReach || !IsRightTargetDistance(userTransform, targetTransform, comp.MaxUseDistance))
        {
            var message = Loc.GetString("garrote-component-too-far-away", ("target", args.Target));
            _popupSystem.PopupEntity(message, args.Target.Value, args.User);
            return false;
        }

        if (comp.CheckDirection && GetEntityDirection(userTransform) != GetEntityDirection(targetTransform) && _actionBlocker.CanInteract(args.Target.Value, null))
        {
            var message = Loc.GetString("garrote-component-must-be-behind", ("target", args.Target));
            _popupSystem.PopupEntity(message, args.Target.Value, args.User);
            return false;
        }

        return true;
    }

    private void UseGarrote(EntityUid uid, GarroteComponent comp, AfterInteractEvent args)
    {
        if (args.Target == null)
            return;

        var messageTarget = Loc.GetString("garrote-component-started-target", ("user", args.User));
        _popupSystem.PopupEntity(messageTarget, args.User, args.Target.Value, PopupType.LargeCaution);

        var messageOthers = Loc.GetString("garrote-component-started-others", ("user", args.User), ("target", args.Target));
        _popupSystem.PopupEntity(messageOthers, args.User, Filter.PvsExcept(args.Target.Value), true, PopupType.MediumCaution);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new GarroteDoAfterEvent(), uid, target: args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTool
        });

        _chatSystem.TryEmoteWithChat(args.Target.Value, "Cough", ChatTransmitRange.HideChat, ignoreActionBlocker: true);
    }
}
