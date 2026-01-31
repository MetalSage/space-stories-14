using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Examine;
using Content.Shared.Foldable;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._Stories.Cards.Card;

public sealed class SharedCardSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedCardStackSystem _cardStack = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardFanComponent, CardSelectedMessage>(OnCardSelected);
        SubscribeLocalEvent<CardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, CardComponent component, ExaminedEvent args)
    {
        if (!TryComp<FoldableComponent>(uid, out var foldable)
            || !foldable.IsFolded || !args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("card-name", ("cardName", component.Name)));
    }

    private void OnCardSelected(EntityUid uid, CardFanComponent component, CardSelectedMessage message)
    {
        if (!TryComp<CardStackComponent>(uid, out var stackComp)
            || !TryGetEntity(message.CardEntity, out var cardEntity)
            || !TryGetEntity(message.User, out var user))
            return;
        _cardStack.RemoveCard(uid, cardEntity.Value, stackComp);
        _handsSystem.TryPickupAnyHand(user.Value, cardEntity.Value);

        Dirty(uid, stackComp);
    }
}
