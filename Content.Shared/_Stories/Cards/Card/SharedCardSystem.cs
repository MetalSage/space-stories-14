using Robust.Shared.Network;
using Robust.Shared.Audio.Systems;
using Content.Shared.Tag;
using Content.Shared.Interaction;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Foldable;

using Content.Shared._Stories.Cards.Stack;
using Content.Shared._Stories.Cards.Fan;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.Shared._Stories.Cards.Card;
public abstract class SharedCardSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
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
    protected virtual void CreateDeck(EntityUid user, EntityUid target, CardComponent? component = null) { }
    private void OnActivateInWorld(EntityUid uid, CardComponent comp, ActivateInWorldEvent args)
    {
        CreateDeck(args.User, args.Target, comp);
    }
    private void OnInteractUsing(EntityUid uid, CardComponent comp, InteractUsingEvent args)
    {
        CreateFan(args.User, args.Target, comp);
    }

    protected virtual void CreateFan(EntityUid user, EntityUid target, CardComponent? component = null) { }
    private void OnCardSelected(EntityUid uid, CardFanComponent component, CardSelectedMessage message)
    {
        if (!TryComp<CardStackComponent>(uid, out var stackComp)
            || !TryGetEntity(message.CardEntity, out var cardEntity)
            || !TryGetEntity(message.User, out var user))
            return;

        RaiseLocalEvent(uid, new CardRemovedEvent(user.Value, cardEntity.Value));
        _appearance.SetData(uid, CardFanStackVisuals.CardsCount, stackComp.CardContainer.ContainedEntities.Count);
    }
}
