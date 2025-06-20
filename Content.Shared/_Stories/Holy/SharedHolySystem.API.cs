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
                if (!_useDelay.TryResetDelay((holy, useDelay), true, HolyDelay))
                    return false;
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

        _damageable.TryChangeDamage(target, holy.Comp.ProtectionDamage * target.Comp.ResistanceCoefficient);

        if (!target.Comp.IgnoreProtectionImpulse)
        {
            _stun.TryKnockdown(target, holy.Comp.ProtectionKnockdownTime, true);
            var fieldDir = _transformSystem.GetWorldPosition(holy);
            var playerDir = _transformSystem.GetWorldPosition(target);
            _throwing.TryThrow(target, (playerDir - fieldDir) * holy.Comp.ProtectionImpulseLengthModifier, holy.Comp.ProtectionImpulseSpeed);
        }
    }

    public bool TryBless(EntityUid uid, TimeSpan time, bool refresh = true)
    {
        if (IsUnholy(uid))
            return false;

        if (!TryComp<BlessableComponent>(uid, out var blessable))
            return false;

        if (!_statusEffects.CanApplyEffect(uid, HolyStatusEffect))
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

        // TODO: BlessAttemptEvent

        Bless((uid, blessable));
        return true;
    }

    private void Bless(Entity<BlessableComponent> entity, TimeSpan time, bool refresh = true)
    {
        // TODO: BlessedEvent
        _statusEffects.TryAddStatusEffect<HolyComponent>(entity, HolyStatusEffect, time * entity.Comp.TimeModifier, refresh);
    }

    private void Bless(Entity<BlessableComponent> entity)
    {
        // TODO: BlessedEvent
        EnsureComp<HolyComponent>(entity);
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
