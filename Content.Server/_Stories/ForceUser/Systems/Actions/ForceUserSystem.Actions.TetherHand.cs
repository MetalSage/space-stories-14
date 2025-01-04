using Content.Shared._Stories.ForceUser;
using Content.Shared.Weapons.Misc;
using Content.Shared._Stories.ForceUser.Actions.Events;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public const string HandTetherGunProto = "HandTetherGun";
    public void InitializeTetherHand()
    {
        SubscribeLocalEvent<ForceUserComponent, HandTetherGunEvent>(OnHandTetherGunEvent);
    }
    private void OnHandTetherGunEvent(EntityUid uid, ForceUserComponent comp, HandTetherGunEvent args)
    {
        if (args.Handled)
            return;

        if (comp.TetherHand == null)
        {
            comp.TetherHand = Spawn(HandTetherGunProto);
            _hands.TryPickupAnyHand(args.Performer, comp.TetherHand.Value);
            _popup.PopupEntity(Loc.GetString("Вы чувствуете силу в ваших руках..."), args.Performer, args.Performer); // TODO: Добавить локализацию
        }
        else
        {
            _tetherGunSystem.StopTetherGun(comp.TetherHand.Value);
            Del(comp.TetherHand.Value);
            comp.TetherHand = null;
        }

        args.Handled = true;
    }
}
