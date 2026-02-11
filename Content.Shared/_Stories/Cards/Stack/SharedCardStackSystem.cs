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
namespace Content.Shared._Stories.Cards.Stack;

public abstract class SharedCardStackSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly FoldableSystem _foldableSystem = default!;

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
                    _audio.PlayPredicted(fanComp.ShuffleSound, uid, args.User);
                else if (TryComp<CardDeckComponent>(uid, out var deckComp))
                    _audio.PlayPredicted(deckComp.ShuffleSound, uid, args.User);
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
                }
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
                }
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
                }
            },
            Category = VerbCategory.Flip,
        });
    }

    protected virtual void ShuffleCards(EntityUid uid, CardStackComponent component) {}
    protected virtual void Split(EntityUid uid, CardStackComponent component, EntityUid user) {}

    private void OnInteractUsing(EntityUid uid, CardStackComponent comp, InteractUsingEvent args)
    {
        if (args.Handled ||
            comp.CardContainer.ContainedEntities.Count > comp.MaxCards)
            return;

        if (HasComp<CardComponent>(args.Used))
        {
            if (_containerSystem.Insert(args.Used, comp.CardContainer))
            {
                var sound = TryComp<CardFanComponent>(uid, out var fanComp) ? fanComp.AddCardSound : comp.AddCardSound;
                _audio.PlayPredicted(sound, uid, args.User);
            }
        }
        else if (HasComp<CardStackComponent>(args.Used))
        {
            if (HasComp<CardDeckComponent>(args.Used) && HasComp<CardFanComponent>(args.Target))
                return;
            CombineStacks(uid, args.Used);
            _audio.PlayPredicted(comp.AddCardSound, uid, args.User);
        }

        _appearance.SetData(uid, CardStackVisual.State, comp.CardContainer.ContainedEntities.Count);
        args.Handled = true;
    }

    private void OnActivateInWorldEvent(EntityUid uid, CardStackComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        var card = comp.CardContainer.ContainedEntities.Last();

        RemoveCard(uid, card, comp);
        _handsSystem.TryPickupAnyHand(args.User, card);

        _appearance.SetData(uid, CardStackVisual.State, comp.CardContainer.ContainedEntities.Count);
        _audio.PlayPredicted(comp.RemoveCardSound, uid, args.User);

        args.Handled = true;
    }

    public void RemoveCard(EntityUid uid, EntityUid card, CardStackComponent comp)
    {
        _containerSystem.Remove(card, comp.CardContainer);

        if (comp.CardContainer.ContainedEntities.Count == 0)
            PredictedQueueDel(uid);
    }

    private void CombineStacks(EntityUid uid, EntityUid used)
    {
        if (!TryComp<CardStackComponent>(uid, out var stackComp) ||
            !TryComp<CardStackComponent>(used, out var usedStack))
            return;

        foreach (var card in usedStack.CardContainer.ContainedEntities.ToList())
        {
            _containerSystem.Insert(card, stackComp.CardContainer);
        }

        PredictedQueueDel(used);
    }

    protected void CardStackRebuild(List<EntityUid> list, CardStackComponent component)
    {
        foreach (var card in list)
        {
            _containerSystem.Remove(card, component.CardContainer, force: true);
        }
        foreach (var card in list)
        {
            _containerSystem.Insert(card, component.CardContainer, force: true);
        }
    }
}
