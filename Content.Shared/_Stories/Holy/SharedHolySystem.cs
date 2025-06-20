using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Prying.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Stories.Holy;

public abstract partial class SharedHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string HolyStatusEffect = "Holy";

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string HolyFaction = "Holy";

    private const string HolyDelay = "Holy";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateProtection(frameTime);
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

        if (_statusEffects.TryGetTime(entity, HolyStatusEffect, out var timeNullable) && timeNullable is { } time)
        {
            var curTime = _timing.CurTime;
            var timeLeft = (TimeSpan)(time.Item2 - curTime);
            args.PushMarkup(Loc.GetString("holy-examine-time", ("time", timeLeft.ToString("hh\\:mm\\:ss"))));
        }
        else
            args.PushMarkup(Loc.GetString("holy-examine"));
    }

    private void OnExamined(Entity<UnholyComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!IsUnholy(args.Examiner))
            if (!IsHoly(args.Examiner) || !entity.Comp.Detectable)
                return;

        args.PushMarkup(Loc.GetString("unholy-examine"));
    }

}
