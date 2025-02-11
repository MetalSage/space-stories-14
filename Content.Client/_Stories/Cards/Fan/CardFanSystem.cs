using Robust.Client.GameObjects;
using System.Numerics;
using System.Linq;
using Content.Shared.Foldable;

using Content.Shared._Stories.Cards.Stack;
using Content.Shared._Stories.Cards.Fan;

namespace Content.Client._Stories.Cards.Fan;
public sealed class CardFanSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardFanComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }
    private void OnAppearanceChanged(EntityUid uid, CardFanComponent comp, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetVisible(0, false);
        UpdateStackVisuals(uid, comp, sprite);
        if (_appearance.TryGetData<bool>(uid, CardStackVisuals.Shuffled, out var shuffled))
        {
            if (shuffled)
                _appearance.SetData(uid, CardStackVisuals.Shuffled, false);
        }
    }
    public void UpdateStackVisuals(EntityUid uid, CardFanComponent comp, SpriteComponent sprite)
    {
        if (!TryComp<CardStackComponent>(uid, out var cardStack))
            return;
        while (sprite.AllLayers.Count() > 1)
        {
            sprite.RemoveLayer(1);
        }
        var processedLayers = new HashSet<Robust.Client.Graphics.RSI.StateId>();
        var layerIndex = 1;
        var r = 0.2f;

        var maxCardsInFan = 10;
        var angleStart = 135f;
        var angleEnd = 225f;

        var totalCards = Math.Min(maxCardsInFan, cardStack.Cards.Count);

        foreach (var card in cardStack.Cards.Take(totalCards))
        {
            if (!TryComp<SpriteComponent>(card, out var cardSprite) ||
                !TryComp<FoldableComponent>(card, out var foldable))
                return;

            var cardLayer = foldable.IsFolded ? cardSprite.LayerGetState(1) : cardSprite.LayerGetState(0);

            processedLayers.Add(cardLayer);

            var layer = sprite.AddLayer(cardLayer);

            var cardIndex = layerIndex - 1;
            var totalProgress = (float)cardIndex / (totalCards - 1);

            var curAngle = MathHelper.Lerp(angleStart, angleEnd, totalProgress);
            if (totalCards == 1)
            {
                curAngle = (angleStart + angleEnd) / 2;
            }
            else
            {
                curAngle = MathHelper.Lerp(angleStart, angleEnd, totalProgress);
            }
            var normX = r * MathF.Sin(curAngle * MathF.PI / 180);
            var normY = r * MathF.Cos(curAngle * MathF.PI / 180);

            if (TryComp<CardFanComponent>(uid, out var _))
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
