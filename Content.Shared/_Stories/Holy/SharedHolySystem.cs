using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.NPC.Prototypes;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Stories.Holy;

public abstract partial class SharedHolySystem : EntitySystem
{
    private const string HolyDelay = "STHoly";
    private static readonly EntProtoId HolyStatusEffect = "STHoly";
    private static readonly ProtoId<NpcFactionPrototype> HolyFaction = "STHoly";

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateProtection(frameTime);

        var query = EntityQueryEnumerator<TemporaryHolyComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!_statusEffects.HasStatusEffect(uid, HolyStatusEffect))
            {
                RemComp<HolyComponent>(uid);
                RemComp<TemporaryHolyComponent>(uid);
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        InitializeProtection();

        SubscribeLocalEvent<HolyComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<UnholyComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<HolyComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (_statusEffects.TryGetTime(entity, HolyStatusEffect, out var time) && time.EndEffectTime is { } endTime)
        {
            var curTime = _timing.CurTime;
            var timeLeft = endTime - curTime;
            args.PushMarkup(Loc.GetString("stories-holy-examine-time", ("time", timeLeft.ToString("hh\\:mm\\:ss"))));
        }
        else
            args.PushMarkup(Loc.GetString("stories-holy-examine"));
    }

    private void OnExamined(Entity<UnholyComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!IsUnholy(args.Examiner))
        {
            if (!IsHoly(args.Examiner) || !entity.Comp.Detectable)
                return;
        }

        args.PushMarkup(Loc.GetString("stories-unholy-examine"));
    }
}

[RegisterComponent]
public sealed partial class TemporaryHolyComponent : Component
{
}
