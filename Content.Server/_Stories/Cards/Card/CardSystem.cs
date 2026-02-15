using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Stories.Cards.Card;

public sealed class CardSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CardComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<CardComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnActivateInWorld(EntityUid uid, CardComponent comp, ActivateInWorldEvent args)
    {
        CreateCardStack(args.User, args.Target, "STCardDeck");
    }

    private void OnInteractUsing(EntityUid uid, CardComponent comp, InteractUsingEvent args)
    {
        CreateCardStack(args.User, args.Target, "STCardFan");
    }

    private void CreateCardStack(EntityUid user, EntityUid target, string stackPrototypeId)
    {
        var usedEntity = _handsSystem.GetActiveItem(user);
        if (usedEntity == target || usedEntity == null || !TryComp<CardComponent>(usedEntity, out _))
            return;

        var spawnPos = Transform(user).Coordinates;
        var entityCreated = Spawn(stackPrototypeId, spawnPos);

        if (!TryComp<CardStackComponent>(entityCreated, out var stackComp))
            return;

        _containerSystem.Insert(usedEntity.Value, stackComp.CardContainer);
        _containerSystem.Insert(target, stackComp.CardContainer);

        _handsSystem.TryPickupAnyHand(user, entityCreated);

        var addCardSound = TryComp<CardFanComponent>(entityCreated, out var fanComp) ? fanComp.AddCardSound : stackComp.AddCardSound;
        _audio.PlayPvs(addCardSound, usedEntity.Value);
        Dirty(entityCreated, stackComp);
    }
}
