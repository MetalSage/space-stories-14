using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared._Stories.Cards.Stack;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._Stories.Cards.Stack;

public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

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
    }

    protected override void ShuffleCards(EntityUid uid, CardStackComponent component)
    {
        var list = component.CardContainer.ContainedEntities.ToList();
        _robustRandom.Shuffle(list);

        CardStackRebuild(list, component);

        var ev = new CardStackShuffledEvent(GetNetEntity(uid), GetNetEntityList(list));
        RaiseNetworkEvent(ev);
        Dirty(uid, component);
    }

    protected override void Split(EntityUid uid, CardStackComponent component, EntityUid user)
    {
        if (component.CardContainer.ContainedEntities.Count <= 1)
            return;

        var allCards = component.CardContainer.ContainedEntities;
        var splitCount = allCards.Count / 2;
        var cardsToMove = allCards.TakeLast(splitCount).ToList();
        foreach (var card in cardsToMove)
        {
            RemoveCard(uid, card, component);
            _transform.SetCoordinates(card, EntityCoordinates.Invalid);
        }

        _appearance.SetData(uid, CardStackVisual.State, component.CardContainer.ContainedEntities.Count);

        var spawnPos = Transform(user).Coordinates;
        var protoId = MetaData(uid).EntityPrototype?.ID;
        if (protoId == null)
            return;
        var entityCreated = Spawn(protoId, spawnPos);

        if (TryComp<CardStackComponent>(entityCreated, out var stackComponent))
        {
            foreach (var card in cardsToMove)
            {
                _containerSystem.Insert(card, stackComponent.CardContainer);
            }

            _handsSystem.TryPickup(user, entityCreated);
            _popup.PopupEntity(Loc.GetString("card-split-take", ("cardsSplit", splitCount)), uid, user);
            _audio.PlayPvs(component.AddCardSound, uid);
        }

        Dirty(uid, component);
    }
}
