using System.Linq;
using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared.Examine;
using Content.Shared.Foldable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Shared._Stories.Cards.Stack;

public abstract class SharedCardStackSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly FoldableSystem _foldableSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CardStackComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
        SubscribeLocalEvent<CardStackComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CardStackComponent, ActivateInWorldEvent>(OnActivateInWorldEvent);
        SubscribeLocalEvent<CardStackComponent, ExaminedEvent>(OnStackExamined);
    }

    private void OnComponentInit(EntityUid uid, CardStackComponent component, ComponentInit args)
    {
        component.CardContainer = _containerSystem.EnsureContainer<Container>(uid, "card-stack-container");
    }

    private void OnStackExamined(EntityUid uid, CardStackComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;
        args.PushMarkup(Loc.GetString("card-count-total",
            ("cardsTotal", component.CardContainer.ContainedEntities.Count)));
    }

    private void OnGetAlternativeVerb(EntityUid uid, CardStackComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("card-shuffle"),
            Act = () =>
            {
                ShuffleCards(uid, component);
                _popup.PopupClient(Loc.GetString("card-shuffle-success"), args.User);
                if (TryComp<CardFanComponent>(uid, out var fanComp))
                    _audio.PlayLocal(fanComp.ShuffleSound, uid, args.User);
                else if (TryComp<CardDeckComponent>(uid, out var deckComp))
                    _audio.PlayLocal(deckComp.ShuffleSound, uid, args.User);
            },
            Priority = 2,
        });
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("card-split"),
            Act = () => Split(uid, component, args.User),
            Priority = 1,
        });
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("card-flip-all"),
            Act = () =>
            {
                foreach (var card in component.CardContainer.ContainedEntities)
                {
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.TryToggleFold(card, foldable);
                    SetFoldVisuals(uid, foldable);
                }

                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
        });
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("card-flip-face"),
            Act = () =>
            {
                foreach (var card in component.CardContainer.ContainedEntities)
                {
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.SetFolded(card, foldable, true);
                    SetFoldVisuals(card, foldable);
                }

                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
            Category = VerbCategory.Flip,
        });
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("card-flip-back"),
            Act = () =>
            {
                foreach (var card in component.CardContainer.ContainedEntities)
                {
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.SetFolded(card, foldable, false);
                    SetFoldVisuals(card, foldable);
                }

                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
            Category = VerbCategory.Flip,
        });
    }

    private void OnInteractUsing(EntityUid uid, CardStackComponent comp, InteractUsingEvent args)
    {
        if (TryComp<CardComponent>(args.Used, out _))
        {
            if (comp.CardContainer.ContainedEntities.Count > comp.MaxCards
                || !TryComp(args.Used, out CardComponent? _))
                return;
            _containerSystem.Insert(args.Used, comp.CardContainer);

            if (TryComp<CardFanComponent>(uid, out var fanComp))
                _audio.PlayLocal(fanComp.AddCardSound, uid, args.User);
            else
                _audio.PlayLocal(comp.AddCardSound, uid, args.User);
        }
        else if (TryComp<CardStackComponent>(args.Used, out _))
        {
            if (TryComp<CardDeckComponent>(args.Used, out _) && TryComp<CardFanComponent>(args.Target, out _))
                return;
            CombineStacks(uid, args.Used);
            _audio.PlayLocal(comp.AddCardSound, uid, args.User);
        }

        var list = comp.CardContainer.ContainedEntities.ToList();
        SetActualCardsOrder(uid, comp, list);
    }

    private void OnActivateInWorldEvent(EntityUid uid, CardStackComponent comp, ActivateInWorldEvent args)
    {
        var card = comp.CardContainer.ContainedEntities.ToList().Last();
        RemoveCard(uid, card, comp);
        _handsSystem.TryPickupAnyHand(args.User, card);

        _audio.PlayLocal(comp.RemoveCardSound, uid, args.User);
    }

    public void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent comp)
    {
        _containerSystem.Remove(card, comp.CardContainer);

        if (comp.CardContainer.ContainedEntities.Count == 0)
            PredictedDel(uid);

        var list = comp.CardContainer.ContainedEntities.ToList();
        SetActualCardsOrder(uid, comp, list);
    }

    protected virtual void ShuffleCards(EntityUid uid, CardStackComponent component) { }

    private void Split(EntityUid uid, CardStackComponent component, EntityUid user)
    {
        if (component.CardContainer.ContainedEntities.Count <= 1)
            return;

        var splitCount = component.CardContainer.ContainedEntities.Count / 2;
        var cardsToMove = new List<EntityUid>();
        for (var i = 0; i < splitCount; i++)
        {
            var card = component.CardContainer.ContainedEntities.Last();
            cardsToMove.Add(card);
            RemoveCard(uid, card, component);
            _transformSystem.SetCoordinates(card, EntityCoordinates.Invalid);
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

            _popup.PopupClient(Loc.GetString("card-split-take", ("cardsSplit", splitCount)), user);
            _handsSystem.TryPickup(user, entityCreated);
            _audio.PlayLocal(component.AddCardSound, uid, user);
        }

        Dirty(uid, component);
    }

    public void SetActualCardsOrder(EntityUid uid, CardStackComponent comp, List<EntityUid> list)
    {
        comp.CardOrder.Clear();
        comp.CardOrder.AddRange(list);

        foreach (var card in comp.CardContainer.ContainedEntities.ToList())
        {
            _containerSystem.Remove(card, comp.CardContainer);
        }

        foreach (var card in list)
        {
            _containerSystem.Insert(card, comp.CardContainer);
        }

        _appearance.SetData(uid, CardStackVisuals.OrderEdited, true);
    }

    private void CombineStacks(EntityUid uid, EntityUid used)
    {
        if (!TryComp<CardStackComponent>(uid, out var stackComp)
            || !TryComp<CardStackComponent>(used, out var usedStack))
            return;

        foreach (var card in usedStack.CardContainer.ContainedEntities.ToList())
        {
            _containerSystem.Insert(card, stackComp.CardContainer);
        }

        PredictedDel(used);

        _appearance.SetData(uid, CardStackVisuals.CardsCount, stackComp.CardContainer.ContainedEntities.Count);
    }

    private void SetFoldVisuals(EntityUid uid, FoldableComponent foldable)
    {
        var folded = foldable.IsFolded;
        _appearance.SetData(uid, FoldableSystem.FoldedVisuals.State, folded);
    }
}
