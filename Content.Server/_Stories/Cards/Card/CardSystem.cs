using Content.Server._Stories.Cards.Stack;
using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Server._Stories.Cards.Card;

public sealed class CardSystem : SharedCardSystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

    }
    protected override void CreateFan(EntityUid user, EntityUid target, CardComponent? component = null)
    {
        var fanComponent = new CardFanComponent();
        var usedEntity = _handsSystem.GetActiveItem(user);

        if (usedEntity == target
            || usedEntity == null
            || !_tagSystem.HasTag(usedEntity.Value, "STCard"))
            return;

        var spawnPos = Transform(user).Coordinates;

        var entityCreated = Spawn("STCardFan", spawnPos);

        if (!TryComp<CardStackComponent>(entityCreated, out var stackComp)
            || !TryComp<CardFanComponent>(entityCreated, out var fanComp))
            return;

        fanComponent = fanComp;
        RaiseLocalEvent(entityCreated, new CardAddedEvent(entityCreated, usedEntity.Value));
        RaiseLocalEvent(entityCreated, new CardAddedEvent(entityCreated, target));
        _handsSystem.TryPickupAnyHand(user, entityCreated);
        Dirty(entityCreated, stackComp);

        _audio.PlayLocal(fanComponent.AddCard, usedEntity.Value, user);
    }
    protected override void CreateDeck(EntityUid user, EntityUid target, CardComponent? component = null)
    {
        var usedEntity = _handsSystem.GetActiveItem(user);

        if (usedEntity == target
            || usedEntity == null
            || !_tagSystem.HasTag(usedEntity.Value, "STCard"))
            return;

        var spawnPos = Transform(user).Coordinates;
        var entityCreated = Spawn("STCardDeck", spawnPos);

        if (!TryComp<CardStackComponent>(entityCreated, out var stackComp))
            return;

        RaiseLocalEvent(entityCreated, new CardAddedEvent(entityCreated, usedEntity.Value));
        RaiseLocalEvent(entityCreated, new CardAddedEvent(entityCreated, target));

        _handsSystem.TryPickupAnyHand(user, entityCreated);
        _audio.PlayLocal(stackComp.AddCard, usedEntity.Value, user);
    }
}
