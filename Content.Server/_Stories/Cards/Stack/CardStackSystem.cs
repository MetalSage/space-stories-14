using System.Linq;
using Content.Server._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Foldable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Server._Stories.Cards.Stack;

public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private FoldableSystem _foldableSystem = default!;
    [Dependency] private readonly CardDeckSystem _deckSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CardStackComponent, CardAddedEvent>(OnCardAdded);
        SubscribeLocalEvent<CardStackComponent, CardRemovedEvent>(OnCardRemoved);
    }
    protected override void Split(EntityUid uid, CardStackComponent component, EntityUid user)
    {
        if (component.CardContainer.ContainedEntities.Count <= 1)
            return;

        var splitCount = component.CardContainer.ContainedEntities.Count / 2;
        var cardsToMove = new List<EntityUid>();
        for (int i = 0; i < splitCount; i++)
        {
            var card = component.CardContainer.ContainedEntities.Last();
            cardsToMove.Add(card);
            RemoveCard(uid, card, component);
            _transformSystem.SetCoordinates(card, EntityCoordinates.Invalid);

            _appearance.SetData(uid, CardStackVisuals.CardsCount, component.CardContainer.ContainedEntities.Count);
        }

        var spawnPos = Transform(user).Coordinates;
        var entityCreated = new EntityUid();
        if (TryComp<CardDeckComponent>(uid, out _))
            entityCreated = Spawn("STCardDeck", spawnPos);
        else if (TryComp<CardFanComponent>(uid, out _))
            entityCreated = Spawn("STCardFan", spawnPos);

        if (TryComp<CardStackComponent>(entityCreated, out var stackComponent))
        {
            foreach (var card in cardsToMove)
            {
                _containerSystem.Insert(card, stackComponent.CardContainer);
            }

            _popup.PopupEntity(Loc.GetString("card-split-take", ("cardsSplit", splitCount)), user);
            _handsSystem.TryPickup(user, entityCreated);
            _audio.PlayPvs(component.AddCard, Transform(uid).Coordinates);
        }
        Dirty(uid, component);
    }
    protected override void ShuffleCards(EntityUid uid, CardStackComponent component)
    {
        var listToShuffle = component.CardContainer.ContainedEntities.ToList();
        _robustRandom.Shuffle(listToShuffle);
        foreach (var card in listToShuffle)
        {
            _containerSystem.Remove(card, component.CardContainer);
        }

        foreach (var card in listToShuffle)
        {
            _containerSystem.Insert(card, component.CardContainer);
        }
        Dirty(uid, component);
        _appearance.SetData(uid, CardStackVisuals.Shuffled, true);
    }
    private void OnCardAdded(EntityUid uid, CardStackComponent comp, CardAddedEvent args)
    {
        AddCard(uid, args.Card, comp);
    }
    private void OnCardRemoved(EntityUid uid, CardStackComponent comp, CardRemovedEvent args)
    {
        RemoveCard(uid, args.Card, comp);
    }
    protected override void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent comp)
    {
        if (!TryComp(card, out CardComponent? _))
            return;
        _containerSystem.Remove(card, comp.CardContainer);

        if (comp.CardContainer.ContainedEntities.Count == 0)
            EntityManager.DeleteEntity(uid);
        Dirty(uid, comp);
    }
    private void OnMapInit(EntityUid uid, CardStackComponent comp, MapInitEvent args)
    {
        var coordinates = Transform(uid).Coordinates;
        foreach (var card in comp.InitialContent)
        {
            var ent = Spawn(card, coordinates);
            AddCard(uid, ent, comp);
        }
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.CardContainer.ContainedEntities.Count);
    }
}
