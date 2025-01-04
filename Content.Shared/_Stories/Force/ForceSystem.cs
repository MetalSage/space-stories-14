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
using Robust.Shared.Timing;
using Content.Shared.Atmos.Piping;
using Content.Shared.Body.Components;
using Content.Shared._Stories.ForceUser;
using Robust.Shared.Utility;
using Content.Shared.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._Stories.Force;
public sealed partial class ForceSystem : EntitySystem // TODO: Навести порядок с Float и FixedPoint2
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    public override void Initialize()
    {
        base.Initialize();
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ForceComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            comp.CurrentDebuff += comp.PassiveDebuff * frameTime;
            if (comp.CurrentDebuff <= 0)
                comp.CurrentDebuff = 0f;

            RefreshDebuffs(uid);

            if (comp.CurrentDebuff == 0 && _mobState.IsAlive(uid))
                if (!TryAddVolume(uid, comp.PassiveVolume.Float() * frameTime, comp) && comp.Volume + comp.PassiveVolume * frameTime > comp.MaxVolume)
                    TryAddVolume(uid, (comp.MaxVolume.Float() - comp.Volume.Float()), comp);
        }
    }
    public bool SetVolume(EntityUid uid, float volume, float passiveVolume, float maxVolume, ForceComponent? component = null)
    {
        if (volume <= 0 || maxVolume <= 0 || volume > maxVolume || passiveVolume <= 0)
        {
            DebugTools.Assert(volume < maxVolume, "Attempted to set volume bigger than max volume"); // ? Дать возможность превышать лимит ?
            DebugTools.Assert(volume == 0, "Attempted to set negative value to volume");
            DebugTools.Assert(maxVolume > 0, "Attempted to set negative value or 0 to max volume");
            DebugTools.Assert(passiveVolume == 0, "Attempted to set negative value to passive volume");
            return false;
        }

        if (!Resolve(uid, ref component))
            return false;

        component.Volume = volume;
        component.MaxVolume = maxVolume;
        component.PassiveVolume = passiveVolume;
        return true;
    }
    /// <summary>
    ///     Попытаться перенести некоторое количество маны из существа в другое существо.
    /// </summary>
    /// <param name="uid">Откуда будет забрано.</param>
    /// <param name="toUid">Куда будет добавлено.</param>
    /// <param name="amount">Сколько будет перенесенно.</param>
    public bool TryTransferVolume(EntityUid uid, EntityUid toUid, float amount, ForceComponent? component = null, ForceComponent? toComponent = null)
    {
        if (amount <= 0)
        {
            DebugTools.Assert(amount == 0, "Attempted to transfer negative force volume");
            return false;
        }

        if (!Resolve(uid, ref component) || !Resolve(toUid, ref toComponent))
            return false;

        if (component.Volume - amount < 0)
            return false;

        if (toComponent.Volume + amount > toComponent.MaxVolume)
            return false;

        var ev = new TransferVolumeAttemptEvent(toUid, uid, oldVolume: toComponent.Volume, newVolume: toComponent.Volume + amount);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return false;

        return TransferVolume(uid, toUid, amount, component, toComponent);
    }

    /// <summary>
    ///     Перенести некоторое количество маны из существа в другое существо.
    ///     В отличии от <see cref="TryTransferVolume"/> оно может переполнить <see cref="ForceComponent.Volume"/> сущности в которую переносят.
    /// </summary>
    /// <param name="uid">Откуда будет забрано.</param>
    /// <param name="toUid">Куда будет добавлено.</param>
    /// <param name="amount">Сколько будет перенесенно.</param>
    private bool TransferVolume(EntityUid uid, EntityUid toUid, float amount, ForceComponent? component = null, ForceComponent? toComponent = null)
    {
        if (amount <= 0)
        {
            DebugTools.Assert(amount == 0, "Attempted to transfer negative force volume");
            return false;
        }

        if (!Resolve(uid, ref component) || !Resolve(toUid, ref toComponent))
            return false;

        if (component.Volume - amount < 0)
            return false;

        if (TryRemoveVolume(uid, amount, component))
            return AddVolume(toUid, amount, toComponent);

        return false;
    }

    /// <summary>
    ///     Попытаться добавить ману существу. Не может переполнить ману выше лимита.
    /// </summary>
    /// <param name="uid">Существу куда добавлять.</param>
    /// <param name="toAdd">Сколько добавлять.</param>
    public bool TryAddVolume(EntityUid uid, float toAdd, ForceComponent? component = null)
    {
        if (toAdd <= 0)
        {
            DebugTools.Assert(toAdd == 0, "Attempted to add negative force volume");
            return false;
        }
        if (!Resolve(uid, ref component))
            return false;
        if (component.Volume + toAdd > component.MaxVolume)
            return false;

        var ev = new VolumeChangeAttemptEvent(uid, oldVolume: component.Volume, newVolume: component.Volume + toAdd, component.CurrentDebuff.Float(), component.MaxVolume);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return false;

        return AddVolume(uid, toAdd, component);
    }

    /// <summary>
    ///     Добавить ману существу. Может переполнить ману выше лимита.
    /// </summary>
    /// <param name="uid">Существу куда добавлять.</param>
    /// <param name="toAdd">Сколько добавлять.</param>
    public bool AddVolume(EntityUid uid, float toAdd, ForceComponent? component = null)
    {
        if (toAdd <= 0)
        {
            DebugTools.Assert(toAdd == 0, "Attempted to add negative force volume");
            return false;
        }
        if (!Resolve(uid, ref component))
            return false;

        var ev = new VolumeChangedEvent(uid, oldVolume: component.Volume, newVolume: component.Volume + toAdd, component.CurrentDebuff.Float(), component.MaxVolume);
        RaiseLocalEvent(uid, ev);

        component.Volume += toAdd;

        RefreshDebuffs(uid);
        return true;
    }

    /// <summary>
    ///     Попытаться убрать ману из существа. Не может сделать значение маны отрицательным.
    /// </summary>
    /// <param name="uid">Существу у которого будет уменьшено.</param>
    /// <param name="toRemove">На сколько уменьшить.</param>
    public bool TryRemoveVolume(EntityUid uid, float toRemove, ForceComponent? component = null)
    {
        if (toRemove <= 0)
        {
            DebugTools.Assert(toRemove == 0, "Attempted to remove negative force volume");
            return false;
        }
        if (!Resolve(uid, ref component))
            return false;
        if (component.Volume - toRemove < 0)
            return false;

        var ev = new VolumeChangeAttemptEvent(uid, oldVolume: component.Volume, newVolume: component.Volume - toRemove, component.CurrentDebuff.Float(), component.MaxVolume);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return false;

        return RemoveVolume(uid, toRemove, component);
    }

    /// <summary>
    ///     Убрать ману из существа. Не может сделать значение маны отрицательным.
    /// </summary>
    /// <param name="uid">Существу у которого будет уменьшено.</param>
    /// <param name="toRemove">На сколько уменьшить.</param>
    public bool RemoveVolume(EntityUid uid, float toRemove, ForceComponent? component = null)
    {
        if (toRemove <= 0)
        {
            DebugTools.Assert(toRemove == 0, "Attempted to remove negative force volume");
            return false;
        }
        if (!Resolve(uid, ref component))
            return false;
        if (component.Volume - toRemove < 0)
            return false;

        var ev = new VolumeChangedEvent(uid, oldVolume: component.Volume, newVolume: component.Volume - toRemove, component.CurrentDebuff.Float(), component.MaxVolume);
        RaiseLocalEvent(uid, ev);

        component.Volume -= toRemove;

        component.CurrentDebuff += (toRemove / component.MaxVolume.Float()) * 10;
        return true;
    }

    #region Debuffs
    public void RefreshDebuffs(EntityUid uid, ForceComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;
        RaiseLocalEvent(uid, new RefreshDebuffsEvent(uid, component.CurrentDebuff.Float(), component.Volume.Float(), component.MaxVolume.Float()));
    }
    #endregion
}
public sealed class RefreshDebuffsEvent : EntityEventArgs
{
    public readonly EntityUid EntityUid;
    public readonly float CurrentDebuff;
    public readonly float CurrentVolume;
    public readonly float MaxVolume;
    public RefreshDebuffsEvent(EntityUid uid, float currentDebuff, float currentVolume, float maxVolume)
    {
        EntityUid = uid;
        CurrentDebuff = currentDebuff;
        CurrentVolume = currentVolume;
        MaxVolume = maxVolume;
    }
}
public sealed class TransferVolumeAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid EntityUid;
    public readonly EntityUid User;
    public readonly FixedPoint2 OldVolume;
    public readonly FixedPoint2 NewVolume;
    public TransferVolumeAttemptEvent(EntityUid uid, EntityUid user, FixedPoint2 oldVolume, FixedPoint2 newVolume)
    {
        EntityUid = uid;
        User = user;
        OldVolume = oldVolume;
        NewVolume = newVolume;
    }
}
public sealed class VolumeChangeAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid EntityUid;
    public readonly float CurrentDebuff;
    public readonly FixedPoint2 OldVolume;
    public readonly FixedPoint2 NewVolume;
    public readonly FixedPoint2 MaxVolume;
    public VolumeChangeAttemptEvent(EntityUid uid, FixedPoint2 oldVolume, FixedPoint2 newVolume, float currentDebuff, FixedPoint2 maxVolume)
    {
        EntityUid = uid;
        CurrentDebuff = currentDebuff;
        OldVolume = oldVolume;
        NewVolume = newVolume;
        MaxVolume = maxVolume;
    }
}
public sealed class VolumeChangedEvent : CancellableEntityEventArgs
{
    public readonly EntityUid EntityUid;
    public readonly float CurrentDebuff;
    public readonly FixedPoint2 OldVolume;
    public readonly FixedPoint2 NewVolume;
    public readonly FixedPoint2 MaxVolume;
    public VolumeChangedEvent(EntityUid uid, FixedPoint2 oldVolume, FixedPoint2 newVolume, float currentDebuff, FixedPoint2 maxVolume)
    {
        EntityUid = uid;
        CurrentDebuff = currentDebuff;
        OldVolume = oldVolume;
        NewVolume = newVolume;
        MaxVolume = maxVolume;
    }
}
