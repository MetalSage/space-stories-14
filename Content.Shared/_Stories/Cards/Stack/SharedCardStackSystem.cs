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
        SubscribeLocalEvent<CardStackComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerb);
        SubscribeLocalEvent<CardStackComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CardStackComponent, ActivateInWorldEvent>(OnActivateInWorldEvent);
        SubscribeLocalEvent<CardStackComponent, ExaminedEvent>(OnStackExamined);
    }
    private void OnStackExamined(EntityUid uid, CardStackComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;
        args.PushMarkup(Loc.GetString("card-count-total", ("cardsTotal", component.CardContainer.ContainedEntities.Count)));
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
                    _audio.PlayPredicted(fanComp.ShuffleSound, Transform(uid).Coordinates, args.User);
                else if (TryComp<CardDeckComponent>(uid, out var deckComp))
                    _audio.PlayPredicted(deckComp.ShuffleSound, Transform(uid).Coordinates, args.User);
            },
            Priority = 2
        });
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("card-split"),
            Act = () => Split(uid, component, args.User),
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
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.SetFolded(card, foldable, true);
                    var folded = foldable.IsFolded;
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
                    if (!TryComp<FoldableComponent>(card, out var foldable))
                        return;
                    _foldableSystem.SetFolded(card, foldable, false);
                    var folded = foldable.IsFolded;
                    _appearance.SetData(uid, CardVisuals.State, folded);
                }
                _popup.PopupClient(Loc.GetString("card-flip-success"), args.User);
            },
            Category = VerbCategory.Flip
        });
    }
    private void OnComponentInit(EntityUid uid, CardStackComponent component, ComponentInit args)
    {
        component.CardContainer = _containerSystem.EnsureContainer<Container>(uid, "card-stack-container");
    }
    private void OnInteractUsing(EntityUid uid, CardStackComponent comp, InteractUsingEvent args)
    {
        if (TryComp<CardComponent>(args.Used, out _))
        {
            var maxCards = 216; // fix hardcode
            if (comp.CardContainer.ContainedEntities.Count > maxCards
                || !TryComp(args.Used, out CardComponent? _))
                return;
            _containerSystem.Insert(args.Used, comp.CardContainer);
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
        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.CardContainer.ContainedEntities.Count);
    }
    private void OnActivateInWorldEvent(EntityUid uid, CardStackComponent comp, ActivateInWorldEvent args)
    {
        _audio.PlayPredicted(comp.RemoveCard, Transform(uid).Coordinates, args.User);

        var card = comp.CardContainer.ContainedEntities.Last();
        RemoveCard(uid, card, comp);
        _handsSystem.TryPickupAnyHand(args.User, card);

        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.CardContainer.ContainedEntities.Count);
    }
    protected void CombineDecks(EntityUid uid, EntityUid target, CardStackComponent component) { }
    protected virtual void ShuffleCards(EntityUid uid, CardStackComponent component) { }
    protected virtual void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent comp) { }
    protected virtual void AddCard(EntityUid uid, EntityUid card, CardStackComponent comp) { }
    protected virtual void Split(EntityUid uid, CardStackComponent component, EntityUid user) { }
}
