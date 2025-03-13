using Robust.Client.GameObjects;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Cabinet;
using Content.Shared.Foldable;
using System.Numerics;
using System.Linq;

using Content.Shared._Stories.Cards.Deck;
using Content.Shared._Stories.Cards.Stack;

namespace Content.Client._Stories.Cards.Deck;

public sealed class CardDeckSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardDeckComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<CardDeckBoxComponent, AppearanceChangeEvent>(OnAppearanceBoxChanged);
        SubscribeLocalEvent<CardDeckBoxComponent, OpenableOpenedEvent>(OnOpenedBox);
    }

    private void OnAppearanceChanged(EntityUid uid, CardDeckComponent comp, ref AppearanceChangeEvent args)
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

    public void UpdateStackVisuals(EntityUid uid, CardDeckComponent comp, SpriteComponent sprite)
    {
        if (!TryComp<CardStackComponent>(uid, out var cardStack))
            return;
        while (sprite.AllLayers.Count() > 1)
            sprite.RemoveLayer(1);

        var processedLayers = new HashSet<Robust.Client.Graphics.RSI.StateId>();
        var offset = 0.02f;
        var layerIndex = 1;
        var maxCardsInDeck = 5;

        var totalCards = Math.Min(maxCardsInDeck, cardStack.Cards.Count);
        if (totalCards == 0)
            return;

        foreach (var card in cardStack.Cards.Take(totalCards))
        {
            if (!TryComp<SpriteComponent>(card, out var cardSprite) ||
                !TryComp<FoldableComponent>(card, out var foldable))
                return;

            var cardLayer = foldable.IsFolded ? cardSprite.LayerGetState(1) : cardSprite.LayerGetState(0);
            processedLayers.Add(cardLayer);
            var layer = sprite.AddLayer(cardLayer);
            var cardIndex = layerIndex - 1;

            sprite.LayerSetOffset(layer, new Vector2(0, offset * layerIndex));
            sprite.LayerSetRotation(layer, Angle.FromDegrees(90));
            sprite.LayerSetVisible(layer, true);
            layerIndex++;
        }
    }
    private void OnOpenedBox(EntityUid uid, CardDeckBoxComponent comp, OpenableOpenedEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var _))
            return;
        if (!TryComp<ItemCabinetComponent>(uid, out var _))
            return;
        if (!TryComp<OpenableComponent>(uid, out var openable))
            return;

        if (!_appearance.TryGetData<bool>(uid, ItemCabinetVisuals.ContainsItem, out var value))
            return;

        if (value)
        {
            _appearance.SetData(uid, CardDeckVisuals.InBox, true);
        }
        else
        {
            _appearance.SetData(uid, CardDeckVisuals.InBox, false);
        }
    }

    private void OnAppearanceBoxChanged(EntityUid uid, CardDeckBoxComponent comp, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        if (!_appearance.TryGetData<bool>(uid, ItemCabinetVisuals.ContainsItem, out var containsItem))
            return;
        if (!_appearance.TryGetData<bool>(uid, OpenableVisuals.Opened, out var opened))
            return;

        if (containsItem && opened)
        {
            sprite.LayerSetVisible(1, true);
        }
        else
        {
            sprite.LayerSetVisible(1, false);
        }
    }
}
