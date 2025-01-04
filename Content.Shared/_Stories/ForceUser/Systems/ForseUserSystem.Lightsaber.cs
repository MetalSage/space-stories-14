using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._Stories.Force.Lightsaber;
using Robust.Shared.Physics.Events;
using Content.Shared.Weapons.Misc;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared._Stories.Force;
using Content.Shared.Verbs;

namespace Content.Shared._Stories.ForceUser;
public abstract partial class SharedForceUserSystem
{
    private void InitializeLightsaber()
    {
        SubscribeLocalEvent<LightsaberComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<LightsaberComponent, LightsaberConnectedEvent>(OnConnected);
        SubscribeLocalEvent<LightsaberComponent, LightsaberDetachedEvent>(OnDetached);
        SubscribeLocalEvent<LightsaberComponent, LightsaberHackedEvent>(OnHacked);
    }

    private void OnGetVerbs(EntityUid uid, LightsaberComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!HasComp<ForceUserComponent>(args.User))
            return;

        if (component.LightsaberOwner == null)
        {
            args.Verbs.Add(new InteractionVerb()
            {
                Text = Loc.GetString("lightsaber-tie"),
                Act = () =>
                {
                    var doAfterEventArgs = new Shared.DoAfter.DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(10f), new LightsaberConnectedEvent(), uid, uid)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true,
                    };

                    _doAfter.TryStartDoAfter(doAfterEventArgs);
                }
            });
        }
        else if (component.LightsaberOwner == args.User)
        {
            args.Verbs.Add(new InteractionVerb()
            {
                Text = Loc.GetString("lightsaber-untie"),
                Act = () =>
                {
                    var doAfterEventArgs = new Shared.DoAfter.DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(10f), new LightsaberDetachedEvent(), uid, uid)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true,
                    };

                    _doAfter.TryStartDoAfter(doAfterEventArgs);
                }
            });
        }
        else if (component.LightsaberOwner is { } owner && _mobState.IsAlive(owner))
        {
            args.Verbs.Add(new InteractionVerb()
            {
                Text = Loc.GetString("lightsaber-break"),
                Act = () =>
                {
                    var doAfterEventArgs = new Shared.DoAfter.DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(10f), new LightsaberHackedEvent(), uid, uid)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true,
                    };

                    _doAfter.TryStartDoAfter(doAfterEventArgs);
                }
            });
        }
    }

    private void OnConnected(EntityUid uid, LightsaberComponent component, LightsaberConnectedEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        UnbindLightsaber(args.User);
        BindLightsaber(args.User, uid);

        args.Handled = true;
    }

    private void OnDetached(EntityUid uid, LightsaberComponent component, LightsaberDetachedEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        UnbindLightsaber(args.User);

        args.Handled = true;
    }

    private void OnHacked(EntityUid uid, LightsaberComponent component, LightsaberHackedEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        BindLightsaber(args.User, uid);

        args.Handled = true;
    }

    protected void BindLightsaber(EntityUid uid, EntityUid lightsaber, ForceUserComponent? forceUserComponent = null, LightsaberComponent? lightsaberComponent = null)
    {
        if (!Resolve(uid, ref forceUserComponent))
            return;

        if (!Resolve(lightsaber, ref lightsaberComponent))
            return;

        if (lightsaberComponent.LightsaberOwner == uid)
            return;

        if (lightsaberComponent.LightsaberOwner != null)
            UnbindLightsaber(lightsaberComponent.LightsaberOwner.Value);

        _popup.PopupEntity(Loc.GetString("Вы чувствуете связь с мечом..."), uid, uid); // FIXME: Hardcode

        forceUserComponent.Lightsaber = lightsaber;
        lightsaberComponent.LightsaberOwner = uid;
    }

    protected void UnbindLightsaber(EntityUid uid, ForceUserComponent? forceUserComponent = null)
    {
        if (!Resolve(uid, ref forceUserComponent))
            return;

        if (!(forceUserComponent.Lightsaber is { } lightsaber))
            return;

        var lightsaberComponent = Comp<LightsaberComponent>(lightsaber);

        _popup.PopupEntity(Loc.GetString("Вы чувствуете разрыв связи с мечом..."), uid, uid); // FIXME: Hardcode

        forceUserComponent.Lightsaber = null;
        lightsaberComponent.LightsaberOwner = null;
    }

}
