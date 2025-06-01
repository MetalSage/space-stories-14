using Robust.Shared.Random;
using Robust.Shared.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Map;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Foldable;
using System.Linq;

using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Card;

namespace Content.Shared._Stories.Cards.Stack;
public abstract class SharedCardStackSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private FoldableSystem _foldableSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CardStackComponent, ActivateInWorldEvent>(OnActivateInWorldEvent);
    }
    private void OnComponentInit(EntityUid uid, CardStackComponent component, ComponentInit args)
    {
        component.CardContainer = _containerSystem.EnsureContainer<Container>(uid, "card-stack-container");
    }
    private void OnActivateInWorldEvent(EntityUid uid, CardStackComponent comp, ActivateInWorldEvent args)
    {
        var card = comp.CardContainer.ContainedEntities.Last();
        RemoveCard(uid, card, comp);
        _handsSystem.TryPickupAnyHand(args.User, card);
        _audio.PlayPredicted(comp.RemoveCard, Transform(uid).Coordinates, args.User);
    }
    protected virtual void CombineDecks(EntityUid uid, EntityUid target, CardStackComponent component) { }
    protected virtual void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent? comp = null) { }
    protected void AddCard(EntityUid uid, EntityUid card, CardStackComponent? comp = null)
    {
        var maxCards = 216; // fix hardcode
        if (!Resolve(uid, ref comp)
            || comp.CardContainer.ContainedEntities.Count > maxCards
            || !TryComp(card, out CardComponent? _))
            return;
        _containerSystem.Insert(card, comp.CardContainer);

        Dirty(uid, comp);
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.CardContainer.ContainedEntities.Count);
    }
    public void Split(EntityUid uid, CardStackComponent component, EntityUid user)
    {
        if (component.CardContainer.ContainedEntities.Count <= 1)
            return;

        var cardsSplit = component.CardContainer.ContainedEntities.Count / 2;
        var cardsToMove = new List<EntityUid>();

        for (int i = 0; i < cardsSplit; i++)
        {
            var card = component.CardContainer.ContainedEntities.Last();
            cardsToMove.Add(card);
            RemoveCard(uid, card, component);
            _transformSystem.SetCoordinates(card, EntityCoordinates.Invalid);

            _appearance.SetData(uid, CardStackVisuals.CardsCount, component.CardContainer.ContainedEntities.Count);
        }

        if (_net.IsClient)
            return;

        var spawnPos = Transform(user).Coordinates;
        var entityCreated = new EntityUid();
        if (TryComp<CardDeckComponent>(uid, out var cardDeckComp))
            entityCreated = Spawn("STCardDeck", spawnPos);
        else if (TryComp<CardFanComponent>(uid, out var cardStackComp))
            entityCreated = Spawn("STCardFan", spawnPos);

        if (TryComp<CardStackComponent>(entityCreated, out var stackComponent))
        {
            foreach (var card in cardsToMove)
            {
                AddCard(entityCreated, card, stackComponent);
            }
            _popup.PopupEntity(Loc.GetString("card-split-take", ("cardsSplit", cardsSplit)), user);
            _handsSystem.TryPickup(user, entityCreated);
            if (_net.IsClient)
                _audio.PlayPredicted(component.AddCard, Transform(uid).Coordinates, user);
        }
        Dirty(uid, component);
    }
}
