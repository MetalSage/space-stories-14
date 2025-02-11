using Robust.Shared.Network;
using Robust.Shared.Audio.Systems;
using Content.Shared.Tag;
using Content.Shared.Interaction;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Foldable;

using Content.Shared._Stories.Cards.Stack;
using Content.Shared._Stories.Cards.Fan;


namespace Content.Shared._Stories.Cards.Card;
public sealed class CardSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly CardStackSystem _stackSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<CardComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CardFanComponent, CardSelectedMessage>(OnCardSelected);
        SubscribeLocalEvent<CardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, CardComponent component, ExaminedEvent args)
    {
        if (!TryComp<FoldableComponent>(uid, out var foldable))
            return;
        if (args.IsInDetailsRange && foldable.IsFolded)
            args.PushMarkup($"{component.Name}");
    }
    private void CreateDeck(EntityUid user, EntityUid target, CardComponent? component = null)
    {
        var stackComponent = new CardStackComponent();
        var usedEntity = _handsSystem.GetActiveItem(user);

        if (usedEntity == target)
            return;
        if (usedEntity == null)
            return;

        if (_net.IsServer)
        {
            if (!_tagSystem.HasTag(usedEntity.Value, "STCard"))
                return;

            var spawnPos = Transform(user).Coordinates;
            var entityCreated = Spawn("STCardDeck", spawnPos);

            if (!TryComp<CardStackComponent>(entityCreated, out var stackComp))
                return;

            stackComponent = stackComp;
            _stackSystem.AddCard(entityCreated, usedEntity.Value, stackComp);
            _stackSystem.AddCard(entityCreated, target, stackComp);

            _handsSystem.TryPickupAnyHand(user, entityCreated);
        }
        _audio.PlayPredicted(stackComponent.AddCard, Transform(user).Coordinates, user);
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
        var fanComponent = new CardFanComponent();
        var usedEntity = _handsSystem.GetActiveItem(user);

        if (usedEntity == target)
            return;
        if (usedEntity == null)
            return;
        if (!_tagSystem.HasTag(usedEntity.Value, "STCard"))
            return;

        var spawnPos = Transform(user).Coordinates;

        if (_net.IsServer)
        {
            var entityCreated = Spawn("STCardFan", spawnPos);

            if (!TryComp<CardStackComponent>(entityCreated, out var stackComp))
                return;
            if (!TryComp<CardFanComponent>(entityCreated, out var fanComp))
                return;

            fanComponent = fanComp;
            _stackSystem.AddCard(entityCreated, usedEntity.Value, stackComp);
            _stackSystem.AddCard(entityCreated, target, stackComp);
            _handsSystem.TryPickupAnyHand(user, entityCreated);

            Dirty(entityCreated, stackComp);
        }
        _audio.PlayPredicted(fanComponent.AddCard, Transform(user).Coordinates, user);
    }
    private void OnCardSelected(EntityUid uid, CardFanComponent component, CardSelectedMessage message)
    {
        if (!TryComp<CardStackComponent>(uid, out var stackComp))
            return;
        if (!TryGetEntity(message.CardEntity, out var cardEntity))
            return;
        if (!TryGetEntity(message.User, out var user))
            return;

        _stackSystem.RemoveCard(uid, cardEntity.Value, stackComp);
        _handsSystem.TryPickupAnyHand(user.Value, cardEntity.Value);
        _appearance.SetData(uid, CardFanStackVisuals.CardsCount, stackComp.Cards.Count);
    }
}
