using System.Linq;
using System.Numerics;
using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Foldable;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Containers;

namespace Content.Client._Stories.Cards.Stack;

public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(EntityUid uid, CardStackComponent comp, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        sprite.LayerSetVisible(0, false);

        if (_appearance.TryGetData<bool>(uid, CardStackVisuals.OrderEdited, out var shuffled) && shuffled)
        {
            foreach (var card in comp.CardContainer.ContainedEntities.ToList())
            {
                _containerSystem.Remove(card, comp.CardContainer);
            }

            foreach (var card in comp.CardOrder)
            {
                _containerSystem.Insert(card, comp.CardContainer);
            }

            _appearance.SetData(uid, CardStackVisuals.OrderEdited, false);
        }

        if (TryComp<CardDeckComponent>(uid, out var deckComp))
            UpdateDeckStackVisuals(uid, deckComp, sprite);
        else if (TryComp<CardFanComponent>(uid, out var fanComp))
            UpdateFanStackVisuals(uid, fanComp, sprite);
    }

    private void UpdateDeckStackVisuals(EntityUid uid, CardDeckComponent comp, SpriteComponent sprite)
    {
        if (!TryComp<CardStackComponent>(uid, out var cardStack))
            return;
        while (sprite.AllLayers.Count() > 1)
        {
            sprite.RemoveLayer(1);
        }

        var processedLayers = new HashSet<RSI.StateId>();
        var layerIndex = 1;

        foreach (var card in cardStack.CardContainer.ContainedEntities)
        {
            if (!TryComp<SpriteComponent>(card, out var cardSprite) ||
                !TryComp<FoldableComponent>(card, out var foldable))
                return;

            var cardLayer = foldable.IsFolded ? cardSprite.LayerGetState(1) : cardSprite.LayerGetState(0);
            processedLayers.Add(cardLayer);
            var layer = sprite.AddLayer(cardLayer);

            var offsetMultiplier = Math.Min(layerIndex - 1, comp.MaxCards - 1);

            sprite.LayerSetOffset(layer, new Vector2(0, comp.Offset * offsetMultiplier));
            sprite.LayerSetRotation(layer, Angle.FromDegrees(90));
            sprite.LayerSetVisible(layer, true);
            layerIndex++;
        }
    }

    private void UpdateFanStackVisuals(EntityUid uid, CardFanComponent comp, SpriteComponent sprite)
    {
        if (!TryComp<CardStackComponent>(uid, out var cardStack))
            return;
        while (sprite.AllLayers.Count() > 1)
        {
            sprite.RemoveLayer(1);
        }

        var processedLayers = new HashSet<RSI.StateId>();
        var layerIndex = 1;

        var totalCards = Math.Min(comp.MaxCards, cardStack.CardContainer.ContainedEntities.Count);

        foreach (var card in cardStack.CardContainer.ContainedEntities.Take(totalCards))
        {
            if (!TryComp<SpriteComponent>(card, out var cardSprite) ||
                !TryComp<FoldableComponent>(card, out var foldable))
                return;

            var cardLayer = foldable.IsFolded ? cardSprite.LayerGetState(1) : cardSprite.LayerGetState(0);
            processedLayers.Add(cardLayer);
            var layer = sprite.AddLayer(cardLayer);

            var cardIndex = layerIndex - 1;
            var totalProgress = (float)cardIndex / (totalCards - 1);

            var curAngle = MathHelper.Lerp(comp.StartAngle, comp.EndAngle, totalProgress);
            if (totalCards == 1)
                curAngle = (comp.StartAngle + comp.EndAngle) / 2;
            else
                curAngle = MathHelper.Lerp(comp.StartAngle, comp.EndAngle, totalProgress);

            var normX = comp.Radius * MathF.Sin(curAngle * MathF.PI / 180);
            var normY = comp.Radius * MathF.Cos(curAngle * MathF.PI / 180);

            if (TryComp<CardFanComponent>(uid, out _))
            {
                sprite.LayerSetRotation(layer, Angle.FromDegrees(curAngle + 180));
                sprite.LayerSetOffset(layer, new Vector2(normX, normY));
                sprite.LayerSetScale(layer, new Vector2(1.0f, 1.0f));
                sprite.LayerSetVisible(layer, true);
                layerIndex++;
            }
        }
    }
}
