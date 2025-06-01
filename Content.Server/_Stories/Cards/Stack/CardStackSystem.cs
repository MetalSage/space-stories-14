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
        SubscribeLocalEvent<CardStackComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CardStackComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
        SubscribeLocalEvent<CardStackComponent, CardAddedEvent>(OnCardAdded);
        SubscribeLocalEvent<CardStackComponent, CardRemovedEvent>(OnCardRemoved);
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
                foreach (var card in component.CardContainer.ContainedEntities)
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
                foreach (var card in component.CardContainer.ContainedEntities)
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
                foreach (var card in component.CardContainer.ContainedEntities)
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
    public void ShuffleCards(EntityUid uid, CardStackComponent component)
    {
        _robustRandom.Shuffle(component.CardContainer.ContainedEntities.ToList());
        _appearance.SetData(uid, CardStackVisuals.Shuffled, true);
        Dirty(uid, component);
    }
    private void OnInteractUsing(EntityUid uid, CardStackComponent comp, InteractUsingEvent args)
    {
        if (!TryComp<CardStackComponent>(args.Target, out var targetStack))
            return;

        if (TryComp<CardComponent>(args.Used, out var _))
        {
            AddCard(args.Target, args.Used, targetStack);
            if (TryComp<CardFanComponent>(uid, out var fanComp))
                _audio.PlayPredicted(fanComp.AddCard, Transform(uid).Coordinates, args.User);
            else
                _audio.PlayPredicted(comp.AddCard, Transform(uid).Coordinates, args.User);
        }
        else if (TryComp<CardStackComponent>(args.Used, out var uidStack))
        {
            CombineDecks(args.Used, args.Target, uidStack);
            _audio.PlayPredicted(comp.AddCard, Transform(uid).Coordinates, args.User);
        }
    }
    private void OnCardAdded(EntityUid uid, CardStackComponent comp, CardAddedEvent args)
    {
        AddCard(uid, args.Card, comp);
    }
    private void OnCardRemoved(EntityUid uid, CardStackComponent comp, CardRemovedEvent args)
    {
        RemoveCard(uid, args.Card, comp);
    }
    protected override void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent? comp = null)
    {
        if (!Resolve(uid, ref comp) || !TryComp(card, out CardComponent? _))
            return;

        _containerSystem.Remove(card, comp.CardContainer);
        // if (user != null)
        //     _handsSystem.TryPickupAnyHand(user.Value, card);
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.CardContainer.ContainedEntities.Count);

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
