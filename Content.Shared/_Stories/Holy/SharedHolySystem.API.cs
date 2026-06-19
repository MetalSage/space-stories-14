using Content.Shared.Timing;

namespace Content.Shared._Stories.Holy;

public abstract partial class SharedHolySystem : EntitySystem
{
    public bool TryApplyProtection(EntityUid target, Entity<HolyComponent> holy)
    {
        if (!IsUnholy(target))
            return false;

        if (!TryComp<UnholyComponent>(target, out var unholy))
            return false;

        if (TryComp<UseDelayComponent>(holy, out var useDelay))
        {
            if (_useDelay.TryGetDelayInfo((holy, useDelay), out _, HolyDelay)) // Если Delay настроен
            {
                if (!_useDelay.TryResetDelay((holy, useDelay), true, HolyDelay))
                    return false;
            }
        }

        ApplyProtection((target, unholy), holy);
        return true;
    }

    private void ApplyProtection(Entity<UnholyComponent> target, Entity<HolyComponent> holy)
    {
        if (target.Comp.ResistanceCoefficient == 0)
            return;

        // TODO: HolyProtectionEvent

        if (holy.Comp.ProtectionSound is { } sound)
            _audio.PlayPvs(sound, holy);

        _damageable.TryChangeDamage(target.Owner, holy.Comp.ProtectionDamage * target.Comp.ResistanceCoefficient);

        if (!target.Comp.IgnoreProtectionImpulse)
        {
            _stun.TryKnockdown(target.Owner, holy.Comp.ProtectionKnockdownTime);
            var fieldDir = _transformSystem.GetWorldPosition(holy);
            var playerDir = _transformSystem.GetWorldPosition(target);
            _throwing.TryThrow(target,
                (playerDir - fieldDir) * holy.Comp.ProtectionImpulseLengthModifier,
                holy.Comp.ProtectionImpulseSpeed);
        }
    }

    public bool TryBless(EntityUid uid, TimeSpan time, bool refresh = true)
    {
        if (IsUnholy(uid))
            return false;

        if (!TryComp<BlessableComponent>(uid, out var blessable))
            return false;

        if (!_statusEffects.CanAddStatusEffect(uid, HolyStatusEffect))
            return false;

        // TODO: BlessAttemptEvent

        Bless((uid, blessable), time, refresh);
        return true;
    }

    public bool TryBless(EntityUid uid)
    {
        if (IsUnholy(uid))
            return false;

        if (!TryComp<BlessableComponent>(uid, out var blessable))
            return false;

        if (!_statusEffects.CanAddStatusEffect(uid, HolyStatusEffect))
            return false;

        // TODO: BlessAttemptEvent

        Bless((uid, blessable));
        return true;
    }

    private void Bless(Entity<BlessableComponent> entity, TimeSpan time, bool refresh = true)
    {
        var duration = time * entity.Comp.TimeModifier;
        if (refresh)
            _statusEffects.TrySetStatusEffectDuration(entity.Owner, HolyStatusEffect, duration);
        else
            _statusEffects.TryAddStatusEffectDuration(entity.Owner, HolyStatusEffect, duration);

        if (!HasComp<HolyComponent>(entity))
        {
            EnsureComp<HolyComponent>(entity);
            EnsureComp<TemporaryHolyComponent>(entity);
        }
    }

    private void Bless(Entity<BlessableComponent> entity)
    {
        _statusEffects.TrySetStatusEffectDuration(entity.Owner, HolyStatusEffect);

        if (!HasComp<HolyComponent>(entity))
        {
            EnsureComp<HolyComponent>(entity);
            EnsureComp<TemporaryHolyComponent>(entity);
        }
    }

    public bool IsUnholy(EntityUid uid)
    {
        return HasComp<UnholyComponent>(uid);
    }

    public bool IsHoly(EntityUid uid)
    {
        return HasComp<HolyComponent>(uid);
    }
}
