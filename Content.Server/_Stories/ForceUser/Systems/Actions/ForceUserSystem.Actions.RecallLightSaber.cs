using Content.Shared._Stories.ForceUser;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Timing;
using Robust.Shared.Timing;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Misc;
using Content.Server._Stories.TetherGun;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared._Stories.PullTo;
using Content.Shared._Stories.Force.Lightsaber;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeRecall()
    {
        SubscribeLocalEvent<ForceUserComponent, RecallLightsaberEvent>(OnRecall);
        SubscribeLocalEvent<LightsaberComponent, PulledToTimeOutEvent>(OnTimeOut);
    }
    private void OnTimeOut(EntityUid uid, LightsaberComponent comp, PulledToTimeOutEvent args)
    {
        if (args.Handled || comp.LightsaberOwner != args.Component.PulledTo || args.Component.PulledTo == null)
            return;

        _popup.PopupEntity(Loc.GetString(_hands.TryPickupAnyHand(args.Component.PulledTo.Value, uid) ? "Ваш световой меч телепортируется вам в руку!" : "ninja-hands-full"), args.Component.PulledTo.Value, args.Component.PulledTo.Value);
    }
    private void OnRecall(EntityUid uid, ForceUserComponent comp, RecallLightsaberEvent args)
    {
        if (args.Handled || comp.Lightsaber == null)
            return;

        if (_container.IsEntityInContainer(comp.Lightsaber.Value) && !_container.TryRemoveFromContainer(comp.Lightsaber.Value))
            return;

        if (TryComp<TetheredComponent>(comp.Lightsaber.Value, out var tetheredComponent))
            _tetherGunSystem.StopTether(tetheredComponent.Tetherer, EnsureComp<TetherGunComponent>(tetheredComponent.Tetherer));

        _pullTo.TryPullTo(comp.Lightsaber.Value, uid, PulledToOnEnter.PickUp, duration: 10f);

        args.Handled = true;
    }
}
