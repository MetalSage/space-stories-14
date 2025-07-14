using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Tag;

using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;

namespace Content.Server._Stories.Cards.Card;

public sealed class CardSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CardComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<CardComponent, InteractUsingEvent>(OnInteractUsing);
    }
    private void OnActivateInWorld(EntityUid uid, CardComponent comp, ActivateInWorldEvent args)
    {
        CreateDeck(args.User, args.Target, comp);
    }
    private void OnInteractUsing(EntityUid uid, CardComponent comp, InteractUsingEvent args)
    {
        CreateFan(args.User, args.Target, comp);
    }

    private void CreateFan(EntityUid user, EntityUid target, CardComponent? component = null)
    {
        var usedEntity = _handsSystem.GetActiveItem(user);
        if (usedEntity == target || usedEntity == null
            || !TryComp<CardComponent>(usedEntity, out _))
            return;

        var spawnPos = Transform(user).Coordinates;
        var entityCreated = Spawn("STCardFan", spawnPos);

        if (!TryComp<CardStackComponent>(entityCreated, out var stackComp)
            || !TryComp<CardFanComponent>(entityCreated, out var fanComp))
            return;
        _containerSystem.Insert(usedEntity.Value, stackComp.CardContainer);
        _containerSystem.Insert(target, stackComp.CardContainer);

        _handsSystem.TryPickupAnyHand(user, entityCreated);

        _audio.PlayLocal(fanComp.AddCardSound, usedEntity.Value, user);
        Dirty(entityCreated, stackComp);
    }
    private void CreateDeck(EntityUid user, EntityUid target, CardComponent? component = null)
    {
        var usedEntity = _handsSystem.GetActiveItem(user);
        if (usedEntity == target || usedEntity == null || component == null)
            return;

        var spawnPos = Transform(user).Coordinates;
        var entityCreated = Spawn("STCardDeck", spawnPos);

        if (!TryComp<CardStackComponent>(entityCreated, out var stackComp))
            return;
        _containerSystem.Insert(usedEntity.Value, stackComp.CardContainer);
        _containerSystem.Insert(target, stackComp.CardContainer);

        _handsSystem.TryPickupAnyHand(user, entityCreated);

        _audio.PlayLocal(stackComp.AddCardSound, usedEntity.Value, user);
        Dirty(entityCreated, stackComp);
    }
}
