using Content.Shared._Stories.Cards.Stack;
using Robust.Shared.Timing;

namespace Content.Client._Stories.Cards.Stack;

public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly CardStackVisualSystem _cardStackVisual = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CardStackShuffledEvent>(OnCardStackShuffledEvent);
    }

    private void OnCardStackShuffledEvent(CardStackShuffledEvent ev)
    {
        var uid = GetEntity(ev.Entity);

        if (ev.Cards == null || !TryComp<CardStackComponent>(uid, out var stackComp))
            return;
        var cards = GetEntityList(ev.Cards);
        _cardStackVisual.UpdateVisuals(uid, cards);
        CardStackRebuild(cards, stackComp);
    }
}
