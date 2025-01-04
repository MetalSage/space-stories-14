using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared._Stories.ForceUser;
using Content.Server._Stories.ForceUser.ProtectiveBubble.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Stories.Force.Lightsaber;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;
using Robust.Shared.Serialization.Manager;
using Content.Shared._Stories.Force;
using Content.Shared.Rounding;
using Content.Shared.Damage;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem
{
    public void InitializeUser()
    {
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, StopProtectiveBubbleEvent>(OnStopProtectiveBubble);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, AttackedEvent>(OnAttack);
    }
    public void UpdateUser(float frameTime)
    {
        var query = EntityQueryEnumerator<ProtectiveBubbleUserComponent, ForceUserComponent>();
        while (query.MoveNext(out var uid, out var bubbleUser, out var forceUser))
        {
            if (bubbleUser == null)
                return;
            if (_force.TryRemoveVolume(uid, frameTime * bubbleUser.VolumeCost))
                _damageable.TryChangeDamage(bubbleUser.ProtectiveBubble, bubbleUser.Regeneration * frameTime, true);
        }
    }
    private void OnAttack(EntityUid uid, ProtectiveBubbleUserComponent component, AttackedEvent args)
    {
        args.BonusDamage = _meleeWeapon.GetDamage(args.Used, args.User) * -1;
        _damageable.TryChangeDamage(component.ProtectiveBubble, _meleeWeapon.GetDamage(args.Used, args.User), true);
    }
    private void OnInit(EntityUid uid, ProtectiveBubbleUserComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.StopProtectiveBubbleActionEntity, out var act, component.StopProtectiveBubbleAction);
        _alerts.ShowAlert(uid, "ProjectiveBubble", 0);
    }
    private void OnShutdown(EntityUid uid, ProtectiveBubbleUserComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(component.StopProtectiveBubbleActionEntity);
        _alerts.ClearAlert(uid, "ProjectiveBubble");
    }
    private void OnStopProtectiveBubble(EntityUid uid, ProtectiveBubbleUserComponent comp, StopProtectiveBubbleEvent args)
    {
        Del(comp.ProtectiveBubble);
    }
}
