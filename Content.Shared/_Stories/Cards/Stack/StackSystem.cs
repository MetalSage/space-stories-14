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
public sealed class CardStackSystem : EntitySystem
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
        SubscribeLocalEvent<CardStackComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CardStackComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CardStackComponent, ActivateInWorldEvent>(OnActivateInWorldEvent);
        SubscribeLocalEvent<CardStackComponent, ExaminedEvent>(OnStackExamined);
        SubscribeLocalEvent<CardStackComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
        SubscribeLocalEvent<CardStackComponent, InteractUsingEvent>(OnInteractUsing);
    }
    private void OnComponentInit(EntityUid uid, CardStackComponent component, ComponentInit args)
    {
        component.CardContainer = _containerSystem.EnsureContainer<Container>(uid, "card-stack-container");
    }

    private void OnStackExamined(EntityUid uid, CardStackComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("card-count-total", ("cardsTotal", component.Cards.Count)));
    }
    private void OnMapInit(EntityUid uid, CardStackComponent comp, MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        var coordinates = Transform(uid).Coordinates;
        foreach (var card in comp.InitialContent)
        {
            var ent = Spawn(card, coordinates);
            AddCard(uid, ent, comp);
        }
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.Cards.Count);
    }
    public void AddCard(EntityUid uid, EntityUid card, CardStackComponent? comp = null)
    {
        var maxCards = 216;
        if (!Resolve(uid, ref comp))
            return;
        if (!TryComp(card, out CardComponent? _))
            return;
        if (comp.Cards.Count > maxCards)
            return;

        comp.Cards.Add(card);
        _containerSystem.Insert(card, comp.CardContainer);

        Dirty(uid, comp);
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.Cards.Count);
    }

    public void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;
        if (!TryComp(card, out CardComponent? _))
            return;

        comp.Cards.Remove(card);
        _containerSystem.Remove(card, comp.CardContainer);
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.Cards.Count);

        if (comp.Cards.Count == 0)
        {
            if (_net.IsServer)
                EntityManager.QueueDeleteEntity(uid);
        }
        Dirty(uid, comp);
    }
    private void OnInteractUsing(EntityUid uid, CardStackComponent comp, InteractUsingEvent args)
    {
        if (!TryComp<CardStackComponent>(args.Target, out var targetStack))
            return;

        if (TryComp<CardComponent>(args.Used, out var _))
        {
            AddCard(args.Target, args.Used, targetStack);
            if (_net.IsClient)
            {
                if (TryComp<CardFanComponent>(uid, out var fanComp))
                {
                    _audio.PlayPredicted(fanComp.AddCard, Transform(uid).Coordinates, args.User);
                }
                else
                {
                    _audio.PlayPredicted(comp.AddCard, Transform(uid).Coordinates, args.User);
                }
            }
        }
        else if (TryComp<CardStackComponent>(args.Used, out var uidStack))
        {
            _deckSystem.CombineDecks(args.Used, args.Target, uidStack);
            _audio.PlayPredicted(comp.AddCard, Transform(uid).Coordinates, args.User);
        }
    }
    private void OnActivateInWorldEvent(EntityUid uid, CardStackComponent comp, ActivateInWorldEvent args)
    {
        if (!TryComp<CardStackComponent>(args.Target, out var deckStackComp))
            return;
        if (_handsSystem.GetActiveItem(args.User) != null)
            return;

        var card = comp.Cards.Last();
        RemoveCard(args.Target, card, deckStackComp);
        _handsSystem.TryPickup(args.User, card);
        if (_net.IsClient)
        {
            _audio.PlayPredicted(comp.RemoveCard, Transform(uid).Coordinates, args.User);
        }
    }
    public void ShuffleCards(EntityUid uid, CardStackComponent component)
    {
        if (_net.IsServer)
            _robustRandom.Shuffle(component.Cards);
        _appearance.SetData(uid, CardStackVisuals.Shuffled, true);
        Dirty(uid, component);
    }
    public void Split(EntityUid uid, CardStackComponent component, EntityUid user)
    {
        if (component.Cards.Count <= 1)
            return;

        var cardsSplit = component.Cards.Count / 2;
        var cardsToMove = new List<EntityUid>();

        for (int i = 0; i < cardsSplit; i++)
        {
            var card = component.Cards.Last();
            cardsToMove.Add(card);
            RemoveCard(uid, card, component);
            _transformSystem.SetCoordinates(card, EntityCoordinates.Invalid);

            _appearance.SetData(uid, CardStackVisuals.CardsCount, component.Cards.Count);
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
    private void OnGetAlternativeVerb(EntityUid uid, CardStackComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("card-shuffle"),
            Act = () =>
            {
                ShuffleCards(uid, component);
                _popup.PopupClient(Loc.GetString("card-shuffle-success"), args.User);
                if (TryComp<CardFanComponent>(uid, out var fanComp))
                {
                    _audio.PlayPredicted(fanComp.ShuffleSound, Transform(uid).Coordinates, args.User);
                }
                else if (TryComp<CardDeckComponent>(uid, out var deckComp))
                {
                    _audio.PlayPredicted(deckComp.ShuffleSound, Transform(uid).Coordinates, args.User);
                }
            },
            Priority = 2
        });
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("card-split"),
            Act = () =>
            {
                Split(uid, component, args.User);
            },
            Priority = 1
        });
        args.Verbs.Add(new()
        {
            Text = Loc.GetString("card-flip-all"),
            Act = () =>
            {
                foreach (var card in component.Cards)
                {
                    var folded = false;
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.TryToggleFold(card, foldable);
                    folded = foldable.IsFolded;
                    _appearance.SetData(uid, CardVisuals.State, folded);
                }
                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
        });
        args.Verbs.Add(new()
        {
            Text = Loc.GetString("card-flip-face"),
            Act = () =>
            {
                foreach (var card in component.Cards)
                {
                    var folded = false;
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.SetFolded(card, foldable, true);
                    folded = foldable.IsFolded;
                    _appearance.SetData(uid, CardVisuals.State, folded);
                }
                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
            Category = VerbCategory.Flip
        });
        args.Verbs.Add(new()
        {
            Text = Loc.GetString("card-flip-back"),
            Act = () =>
            {
                foreach (var card in component.Cards)
                {
                    var folded = false;
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.SetFolded(card, foldable, false);
                    folded = foldable.IsFolded;
                    _appearance.SetData(uid, CardVisuals.State, folded);
                }
                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
            Category = VerbCategory.Flip
        });
    }
}
