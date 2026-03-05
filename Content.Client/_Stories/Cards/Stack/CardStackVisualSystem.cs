using System.Linq;
using System.Numerics;
using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Foldable;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client._Stories.Cards.Stack;

public sealed class CardStackVisualSystem : VisualizerSystem<CardStackComponent>
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    protected override void OnAppearanceChange(EntityUid uid, CardStackComponent comp, ref AppearanceChangeEvent args)
    {
        UpdateVisuals(uid);
    }

    private void OnAppearanceChanged(EntityUid uid, CardComponent comp, AppearanceChangeEvent args)
    {
        if (!_containerSystem.TryGetContainingContainer(uid, out var container))
            return;
        UpdateVisuals(container.Owner);
    }

    public void UpdateVisuals(EntityUid uid, List<EntityUid>? cards = null)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        _spriteSystem.LayerSetVisible(uid, 0, false);

        if (TryComp<CardDeckComponent>(uid, out var cardDeck))
            UpdateDeckStackVisuals(uid, cardDeck, sprite, cards);
        else if (TryComp<CardFanComponent>(uid, out var fanComp))
            UpdateFanStackVisuals(uid, fanComp, sprite, cards);
    }


    private void UpdateDeckStackVisuals(EntityUid uid,
        CardDeckComponent comp,
        SpriteComponent sprite,
        List<EntityUid>? cards = null)
    {
        if (!TryComp<CardStackComponent>(uid, out var cardStack))
            return;
        while (sprite.AllLayers.Count() > 1)
        {
            _spriteSystem.RemoveLayer(uid, 1);
        }

        var cardsList = cards is { Count: > 0 } ? cards : cardStack.CardContainer.ContainedEntities;

        var layerIndex = 1;
        foreach (var card in cardsList)
        {
            if (!TryComp<FoldableComponent>(card, out var foldable))
                continue;

            var cardLayer = foldable.IsFolded
                ? _spriteSystem.LayerGetRsiState(card, 1)
                : _spriteSystem.LayerGetRsiState(card, 0);
            var layer = _spriteSystem.AddBlankLayer((uid, sprite), layerIndex);
            _spriteSystem.LayerSetRsiState((uid, sprite), layerIndex, cardLayer);

            var offsetMultiplier = Math.Min(layerIndex - 1, comp.MaxCards - 1);

            _spriteSystem.LayerSetOffset(layer, new Vector2(0, comp.Offset * offsetMultiplier));
            _spriteSystem.LayerSetRotation(layer, Angle.FromDegrees(90));
            _spriteSystem.LayerSetVisible(layer, true);
            layerIndex++;
        }
    }

    private void UpdateFanStackVisuals(EntityUid uid,
        CardFanComponent comp,
        SpriteComponent sprite,
        List<EntityUid>? cards = null)
    {
        if (!TryComp<CardStackComponent>(uid, out var cardStack))
            return;
        while (sprite.AllLayers.Count() > 1)
        {
            _spriteSystem.RemoveLayer(uid, 1);
        }

        var totalCards = Math.Min(comp.MaxCards, cardStack.CardContainer.ContainedEntities.Count);
        var cardsList = cards is { Count: > 0 } ? cards : cardStack.CardContainer.ContainedEntities.Take(totalCards);

        var layerIndex = 1;
        foreach (var card in cardsList)
        {
            if (!TryComp<FoldableComponent>(card, out var foldable))
                continue;

            var cardLayer = foldable.IsFolded
                ? _spriteSystem.LayerGetRsiState(card, 1)
                : _spriteSystem.LayerGetRsiState(card, 0);
            var layer = _spriteSystem.AddBlankLayer((uid, sprite), layerIndex);
            _spriteSystem.LayerSetRsiState((uid, sprite), layerIndex, cardLayer);

            var cardIndex = layerIndex - 1;
            float totalProgress;
            if (totalCards <= 1)
                totalProgress = 0.5f;
            else
                totalProgress = (float)cardIndex / (totalCards - 1);

            var curAngle = MathHelper.Lerp(comp.StartAngle, comp.EndAngle, totalProgress);
            var normX = comp.Radius * MathF.Sin(curAngle * MathF.PI / 180);
            var normY = comp.Radius * MathF.Cos(curAngle * MathF.PI / 180);

            _spriteSystem.LayerSetRotation(layer, Angle.FromDegrees(curAngle + 180));
            _spriteSystem.LayerSetOffset(layer, new Vector2(normX, normY));
            _spriteSystem.LayerSetScale(layer, new Vector2(1.0f, 1.0f));
            _spriteSystem.LayerSetVisible(layer, true);
            layerIndex++;
        }
    }
}
