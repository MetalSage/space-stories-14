using System.Linq;
using System.Numerics;
using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Fan;
using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Foldable;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Client._Stories.Cards.Stack;

public sealed class CardStackVisualSystem : VisualizerSystem<CardStackComponent>
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
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

        if (TryComp<CardStackComponent>(uid, out var stackComp) && stackComp.CardContainer.ContainedEntities.Count > 0)
            UpdateDeckStackVisuals(uid, stackComp, sprite, cards);
        else if (TryComp<CardFanComponent>(uid, out var fanComp))
            UpdateFanStackVisuals(uid, fanComp, sprite);
    }


    private void UpdateDeckStackVisuals(EntityUid uid, CardStackComponent cardStack, SpriteComponent sprite, List<EntityUid>? cards = null)
    {
        Log.Info("sss");
        if (!TryComp<CardDeckComponent>(uid, out var comp))
            return;
        while (sprite.AllLayers.Count() > 1)
        {
            _spriteSystem.RemoveLayer(uid, 1);
        }

        var cardsList = cards is { Count: > 0 } ? cards : cardStack.CardContainer.ContainedEntities;

        var layerIndex = 1;
        foreach (var card in cardsList)
        {
            if (!TryComp<SpriteComponent>(card, out var cardSprite) ||
                !TryComp<FoldableComponent>(card, out var foldable))
                continue;

            var cardLayer = foldable.IsFolded ? _spriteSystem.LayerGetRsiState(card, 1) : _spriteSystem.LayerGetRsiState(card, 0);
            var layer = _spriteSystem.AddBlankLayer((uid, sprite), layerIndex);
            _spriteSystem.LayerSetRsiState((uid, sprite), layerIndex, cardLayer);

            var offsetMultiplier = Math.Min(layerIndex - 1, comp.MaxCards - 1);

            _spriteSystem.LayerSetOffset(layer, new Vector2(0, comp.Offset * offsetMultiplier));
            _spriteSystem.LayerSetRotation(layer, Angle.FromDegrees(90));
            _spriteSystem.LayerSetVisible(layer, true);
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
