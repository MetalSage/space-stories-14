using Content.Server.Polymorph.Systems;
using Content.Shared._Stories.Spaf;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Nutrition.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Spaf;

public sealed class SpafSystem : SharedSpafSystem
{
    private const string PacifiedKey = "Pacified";
    private const float PacifiedTime = 3f;
    private const float PacifiedRange = 5f;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpafComponent, SpafPolymorphEvent>(OnPolymorph);

        SubscribeLocalEvent<EdibleComponent, LandEvent>(OnFoodLand);
    }

    private void OnFoodLand(EntityUid uid, EdibleComponent component, ref LandEvent args)
    {
        var ents = _lookup.GetEntitiesInRange<SpafComponent>(Transform(uid).Coordinates, PacifiedRange);

        foreach (var ent in ents)
        {
            _statusEffects.TryAddStatusEffect<PacifiedComponent>(ent,
                PacifiedKey,
                TimeSpan.FromSeconds(PacifiedTime),
                true);
        }
    }

    private void OnPolymorph(EntityUid uid, SpafComponent component, SpafPolymorphEvent args)
    {
        if (args.Handled || !TryModifyHunger(args.Performer, args.HungerCost))
            return;

        if (!_prototype.TryIndex(args.ProtoId, out var prototype))
            return;

        _polymorph.PolymorphEntity(args.Performer, prototype.Configuration);

        args.Handled = true;
    }
}
