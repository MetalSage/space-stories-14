using Robust.Shared.Map;
using System.Linq;

using Content.Shared._Stories.Cards.Stack;

namespace Content.Shared._Stories.Cards.Deck;

public sealed class CardDeckSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly CardStackSystem _stackSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    public void CombineDecks(EntityUid uid, EntityUid target, CardStackComponent component)
    {
        if (!TryComp<CardStackComponent>(target, out var targetStack))
            return;

        var cardsToMove = component.Cards.ToList();

        foreach (var card in cardsToMove)
        {
            _stackSystem.RemoveCard(uid, card, component);
            _transformSystem.SetCoordinates(card, EntityCoordinates.Invalid);

            _stackSystem.AddCard(target, card, targetStack);
        }
    }
}
