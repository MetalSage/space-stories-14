using Robust.Shared.Prototypes;
using Content.Server.Polymorph.Systems;
using Content.Shared._Stories.Spaf;
using Content.Shared.Throwing;
using Content.Server.Nutrition.Components;
using Content.Shared.StatusEffect;
using Content.Shared.CombatMode.Pacification;

namespace Content.Server._Stories.Spaf;

public sealed partial class SpafSystem : SharedSpafSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    private const string PacifiedKey = "Pacified";
    private const float PacifiedTime = 3f;
    private const float PacifiedRange = 5f;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpafComponent, SpafPolymorphEvent>(OnPolymorph);

        SubscribeLocalEvent<FoodComponent, LandEvent>(OnFoodLand);
    }

    private void OnFoodLand(EntityUid uid, FoodComponent component, ref LandEvent args)
    {
        var ents = _lookup.GetEntitiesInRange<SpafComponent>(Transform(uid).Coordinates, PacifiedRange);

        foreach (var ent in ents)
        {
            _statusEffects.TryAddStatusEffect<PacifiedComponent>(ent, PacifiedKey, TimeSpan.FromSeconds(PacifiedTime), true);
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
