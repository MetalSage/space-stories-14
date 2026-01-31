using System.Linq;
using Content.Shared._Stories.Cards.Stack;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._Stories.Cards.Stack;

public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, CardStackComponent comp, MapInitEvent args)
    {
        var coordinates = Transform(uid).Coordinates;
        foreach (var card in comp.InitialContent)
        {
            var ent = Spawn(card, coordinates);
            _containerSystem.Insert(ent, comp.CardContainer);
        }

        _appearance.SetData(uid, CardStackVisuals.CardsCount, comp.CardContainer.ContainedEntities.Count);
    }

    protected override void ShuffleCards(EntityUid uid, CardStackComponent component)
    {
        var list = component.CardContainer.ContainedEntities.ToList();
        _robustRandom.Shuffle(list);

        SetActualCardsOrder(uid, component, list);

        Dirty(uid, component);
    }
}
