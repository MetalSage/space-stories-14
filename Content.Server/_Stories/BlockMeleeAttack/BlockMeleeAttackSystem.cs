using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Server._Stories.BlockMeleeAttack;

public sealed partial class BlockMeleeAttackSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BlockMeleeAttackComponent, ItemToggledEvent>(ToggleReflect);
        SubscribeLocalEvent<BlockMeleeAttackUserComponent, DamageModifyEvent>(OnUserDamageModified);
        SubscribeLocalEvent<BlockMeleeAttackComponent, GotEquippedHandEvent>(OnReflectHandEquipped);
        SubscribeLocalEvent<BlockMeleeAttackComponent, GotUnequippedHandEvent>(OnReflectHandUnequipped);
        SubscribeLocalEvent<BlockMeleeAttackComponent, DamageModifyEvent>(OnDamage);
    }
    private void OnDamage(EntityUid uid, BlockMeleeAttackComponent component, DamageModifyEvent args)
    {
        if (args.Origin == null) return;
        if (!component.Enabled) return;
        if (args.Damage.GetTotal() <= 0) return;
        if (args.Origin == uid) return;

        if (!_random.Prob(component.BlockProb)) return;

        args.Damage *= 0;
        _popup.PopupEntity(Loc.GetString("Заблокировано!"), uid);
        _audio.PlayPvs(component.BlockSound, uid);
    }
    private void ToggleReflect(EntityUid uid, BlockMeleeAttackComponent comp, ItemToggledEvent args)
    {
        comp.Enabled = args.Activated;
    }

    private void OnUserDamageModified(EntityUid uid, BlockMeleeAttackUserComponent component, DamageModifyEvent args)
    {
        if (TryComp<BlockMeleeAttackComponent>(component.BlockingItem, out var blocking))
        {
            if (args.Origin == null) return;
            if (!blocking.Enabled) return;
            if (args.Damage.GetTotal() <= 0) return;
            if (args.Origin == uid || args.Origin == component.BlockingItem) return;

            if (!_random.Prob(blocking.BlockProb)) return;

            args.Damage *= 0;
            _popup.PopupEntity(Loc.GetString("Заблокировано!"), component.BlockingItem.Value);
            _audio.PlayPvs(blocking.BlockSound, uid);
        }
    }

    private void OnReflectHandEquipped(EntityUid uid, BlockMeleeAttackComponent component, GotEquippedHandEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        component.User = args.User;
        EnsureComp<BlockMeleeAttackUserComponent>(args.User).BlockingItem = uid;
    }

    private void OnReflectHandUnequipped(EntityUid uid, BlockMeleeAttackComponent component, GotUnequippedHandEvent args)
    {
        if (component.User != null) RemComp<BlockMeleeAttackUserComponent>(component.User.Value);
    }
}


