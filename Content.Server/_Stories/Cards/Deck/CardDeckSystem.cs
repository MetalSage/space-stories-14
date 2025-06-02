using Robust.Shared.Map;
using System.Linq;

using Content.Shared._Stories.Cards.Stack;

namespace Content.Server._Stories.Cards.Deck;

public sealed class CardDeckSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
    private void CombineDecks(EntityUid uid, EntityUid target, CardStackComponent component)
    {
        if (!TryComp<CardStackComponent>(target, out var targetStack))
            return;

        var cardsToMove = component.CardContainer.ContainedEntities;

        foreach (var card in cardsToMove)
        {
            // RemoveCard(uid, card, component);
            _transformSystem.SetCoordinates(card, EntityCoordinates.Invalid);

            // AddCard(target, card, targetStack);
        }
    }
}
